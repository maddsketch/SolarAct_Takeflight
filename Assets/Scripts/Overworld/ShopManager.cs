using UnityEngine;

// Scene-level singleton. Lives in the Overworld scene.
// Drives the shop session and mediates between ShopInteractable and ShopUI.
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public bool IsOpen { get; private set; }
    public ShopDefinition Current { get; private set; }

    public event System.Action<ShopDefinition> onShopOpen;
    public event System.Action onShopClose;
    public event System.Action<ItemDefinition> onPurchaseSucceeded;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenShop(ShopDefinition shop)
    {
        if (IsOpen || shop == null) return;

        Current = shop;
        IsOpen = true;
        onShopOpen?.Invoke(shop);
    }

    public void CloseShop()
    {
        if (!IsOpen) return;

        IsOpen = false;
        Current = null;
        onShopClose?.Invoke();
    }

    // Returns true and deducts currency if the player can afford the item and meets the level requirement.
    public bool TryBuy(string itemID)
    {
        if (!IsOpen || InventoryManager.Instance == null) return false;

        var def = InventoryManager.Instance.GetDefinition(itemID);
        if (def == null) return false;

        int playerLevel = XPManager.Instance?.PlayerLevel ?? 1;
        if (playerLevel < def.requiredLevel)
        {
            Debug.Log($"[ShopManager] '{itemID}' requires level {def.requiredLevel} (player is {playerLevel}).");
            return false;
        }

        if (!InventoryManager.Instance.SpendCurrency(def.shopPrice))
        {
            Debug.Log($"[ShopManager] Can't afford '{itemID}' (price: {def.shopPrice}).");
            return false;
        }

        bool added = InventoryManager.Instance.AddItem(itemID);
        if (!added)
        {
            // Refund if inventory rejected it (e.g. already at max stack).
            InventoryManager.Instance.AddCurrency(def.shopPrice);
            Debug.Log($"[ShopManager] Could not add '{itemID}' to inventory — refunded.");
            return false;
        }

        onPurchaseSucceeded?.Invoke(def);
        return true;
    }
}
