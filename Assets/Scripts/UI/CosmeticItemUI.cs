using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CosmeticItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI equipButtonText;

    private string itemID;
    private CosmeticType cosmeticType;
    private System.Action<string, CosmeticType> onEquipRequested;

    public void Populate(ItemDefinition def, bool isEquipped, System.Action<string, CosmeticType> equipCallback)
    {
        itemID = def.itemID;
        cosmeticType = def.cosmeticType;
        onEquipRequested = equipCallback;

        if (icon != null) icon.sprite = def.icon;
        if (nameText != null) nameText.text = def.displayName;
        if (descriptionText != null) descriptionText.text = def.description;

        RefreshEquipState(isEquipped);

        equipButton.onClick.RemoveAllListeners();
        equipButton.onClick.AddListener(OnEquipClicked);
    }

    public void RefreshEquipState(bool isEquipped)
    {
        if (equipButtonText != null)
            equipButtonText.text = isEquipped ? "Equipped" : "Equip";

        equipButton.interactable = !isEquipped;
    }

    private void OnEquipClicked()
    {
        onEquipRequested?.Invoke(itemID, cosmeticType);
    }
}
