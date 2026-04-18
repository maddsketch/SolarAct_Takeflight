using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Full achievement list panel — open from pause menu or main menu.
// Instantiates one row per achievement. Locked hidden achievements show ???
public class AchievementMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform  rowParent;
    [SerializeField] private GameObject rowPrefab;       // AchievementRowUI prefab
    [SerializeField] private Button     closeButton;

    public bool IsOpen { get; private set; }

    void Start()
    {
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    public void Open()
    {
        IsOpen = true;
        panel.SetActive(true);
        BuildList();
    }

    public void Close()
    {
        IsOpen = false;
        panel.SetActive(false);
    }

    private void BuildList()
    {
        foreach (Transform child in rowParent)
            Destroy(child.gameObject);

        var manager = AchievementManager.Instance;
        if (manager == null) return;

        foreach (var achievement in manager.GetAll())
        {
            bool unlocked = manager.IsUnlocked(achievement.achievementID);
            bool visible  = unlocked || !achievement.isHidden;

            var go  = Instantiate(rowPrefab, rowParent);
            var row = go.GetComponent<AchievementRowUI>();
            if (row == null) continue;

            if (visible)
                row.Populate(achievement.icon, achievement.displayName, achievement.description, unlocked);
            else
                row.PopulateHidden();
        }
    }
}
