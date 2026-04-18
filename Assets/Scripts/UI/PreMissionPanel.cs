using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Overlay panel shown when the player interacts with a ZoneTrigger.
// Lets the player swap weapon/consumable before launching the shmup level.
public class PreMissionPanel : MonoBehaviour
{
    public static PreMissionPanel Instance { get; private set; }

    [Header("Mission Info")]
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private Image[] difficultySlots;
    [SerializeField] private Sprite filledDifficultySprite;
    [SerializeField] private Sprite emptyDifficultySprite;
    [SerializeField] private TextMeshProUGUI briefingText;
    [SerializeField] private TextMeshProUGUI rewardsText;

    [Header("Loadout — Weapon")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Button prevWeaponButton;
    [SerializeField] private Button nextWeaponButton;

    [Header("Loadout — Secondary weapon")]
    [SerializeField] private TextMeshProUGUI secondaryWeaponNameText;
    [SerializeField] private Button prevSecondaryWeaponButton;
    [SerializeField] private Button nextSecondaryWeaponButton;

    [Header("Loadout — Consumable")]
    [SerializeField] private TextMeshProUGUI consumableNameText;
    [SerializeField] private TextMeshProUGUI consumableQtyText;
    [SerializeField] private Button prevConsumableButton;
    [SerializeField] private Button nextConsumableButton;

    [Header("Actions")]
    [SerializeField] private Button flyOutButton;
    [SerializeField] private Button cancelButton;

    private LevelDefinition currentLevel;
    private List<ItemDefinition> availableWeapons    = new();
    private List<ItemDefinition> availableSecondaryWeapons = new();
    private List<ItemDefinition> availableConsumables = new();
    private int weaponIndex;
    private int secondaryWeaponIndex;
    private int consumableIndex;

    // ---------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        prevWeaponButton.onClick.AddListener(() => CycleWeapon(-1));
        nextWeaponButton.onClick.AddListener(() => CycleWeapon(1));
        prevConsumableButton.onClick.AddListener(() => CycleConsumable(-1));
        nextConsumableButton.onClick.AddListener(() => CycleConsumable(1));
        flyOutButton.onClick.AddListener(OnFlyOut);
        cancelButton.onClick.AddListener(OnCancel);

        if (prevSecondaryWeaponButton != null)
            prevSecondaryWeaponButton.onClick.AddListener(() => CycleSecondaryWeapon(-1));
        if (nextSecondaryWeaponButton != null)
            nextSecondaryWeaponButton.onClick.AddListener(() => CycleSecondaryWeapon(1));
    }

    // ---------------------------------------------------------------
    // Public API

    public void Open(LevelDefinition level)
    {
        currentLevel = level;
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        PopulateMissionInfo();
        PopulateLoadout();
    }

    public bool IsOpen => gameObject.activeSelf;

    // ---------------------------------------------------------------
    // Mission info

    private void PopulateMissionInfo()
    {
        if (missionNameText != null)
            missionNameText.text = currentLevel.displayName;

        for (int i = 0; i < difficultySlots.Length; i++)
        {
            if (difficultySlots[i] != null)
                difficultySlots[i].sprite = i < currentLevel.difficulty
                    ? filledDifficultySprite
                    : emptyDifficultySprite;
        }

        if (briefingText != null)
            briefingText.text = currentLevel.briefingText;

        if (rewardsText != null)
        {
            var tiers = currentLevel.killPerformanceTiers;
            if (tiers != null && tiers.Length > 0)
            {
                var sorted = new List<KillPerformanceTier>(tiers);
                sorted.Sort((a, b) => a.minKills.CompareTo(b.minKills));
                var sb = new StringBuilder();
                sb.AppendLine("Rewards (best tier you qualify for):");
                foreach (var t in sorted)
                    sb.AppendLine($"  {t.minKills}+ kills: {t.credits} Credits · {t.xp} XP");
                rewardsText.text = sb.ToString().TrimEnd();
            }
            else
                rewardsText.text = $"Rewards:  {currentLevel.creditReward} Credits   ·   {currentLevel.xpReward} XP";
        }
    }

    // ---------------------------------------------------------------
    // Loadout

    private void PopulateLoadout()
    {
        availableWeapons.Clear();
        availableConsumables.Clear();

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        // Collect owned weapons and consumables
        foreach (var entry in GameStateManager.Instance.Current.inventory)
        {
            var def = inv.GetDefinition(entry.itemID);
            if (def == null) continue;

            if (def.category == ItemCategory.Weapon)
                availableWeapons.Add(def);
            else if (def.category == ItemCategory.Consumable)
                availableConsumables.Add(def);
        }

        // Set index to currently equipped
        weaponIndex = Mathf.Max(0, availableWeapons.FindIndex(
            d => d.itemID == GameStateManager.Instance.Current.equippedWeaponID));

        consumableIndex = Mathf.Max(0, availableConsumables.FindIndex(
            d => d.itemID == GameStateManager.Instance.Current.equippedConsumableID));

        RebuildSecondaryWeaponChoices();
        RefreshWeaponDisplay();
        RefreshSecondaryWeaponDisplay();
        RefreshConsumableDisplay();
    }

    private void RebuildSecondaryWeaponChoices()
    {
        string primaryId = GameStateManager.Instance.Current.equippedWeaponID;
        string secondaryId = GameStateManager.Instance.Current.equippedSecondaryWeaponID;
        if (!string.IsNullOrEmpty(secondaryId) && secondaryId == primaryId)
            InventoryManager.Instance?.EquipSecondaryWeapon(null);

        availableSecondaryWeapons.Clear();
        foreach (var w in availableWeapons)
        {
            if (!string.IsNullOrEmpty(primaryId) && w.itemID == primaryId)
                continue;
            availableSecondaryWeapons.Add(w);
        }

        secondaryId = GameStateManager.Instance.Current.equippedSecondaryWeaponID;
        if (!string.IsNullOrEmpty(secondaryId) &&
            availableSecondaryWeapons.FindIndex(d => d.itemID == secondaryId) < 0)
            InventoryManager.Instance?.EquipSecondaryWeapon(null);

        secondaryWeaponIndex = Mathf.Max(0, availableSecondaryWeapons.FindIndex(
            d => d.itemID == GameStateManager.Instance.Current.equippedSecondaryWeaponID));

        if (availableSecondaryWeapons.Count == 0)
            InventoryManager.Instance?.EquipSecondaryWeapon(null);
    }

    private void CycleWeapon(int dir)
    {
        if (availableWeapons.Count == 0) return;
        weaponIndex = (weaponIndex + dir + availableWeapons.Count) % availableWeapons.Count;
        InventoryManager.Instance.EquipWeapon(availableWeapons[weaponIndex].itemID);
        RebuildSecondaryWeaponChoices();
        RefreshWeaponDisplay();
        RefreshSecondaryWeaponDisplay();
    }

    private void CycleSecondaryWeapon(int dir)
    {
        if (availableSecondaryWeapons.Count == 0) return;
        secondaryWeaponIndex = (secondaryWeaponIndex + dir + availableSecondaryWeapons.Count) % availableSecondaryWeapons.Count;
        InventoryManager.Instance.EquipSecondaryWeapon(availableSecondaryWeapons[secondaryWeaponIndex].itemID);
        RefreshSecondaryWeaponDisplay();
    }

    private void CycleConsumable(int dir)
    {
        if (availableConsumables.Count == 0) return;
        consumableIndex = (consumableIndex + dir + availableConsumables.Count) % availableConsumables.Count;
        InventoryManager.Instance.EquipConsumable(availableConsumables[consumableIndex].itemID);
        RefreshConsumableDisplay();
    }

    private void RefreshWeaponDisplay()
    {
        if (weaponNameText == null) return;
        weaponNameText.text = availableWeapons.Count > 0
            ? availableWeapons[weaponIndex].displayName
            : "None";

        bool multi = availableWeapons.Count > 1;
        prevWeaponButton.gameObject.SetActive(multi);
        nextWeaponButton.gameObject.SetActive(multi);
    }

    private void RefreshSecondaryWeaponDisplay()
    {
        if (secondaryWeaponNameText == null) return;

        if (availableSecondaryWeapons.Count == 0)
        {
            secondaryWeaponNameText.text = "None";
            if (prevSecondaryWeaponButton != null) prevSecondaryWeaponButton.gameObject.SetActive(false);
            if (nextSecondaryWeaponButton != null) nextSecondaryWeaponButton.gameObject.SetActive(false);
            return;
        }

        secondaryWeaponNameText.text = availableSecondaryWeapons[secondaryWeaponIndex].displayName;
        bool multi = availableSecondaryWeapons.Count > 1;
        if (prevSecondaryWeaponButton != null) prevSecondaryWeaponButton.gameObject.SetActive(multi);
        if (nextSecondaryWeaponButton != null) nextSecondaryWeaponButton.gameObject.SetActive(multi);
    }

    private void RefreshConsumableDisplay()
    {
        if (consumableNameText == null) return;

        if (availableConsumables.Count > 0)
        {
            var def = availableConsumables[consumableIndex];
            consumableNameText.text = def.displayName;

            if (consumableQtyText != null)
            {
                int qty = InventoryManager.Instance.GetQuantity(def.itemID);
                consumableQtyText.text = $"x{qty}";
            }
        }
        else
        {
            consumableNameText.text = "None";
            if (consumableQtyText != null) consumableQtyText.text = "";
        }

        bool multi = availableConsumables.Count > 1;
        prevConsumableButton.gameObject.SetActive(multi);
        nextConsumableButton.gameObject.SetActive(multi);
    }

    // ---------------------------------------------------------------
    // Actions

    private void OnFlyOut()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        GameStateManager.Instance.CaptureOverworldPosition(
            GameObject.FindWithTag("Player")?.transform.position ?? Vector3.zero);
        SceneTransitionManager.Instance.LoadShmupLevel(currentLevel);
    }

    private void OnCancel()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
