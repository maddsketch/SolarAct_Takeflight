using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the Shmup HUD Canvas.
// Shows hearts for health display, similar to HubUI but for shmup scenes.
public class ShmupHealthUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;   // Image with full/empty sprite swap
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    [Header("Optional Stats")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI killCountText;

    [Header("Weapon")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponAmmoText;
    [Tooltip("Shown when a secondary weapon exists; indicates Tab / LB to switch.")]
    [SerializeField] private TextMeshProUGUI secondaryWeaponHintText;
    [Tooltip("Primary stash while the secondary weapon is active (finite primary only).")]
    [SerializeField] private TextMeshProUGUI primaryReserveAmmoText;

    [Header("Low Health Warning")]
    [SerializeField] private int lowHealthThreshold = 2;
    [SerializeField] private Image lowHealthWarningImage;
    [SerializeField] private Color lowHealthFlashColor = Color.red;
    [SerializeField] private float lowHealthPulseSpeed = 8f;

    [Header("Avatar Animator")]
    [SerializeField] private Animator avatarAnimator;
    [SerializeField] private string onHitTriggerName = "OnHit";
    [SerializeField] private string isLowHealthBoolName = "IsLowHealth";
    [SerializeField] private string onKillTierReachedTriggerName = "OnKillTierReached";
    [SerializeField] private LevelDefinition levelDefinitionOverride;

    private Health playerHealth;
    private WeaponController playerWeapon;
    private Coroutine lowHealthPulseRoutine;
    private bool isLowHealthActive;
    private int[] runtimeKillTierThresholds;
    private int nextKillTierIndex;

    IEnumerator Start()
    {
        // Wait two frames so all Awake/Start calls finish before we read Health values
        yield return null;
        yield return null;

        var player = GameObject.FindWithTag("Player");
        playerHealth = player?.GetComponentInChildren<Health>();
        playerWeapon = player != null ? player.GetComponent<WeaponController>() : null;
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(OnHealthChanged);
            playerHealth.onDamaged.AddListener(OnDamaged);
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;

        if (GameManager.Instance != null)
            GameManager.Instance.onKillCountChanged += OnKillCountChanged;

        if (playerWeapon != null)
            playerWeapon.OnAmmoChanged += OnWeaponAmmoChanged;

        InitializeKillTierThresholds();
        SetLowHealthVisuals(false, 0f);
        Refresh();
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
            playerHealth.onDamaged.RemoveListener(OnDamaged);
        }

        if (playerWeapon != null)
            playerWeapon.OnAmmoChanged -= OnWeaponAmmoChanged;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;

        if (GameManager.Instance != null)
            GameManager.Instance.onKillCountChanged -= OnKillCountChanged;

        StopLowHealthPulse();
        SetLowHealthVisuals(false, 0f);
    }

    private void Refresh()
    {
        if (levelText != null && XPManager.Instance != null)
            levelText.text = $"LVL {XPManager.Instance.PlayerLevel}";

        if (currencyText != null && InventoryManager.Instance != null)
            currencyText.text = $"{InventoryManager.Instance.Currency}";

        if (killCountText != null && GameManager.Instance != null)
            killCountText.text = $"KILLS: {GameManager.Instance.EnemiesKilled}";

        if (playerHealth != null)
            RefreshHearts(playerHealth.Current, playerHealth.Max);

        if (playerHealth != null)
            EvaluateLowHealth(playerHealth.Current);

        RefreshWeaponHud();
    }

    private void RefreshHearts(int current, int max)
    {
        if (heartsContainer == null || heartPrefab == null) return;

        // Spawn or destroy heart objects to match max
        while (heartsContainer.childCount < max)
            Instantiate(heartPrefab, heartsContainer);

        while (heartsContainer.childCount > max)
            DestroyImmediate(heartsContainer.GetChild(0).gameObject);

        // Set each heart full or empty
        for (int i = 0; i < heartsContainer.childCount; i++)
        {
            var img = heartsContainer.GetChild(i).GetComponent<Image>();
            if (img == null) continue;
            img.sprite = i < current ? heartFullSprite : heartEmptySprite;
        }
    }

    private void RefreshWeaponHud()
    {
        if (weaponIcon != null)
        {
            Sprite s = null;
            if (playerWeapon != null && playerWeapon.ActiveWeapon != null)
                s = playerWeapon.ActiveWeapon.icon;

            weaponIcon.sprite = s;
            weaponIcon.enabled = s != null;
        }

        if (weaponAmmoText != null)
        {
            if (playerWeapon != null && playerWeapon.ActiveWeapon != null)
            {
                weaponAmmoText.text = playerWeapon.IsInfiniteAmmo
                    ? "Ammo: ∞"
                    : $"Ammo: {playerWeapon.CurrentAmmo}/{playerWeapon.MaxAmmo}";
            }
            else
                weaponAmmoText.text = string.Empty;
        }

        if (secondaryWeaponHintText != null)
        {
            if (playerWeapon != null && playerWeapon.SecondaryWeapon != null && playerWeapon.PrimaryWeapon != null)
            {
                string nextName = playerWeapon.IsPrimaryActive
                    ? playerWeapon.SecondaryWeapon.displayName
                    : playerWeapon.PrimaryWeapon.displayName;
                secondaryWeaponHintText.text = $"Tab / LB: {nextName}";
            }
            else
                secondaryWeaponHintText.text = string.Empty;
        }

        if (primaryReserveAmmoText != null)
        {
            if (playerWeapon != null &&
                playerWeapon.SecondaryWeapon != null &&
                !playerWeapon.IsPrimaryActive &&
                playerWeapon.PrimaryWeapon != null &&
                !playerWeapon.PrimaryIsInfinite)
            {
                primaryReserveAmmoText.text =
                    $"Reserve: {playerWeapon.PrimaryAmmo}/{playerWeapon.PrimaryMaxAmmo}";
            }
            else
                primaryReserveAmmoText.text = string.Empty;
        }
    }

    private void OnWeaponAmmoChanged()
    {
        RefreshWeaponHud();
    }

    // --- Listeners ---

    private void OnHealthChanged(int current, int max)
    {
        RefreshHearts(current, max);
        EvaluateLowHealth(current);
    }

    private void OnDamaged()
    {
        if (avatarAnimator == null || string.IsNullOrEmpty(onHitTriggerName)) return;
        avatarAnimator.SetTrigger(onHitTriggerName);
    }
    private void OnLevelUp(int newLevel)
    {
        if (levelText != null) levelText.text = $"LVL {newLevel}";
    }
    private void OnInventoryChanged()
    {
        if (currencyText != null && InventoryManager.Instance != null)
            currencyText.text = $"{InventoryManager.Instance.Currency}";
        RefreshWeaponHud();
    }

    private void OnKillCountChanged(int kills)
    {
        if (killCountText != null)
            killCountText.text = $"KILLS: {kills}";

        TryTriggerKillTierAnimation(kills);
    }

    private void EvaluateLowHealth(int currentHealth)
    {
        bool shouldBeLow = currentHealth > 0 && currentHealth <= lowHealthThreshold;
        if (shouldBeLow == isLowHealthActive) return;

        isLowHealthActive = shouldBeLow;
        if (shouldBeLow)
            StartLowHealthPulse();
        else
        {
            StopLowHealthPulse();
            SetLowHealthVisuals(false, 0f);
        }

        if (avatarAnimator != null && !string.IsNullOrEmpty(isLowHealthBoolName))
            avatarAnimator.SetBool(isLowHealthBoolName, shouldBeLow);
    }

    private void StartLowHealthPulse()
    {
        if (lowHealthPulseRoutine != null) return;
        lowHealthPulseRoutine = StartCoroutine(LowHealthPulseRoutine());
    }

    private void StopLowHealthPulse()
    {
        if (lowHealthPulseRoutine == null) return;
        StopCoroutine(lowHealthPulseRoutine);
        lowHealthPulseRoutine = null;
    }

    private IEnumerator LowHealthPulseRoutine()
    {
        while (true)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
            SetLowHealthVisuals(true, pulse);
            yield return null;
        }
    }

    private void SetLowHealthVisuals(bool enabled, float pulse)
    {
        if (lowHealthWarningImage != null)
        {
            lowHealthWarningImage.gameObject.SetActive(enabled);
            Color c = lowHealthWarningImage.color;
            c.a = enabled ? Mathf.Lerp(0.35f, 1f, pulse) : 0f;
            lowHealthWarningImage.color = c;
        }

        if (heartsContainer == null) return;
        Color heartColor = enabled ? Color.Lerp(Color.white, lowHealthFlashColor, pulse) : Color.white;
        for (int i = 0; i < heartsContainer.childCount; i++)
        {
            Image img = heartsContainer.GetChild(i).GetComponent<Image>();
            if (img != null)
                img.color = heartColor;
        }
    }

    private void InitializeKillTierThresholds()
    {
        LevelDefinition level = SceneTransitionManager.Instance?.CurrentLevel ?? levelDefinitionOverride;
        if (level == null || level.killPerformanceTiers == null || level.killPerformanceTiers.Length == 0)
        {
            runtimeKillTierThresholds = System.Array.Empty<int>();
            nextKillTierIndex = 0;
            return;
        }

        var tiers = level.killPerformanceTiers;
        int[] rawThresholds = new int[tiers.Length];
        int count = 0;
        for (int i = 0; i < tiers.Length; i++)
        {
            int threshold = Mathf.Max(0, tiers[i].minKills);
            bool exists = false;
            for (int j = 0; j < count; j++)
            {
                if (rawThresholds[j] == threshold)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                rawThresholds[count++] = threshold;
        }

        runtimeKillTierThresholds = new int[count];
        for (int i = 0; i < count; i++)
            runtimeKillTierThresholds[i] = rawThresholds[i];
        System.Array.Sort(runtimeKillTierThresholds);
        nextKillTierIndex = 0;
    }

    private void TryTriggerKillTierAnimation(int kills)
    {
        if (runtimeKillTierThresholds == null || runtimeKillTierThresholds.Length == 0) return;
        if (avatarAnimator == null || string.IsNullOrEmpty(onKillTierReachedTriggerName)) return;

        while (nextKillTierIndex < runtimeKillTierThresholds.Length &&
               kills >= runtimeKillTierThresholds[nextKillTierIndex])
        {
            avatarAnimator.SetTrigger(onKillTierReachedTriggerName);
            nextKillTierIndex++;
        }
    }
}