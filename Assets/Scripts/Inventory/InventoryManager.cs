using System;
using System.Collections.Generic;
using UnityEngine;

// Provides inventory operations over the active SaveData.
// Relies on GameStateManager.Instance.Current being valid.
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Fired whenever the inventory contents or currency change.
    public event Action OnInventoryChanged;

    // Fired when equipped cosmetics change (hull, skin, or accessories).
    public event Action OnCosmeticChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Item registry ---

    // Load all ItemDefinitions from Resources/Items at startup.
    private Dictionary<string, ItemDefinition> _registry;

    void Start()
    {
        BuildRegistry();
    }

    private void BuildRegistry()
    {
        _registry = new Dictionary<string, ItemDefinition>();
        foreach (var def in Resources.LoadAll<ItemDefinition>("Items"))
        {
            if (string.IsNullOrEmpty(def.itemID))
            {
                Debug.LogWarning($"[InventoryManager] ItemDefinition '{def.name}' has no itemID — skipping.");
                continue;
            }
            _registry[def.itemID] = def;
        }
        Debug.Log($"[InventoryManager] Registered {_registry.Count} items.");
    }

    public ItemDefinition GetDefinition(string itemID)
    {
        _registry.TryGetValue(itemID, out var def);
        return def;
    }

    // --- Queries ---

    private List<InventoryEntry> Inventory => GameStateManager.Instance.Current.inventory;

    public bool HasItem(string itemID, int qty = 1)
    {
        int idx = FindIndex(itemID);
        return idx >= 0 && Inventory[idx].quantity >= qty;
    }

    public int GetQuantity(string itemID)
    {
        int idx = FindIndex(itemID);
        return idx >= 0 ? Inventory[idx].quantity : 0;
    }

    // --- Mutations ---

    // Returns false if the item would exceed maxStackSize.
    public bool AddItem(string itemID, int qty = 1)
    {
        var def = GetDefinition(itemID);
        if (def == null)
        {
            Debug.LogWarning($"[InventoryManager] AddItem: unknown itemID '{itemID}'.");
            return false;
        }

        int idx = FindIndex(itemID);
        if (idx >= 0)
        {
            var entry = Inventory[idx];
            if (entry.quantity + qty > def.maxStackSize)
            {
                Debug.Log($"[InventoryManager] {itemID} would exceed maxStackSize ({def.maxStackSize}).");
                return false;
            }
            entry.quantity += qty;
            Inventory[idx] = entry;
        }
        else
        {
            if (qty > def.maxStackSize) return false;
            Inventory.Add(new InventoryEntry { itemID = itemID, quantity = qty });
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Returns false if the player doesn't have enough.
    public bool RemoveItem(string itemID, int qty = 1)
    {
        int idx = FindIndex(itemID);
        if (idx < 0 || Inventory[idx].quantity < qty)
        {
            Debug.Log($"[InventoryManager] RemoveItem: not enough '{itemID}'.");
            return false;
        }

        var entry = Inventory[idx];
        entry.quantity -= qty;
        if (entry.quantity == 0)
            Inventory.RemoveAt(idx);
        else
            Inventory[idx] = entry;

        OnInventoryChanged?.Invoke();
        return true;
    }

    // --- Weapon equip ---

    public string EquippedWeaponID => GameStateManager.Instance.Current.equippedWeaponID;
    public string EquippedSecondaryWeaponID => GameStateManager.Instance.Current.equippedSecondaryWeaponID;

    // Equip a weapon from inventory. Pass null to unequip.
    public bool EquipWeapon(string itemID)
    {
        if (itemID != null && !HasItem(itemID))
        {
            Debug.LogWarning($"[InventoryManager] EquipWeapon: '{itemID}' not in inventory.");
            return false;
        }

        var oldDef = GetDefinition(GameStateManager.Instance.Current.equippedWeaponID);
        var newDef = itemID != null ? GetDefinition(itemID) : null;

        GameStateManager.Instance.Current.equippedWeaponID = itemID;
        OnInventoryChanged?.Invoke();

        bool cosmeticRelevant = (oldDef != null && oldDef.includesCosmetic)
                             || (newDef != null && newDef.includesCosmetic);
        if (cosmeticRelevant)
            OnCosmeticChanged?.Invoke();

        return true;
    }

    // Equip secondary shmup weapon from inventory. Pass null to unequip.
    public bool EquipSecondaryWeapon(string itemID)
    {
        if (itemID != null && !HasItem(itemID))
        {
            Debug.LogWarning($"[InventoryManager] EquipSecondaryWeapon: '{itemID}' not in inventory.");
            return false;
        }

        var oldDef = GetDefinition(GameStateManager.Instance.Current.equippedSecondaryWeaponID);
        var newDef = itemID != null ? GetDefinition(itemID) : null;

        GameStateManager.Instance.Current.equippedSecondaryWeaponID = itemID;
        OnInventoryChanged?.Invoke();

        bool cosmeticRelevant = (oldDef != null && oldDef.includesCosmetic)
                             || (newDef != null && newDef.includesCosmetic);
        if (cosmeticRelevant)
            OnCosmeticChanged?.Invoke();

        return true;
    }

    // --- Consumable equip ---

    public string EquippedConsumableID => GameStateManager.Instance.Current.equippedConsumableID;

    // Equip a consumable from inventory. Pass null to unequip.
    public bool EquipConsumable(string itemID)
    {
        if (itemID != null && !HasItem(itemID))
        {
            Debug.LogWarning($"[InventoryManager] EquipConsumable: '{itemID}' not in inventory.");
            return false;
        }

        GameStateManager.Instance.Current.equippedConsumableID = itemID;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // --- Cosmetic equip ---

    public string EquippedHullID => GameStateManager.Instance.Current.equippedHullID;
    public string EquippedSkinID => GameStateManager.Instance.Current.equippedSkinID;
    public List<string> EquippedAccessoryIDs => GameStateManager.Instance.Current.equippedAccessoryIDs;

    public bool EquipHull(string itemID)
    {
        if (itemID != null && !HasItem(itemID))
        {
            Debug.LogWarning($"[InventoryManager] EquipHull: '{itemID}' not in inventory.");
            return false;
        }

        GameStateManager.Instance.Current.equippedHullID = itemID;
        OnCosmeticChanged?.Invoke();
        return true;
    }

    public bool EquipSkin(string itemID)
    {
        if (itemID != null && !HasItem(itemID))
        {
            Debug.LogWarning($"[InventoryManager] EquipSkin: '{itemID}' not in inventory.");
            return false;
        }

        GameStateManager.Instance.Current.equippedSkinID = itemID;
        OnCosmeticChanged?.Invoke();
        return true;
    }

    public bool EquipAccessory(string itemID)
    {
        if (itemID == null || !HasItem(itemID)) return false;

        var ids = GameStateManager.Instance.Current.equippedAccessoryIDs;
        if (!ids.Contains(itemID))
            ids.Add(itemID);

        OnCosmeticChanged?.Invoke();
        return true;
    }

    public void UnequipAccessory(string itemID)
    {
        GameStateManager.Instance.Current.equippedAccessoryIDs.Remove(itemID);
        OnCosmeticChanged?.Invoke();
    }

    // --- Currency ---

    public int Currency => GameStateManager.Instance.Current.currency;

    public void AddCurrency(int amount)
    {
        GameStateManager.Instance.Current.currency += amount;
        AchievementManager.Instance?.RegisterCurrencyEarned(amount);
        OnInventoryChanged?.Invoke();
    }

    // Returns false if the player can't afford it.
    public bool SpendCurrency(int amount)
    {
        if (GameStateManager.Instance.Current.currency < amount)
            return false;

        GameStateManager.Instance.Current.currency -= amount;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // --- Helpers ---

    private int FindIndex(string itemID)
    {
        for (int i = 0; i < Inventory.Count; i++)
            if (Inventory[i].itemID == itemID) return i;
        return -1;
    }
}
