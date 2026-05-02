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
    [SerializeField] private Button actionButton;           // "Equip" for weapons/consumables/cosmetics
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [Header("Disabled button look")]
    [SerializeField] private Color actionButtonDisabledGraphic = new Color(0.18f, 0.18f, 0.2f, 1f);
    [SerializeField] private Color actionButtonTextDisabledColor = new Color(0.42f, 0.42f, 0.45f, 1f);

    private string itemID;
    private ItemCategory category;
    private CosmeticType cosmeticType;
    private ItemDefinition cachedDef;
    private Color actionButtonTextEnabledColor = Color.white;
    private bool capturedActionButtonTextColor;

    void Awake()
    {
        ApplyActionButtonDisabledColors();
    }

    public void Populate(ItemDefinition def, int quantity)
    {
        cachedDef = def;
        itemID   = def.itemID;
        category = def.category;
        cosmeticType = def.category == ItemCategory.Cosmetic ? def.cosmeticType : default;

        if (icon != null)     icon.sprite = def.icon;
        if (nameText != null) nameText.text = def.displayName;

        // Quantity only meaningful for stackable items.
        if (quantityText != null)
            quantityText.text = def.maxStackSize > 1 ? $"x{quantity}" : string.Empty;

        // Upgrades are passive — no action button needed.
        bool hasAction = category == ItemCategory.Weapon
            || category == ItemCategory.Consumable
            || category == ItemCategory.Cosmetic;
        if (actionButton != null) actionButton.gameObject.SetActive(hasAction);

        if (hasAction)
        {
            string label = category switch
            {
                ItemCategory.Weapon => "Equip",
                ItemCategory.Consumable => "Select",
                ItemCategory.Cosmetic => "Equip",
                _ => "Equip"
            };
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

        bool equipped = false;
        if (category == ItemCategory.Weapon)
        {
            equipped = InventoryManager.Instance.EquippedWeaponID == itemID
                || InventoryManager.Instance.EquippedSecondaryWeaponID == itemID;
        }
        else if (category == ItemCategory.Consumable)
        {
            equipped = InventoryManager.Instance.EquippedConsumableID == itemID;
        }
        else if (category == ItemCategory.Cosmetic)
        {
            equipped = cosmeticType switch
            {
                CosmeticType.Hull => InventoryManager.Instance.EquippedHullID == itemID,
                CosmeticType.Skin => InventoryManager.Instance.EquippedSkinID == itemID,
                CosmeticType.Accessory => InventoryManager.Instance.EquippedAccessoryIDs.Contains(itemID),
                _ => false
            };
        }

        bool primaryEquipped = category == ItemCategory.Weapon
            && InventoryManager.Instance.EquippedWeaponID == itemID;

        bool showEquippedBadge = equipped
            && (category == ItemCategory.Weapon
                || category == ItemCategory.Consumable
                || category == ItemCategory.Cosmetic);

        if (equippedLabel != null)
        {
            equippedLabel.gameObject.SetActive(showEquippedBadge);
            if (showEquippedBadge)
                equippedLabel.text = category == ItemCategory.Consumable ? "Selected" : "Equipped";
        }

        if (actionButton == null || !actionButton.gameObject.activeSelf)
            return;

        if (category == ItemCategory.Weapon)
            actionButton.interactable = !primaryEquipped;
        else if (category == ItemCategory.Cosmetic || category == ItemCategory.Consumable)
            actionButton.interactable = !equipped;
        else
            actionButton.interactable = true;

        if (actionButtonText != null)
        {
            if (!capturedActionButtonTextColor)
            {
                actionButtonTextEnabledColor = actionButtonText.color;
                capturedActionButtonTextColor = true;
            }
            actionButtonText.color = actionButton.interactable
                ? actionButtonTextEnabledColor
                : actionButtonTextDisabledColor;
        }
    }

    private void ApplyActionButtonDisabledColors()
    {
        if (actionButton == null) return;

        var colors = actionButton.colors;
        colors.disabledColor = actionButtonDisabledGraphic;
        actionButton.colors = colors;
    }

    private void OnActionClicked()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        bool ok = false;
        if (category == ItemCategory.Weapon)
            ok = inv.EquipWeapon(itemID);
        else if (category == ItemCategory.Consumable)
            ok = inv.EquipConsumable(itemID);
        else if (category == ItemCategory.Cosmetic)
        {
            switch (cosmeticType)
            {
                case CosmeticType.Hull:      ok = inv.EquipHull(itemID); break;
                case CosmeticType.Skin:      ok = inv.EquipSkin(itemID); break;
                case CosmeticType.Accessory: ok = inv.EquipAccessory(itemID); break;
            }
        }

        if (ok)
            InventoryUI.Instance?.NotifyEquipSucceeded(cachedDef);
    }
}
