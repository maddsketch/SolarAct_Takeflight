using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One row in the achievement list. AchievementMenuUI populates this.
public class AchievementRowUI : MonoBehaviour
{
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject      lockedOverlay;    // greyed-out overlay shown when locked
    [SerializeField] private GameObject      unlockedBadge;    // tick/checkmark shown when unlocked

    public void Populate(Sprite icon, string displayName, string description, bool unlocked)
    {
        if (iconImage       != null) iconImage.sprite      = icon;
        if (nameText        != null) nameText.text         = displayName;
        if (descriptionText != null) descriptionText.text  = description;
        if (lockedOverlay   != null) lockedOverlay.SetActive(!unlocked);
        if (unlockedBadge   != null) unlockedBadge.SetActive(unlocked);
    }

    public void PopulateHidden()
    {
        if (iconImage       != null) iconImage.sprite     = null;
        if (nameText        != null) nameText.text        = "???";
        if (descriptionText != null) descriptionText.text = "???";
        if (lockedOverlay   != null) lockedOverlay.SetActive(true);
        if (unlockedBadge   != null) unlockedBadge.SetActive(false);
    }
}
