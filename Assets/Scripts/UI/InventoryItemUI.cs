using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the root of the inventory item row prefab.
// InventoryUI instantiates one per item in the player's inventory.
public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI equippedLabel; // "EQUIPPED" badge
    [SerializeField] private Button actionButton;           // "Equip" for weapons/consumables
    [SerializeField] private TextMeshProUGUI actionButtonText;

    private string itemID;
    private ItemCategory category;

    public void Populate(ItemDefinition def, int quantity)
    {
        itemID   = def.itemID;
        category = def.category;

        if (icon != null)     icon.sprite = def.icon;
        if (nameText != null) nameText.text = def.displayName;

        // Quantity only meaningful for stackable items.
        if (quantityText != null)
            quantityText.text = def.maxStackSize > 1 ? $"x{quantity}" : string.Empty;

        // Upgrades are passive — no action button needed.
        bool hasAction = category == ItemCategory.Weapon || category == ItemCategory.Consumable;
        if (actionButton != null) actionButton.gameObject.SetActive(hasAction);

        if (hasAction)
        {
            string label = category == ItemCategory.Weapon ? "Equip" : "Select";
            if (actionButtonText != null) actionButtonText.text = label;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        Refresh();
    }

    // Call whenever equipped state might have changed.
    public void Refresh()
    {
        if (InventoryManager.Instance == null) return;

        bool equipped = category == ItemCategory.Weapon
            ? InventoryManager.Instance.EquippedWeaponID == itemID
                || InventoryManager.Instance.EquippedSecondaryWeaponID == itemID
            : category == ItemCategory.Consumable
                ? InventoryManager.Instance.EquippedConsumableID == itemID
                : false;

        bool primaryEquipped = category == ItemCategory.Weapon
            && InventoryManager.Instance.EquippedWeaponID == itemID;

        if (equippedLabel != null) equippedLabel.gameObject.SetActive(equipped);
        if (actionButton != null) actionButton.interactable = !primaryEquipped;
    }

    private void OnActionClicked()
    {
        if (category == ItemCategory.Weapon)
            InventoryManager.Instance?.EquipWeapon(itemID);
        else if (category == ItemCategory.Consumable)
            InventoryManager.Instance?.EquipConsumable(itemID);
    }
}
