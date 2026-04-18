using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticMenuUI : MonoBehaviour
{
    public static CosmeticMenuUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Category Tabs")]
    [SerializeField] private Button hullTab;
    [SerializeField] private Button skinTab;
    [SerializeField] private Button accessoryTab;

    [Header("Item List")]
    [SerializeField] private Transform itemListParent;
    [SerializeField] private GameObject cosmeticItemPrefab;

    [Header("Actions")]
    [SerializeField] private Button closeButton;

    public bool IsOpen => panel.activeSelf;

    private CosmeticType activeTab = CosmeticType.Hull;
    private readonly List<CosmeticItemUI> rows = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCosmeticChanged -= OnCosmeticChanged;
    }

    void Start()
    {
        hullTab.onClick.AddListener(() => SwitchTab(CosmeticType.Hull));
        skinTab.onClick.AddListener(() => SwitchTab(CosmeticType.Skin));
        accessoryTab.onClick.AddListener(() => SwitchTab(CosmeticType.Accessory));
        closeButton.onClick.AddListener(Close);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCosmeticChanged += OnCosmeticChanged;
    }

    public void Open()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        activeTab = CosmeticType.Hull;
        RebuildList();
    }

    public void Close()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void SwitchTab(CosmeticType tab)
    {
        activeTab = tab;
        RebuildList();
    }

    private void RebuildList()
    {
        ClearList();

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        var data = GameStateManager.Instance?.Current;
        if (data == null) return;

        foreach (var entry in data.inventory)
        {
            var def = inv.GetDefinition(entry.itemID);
            if (def == null || def.category != ItemCategory.Cosmetic) continue;
            if (def.cosmeticType != activeTab) continue;

            var go = Instantiate(cosmeticItemPrefab, itemListParent);
            var row = go.GetComponent<CosmeticItemUI>();
            if (row == null) continue;

            bool equipped = IsEquipped(def);
            row.Populate(def, equipped, OnEquipRequested);
            rows.Add(row);
        }
    }

    private bool IsEquipped(ItemDefinition def)
    {
        var inv = InventoryManager.Instance;
        return def.cosmeticType switch
        {
            CosmeticType.Hull => inv.EquippedHullID == def.itemID,
            CosmeticType.Skin => inv.EquippedSkinID == def.itemID,
            CosmeticType.Accessory => inv.EquippedAccessoryIDs.Contains(def.itemID),
            _ => false
        };
    }

    private void OnEquipRequested(string itemID, CosmeticType type)
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        switch (type)
        {
            case CosmeticType.Hull:      inv.EquipHull(itemID);      break;
            case CosmeticType.Skin:      inv.EquipSkin(itemID);      break;
            case CosmeticType.Accessory: inv.EquipAccessory(itemID); break;
        }
    }

    private void OnCosmeticChanged()
    {
        if (!IsOpen) return;

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        foreach (var row in rows)
        {
            if (row == null) continue;
            // Re-check equipped state by pulling the definition from the row's item
            // Rebuild is simpler and safe since cosmetic changes are infrequent
        }

        RebuildList();
    }

    private void ClearList()
    {
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        rows.Clear();
    }
}
