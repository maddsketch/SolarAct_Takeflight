using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Attach to the HUD canvas in the Hub scene.
// Lists currently active quest names. Refreshes on enable and when quests change.
public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI questListText;
    [SerializeField] private string emptyText = "No active quests.";

    void OnEnable() => Refresh();

    public void Refresh()
    {
        var quests = GameStateManager.Instance?.Current.activeQuestIDs;

        if (quests == null || quests.Count == 0)
        {
            if (questListText != null) questListText.text = emptyText;
            return;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var id in quests)
            sb.AppendLine($"• {FormatQuestID(id)}");

        if (questListText != null) questListText.text = sb.ToString().TrimEnd();
    }

    // Converts "rescue_the_pilot" → "Rescue The Pilot"
    private string FormatQuestID(string id)
    {
        var parts = id.Split('_');
        var result = new System.Text.StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length > 0)
                result.Append(char.ToUpper(part[0]) + part.Substring(1) + " ");
        }
        return result.ToString().TrimEnd();
    }
}
