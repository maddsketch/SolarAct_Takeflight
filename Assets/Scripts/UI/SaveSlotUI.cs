using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to each save slot row in the Save Slot panel.
// MainMenuManager calls Populate() and wires the button.
public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotLabel;      // "Slot 1"
    [SerializeField] private TextMeshProUGUI timestampText;  // save date or "Empty"
    [SerializeField] private TextMeshProUGUI progressText;   // optional: levels cleared
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject deleteConfirmPanel;  // small inline "Are you sure?" group
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;

    private int slotIndex;
    private Action<int> onSelect;

    void Start()
    {
        deleteButton.onClick.AddListener(() => deleteConfirmPanel.SetActive(true));
        cancelDeleteButton.onClick.AddListener(() => deleteConfirmPanel.SetActive(false));
        confirmDeleteButton.onClick.AddListener(OnConfirmDelete);
    }

    public void Populate(int slot, Action<int> selectCallback)
    {
        slotIndex = slot;
        onSelect  = selectCallback;

        if (deleteConfirmPanel != null) deleteConfirmPanel.SetActive(false);

        if (slotLabel != null) slotLabel.text = $"Slot {slot + 1}";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke(slotIndex));

        var save = GameStateManager.Instance?.PeekSave(slot);
        bool hasSave = save != null;

        if (timestampText != null)
            timestampText.text = hasSave ? save.saveTimestamp : "Empty";

        if (progressText != null)
            progressText.text  = hasSave ? $"{save.completedLevels.Count} levels cleared" : string.Empty;

        if (deleteButton != null) deleteButton.gameObject.SetActive(hasSave);
    }

    private void OnConfirmDelete()
    {
        GameStateManager.Instance?.DeleteSave(slotIndex);
        deleteConfirmPanel.SetActive(false);
        Populate(slotIndex, onSelect); // refresh display
    }
}
