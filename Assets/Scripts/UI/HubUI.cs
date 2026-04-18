using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// HUD for Hub and Overworld scenes.
// Shows hearts, player level, currency, and delegates to QuestTrackerUI for quests.
public class HubUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;   // Image with full/empty sprite swap
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Quests")]
    [SerializeField] private QuestTrackerUI questTracker;

    [Header("Low Health Warning")]
    [SerializeField] private int lowHealthThreshold = 2;
    [SerializeField] private Image lowHealthWarningImage;
    [SerializeField] private Color lowHealthFlashColor = Color.red;
    [SerializeField] private float lowHealthPulseSpeed = 8f;

    [Header("Side Panel Slide")]
    [SerializeField] private RectTransform topRightPanel;
    [SerializeField] private float slideSpeed = 800f;
    [SerializeField] private Button toggleButton;

    private Vector2 topRightShownPos;
    private Vector2 topRightHiddenPos;
    private bool topRightVisible;
    private Coroutine slideCoroutine;

    private Health playerHealth;
    private Coroutine lowHealthPulseRoutine;
    private bool isLowHealthActive;

    IEnumerator Start()
    {
        yield return null;
        yield return null;

        var player = GameObject.FindWithTag("Player");
        playerHealth = player?.GetComponentInChildren<Health>();
        if (playerHealth != null)
            playerHealth.onHealthChanged.AddListener(OnHealthChanged);

        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp += OnLevelUp;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;

        if (topRightPanel != null)
        {
            topRightShownPos = topRightPanel.anchoredPosition;
            topRightHiddenPos = topRightShownPos + new Vector2(topRightPanel.rect.width + 20f, 0f);
            topRightPanel.anchoredPosition = topRightHiddenPos;
            topRightVisible = false;
        }

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleTopRight);

        SetLowHealthVisuals(false, 0f);
        Refresh();
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);

        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp -= OnLevelUp;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;

        StopLowHealthPulse();
        SetLowHealthVisuals(false, 0f);
    }

    private void Refresh()
    {
        if (levelText != null && XPManager.Instance != null)
            levelText.text = $"LVL {XPManager.Instance.PlayerLevel}";

        if (currencyText != null && InventoryManager.Instance != null)
            currencyText.text = $"{InventoryManager.Instance.Currency}";

        if (playerHealth != null)
            RefreshHearts(playerHealth.Current, playerHealth.Max);
        if (playerHealth != null)
            EvaluateLowHealth(playerHealth.Current);

        questTracker?.Refresh();
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

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            ToggleTopRight();
    }

    public void ToggleTopRight()
    {
        if (topRightPanel == null) return;

        topRightVisible = !topRightVisible;
        Vector2 target = topRightVisible ? topRightShownPos : topRightHiddenPos;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(target));
    }

    IEnumerator SlideTo(Vector2 target)
    {
        if (slideSpeed <= 0f)
        {
            topRightPanel.anchoredPosition = target;
            slideCoroutine = null;
            yield break;
        }

        while (Vector2.Distance(topRightPanel.anchoredPosition, target) > 0.5f)
        {
            topRightPanel.anchoredPosition = Vector2.MoveTowards(
                topRightPanel.anchoredPosition, target, slideSpeed * Time.deltaTime);
            yield return null;
        }
        topRightPanel.anchoredPosition = target;
        slideCoroutine = null;
    }

    // --- Listeners ---

    private void OnHealthChanged(int current, int max)
    {
        RefreshHearts(current, max);
        EvaluateLowHealth(current);
    }
    private void OnLevelUp(int newLevel)
    {
        if (levelText != null) levelText.text = $"LVL {newLevel}";
    }
    private void OnInventoryChanged()
    {
        if (currencyText != null && InventoryManager.Instance != null)
            currencyText.text = $"{InventoryManager.Instance.Currency}";
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
}
