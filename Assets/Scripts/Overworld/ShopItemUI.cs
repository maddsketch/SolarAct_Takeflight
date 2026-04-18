using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the root of the shop item row prefab.
// ShopUI instantiates one of these per item in the ShopDefinition.
public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI levelRequirementText;  // e.g. "Requires Level 4"
    [SerializeField] private Button buyButton;

    private string itemID;

    public void Populate(ItemDefinition def)
    {
        itemID = def.itemID;

        if (icon != null)            icon.sprite = def.icon;
        if (nameText != null)        nameText.text = def.displayName;
        if (descriptionText != null) descriptionText.text = def.description;
        if (priceText != null)       priceText.text = $"{def.shopPrice} Credits";

        bool locked = def.requiredLevel > 1;
        if (levelRequirementText != null)
        {
            levelRequirementText.gameObject.SetActive(locked);
            levelRequirementText.text = $"Requires Level {def.requiredLevel}";
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    // Called each frame by ShopUI to grey out the button when unaffordable or level-locked.
    public void RefreshAffordability(int playerCurrency, int maxStack)
    {
        var def = InventoryManager.Instance?.GetDefinition(itemID);
        if (def == null) return;

        int  playerLevel   = XPManager.Instance?.PlayerLevel ?? 1;
        bool meetsLevel    = playerLevel >= def.requiredLevel;
        bool canAfford     = playerCurrency >= def.shopPrice;
        bool hasSpace      = InventoryManager.Instance.GetQuantity(itemID) < maxStack;

        buyButton.interactable = meetsLevel && canAfford && hasSpace;

        if (levelRequirementText != null)
            levelRequirementText.gameObject.SetActive(!meetsLevel);
    }

    private void OnBuyClicked()
    {
        ShopManager.Instance?.TryBuy(itemID);
    }
}
