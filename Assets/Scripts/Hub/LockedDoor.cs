using UnityEngine;

// Attach to a door GameObject alongside a Collider (set as non-trigger to block).
// Opens when the required flag is set OR the player has the required item.
public class LockedDoor : Interactable
{
    public enum UnlockMode { Flag, Item }

    [SerializeField] private UnlockMode unlockMode = UnlockMode.Flag;
    [SerializeField] private string requiredFlag;       // used when mode = Flag
    [SerializeField] private string requiredItemID;     // used when mode = Item
    [SerializeField] private string lockedPrompt   = "Requires access.";
    [SerializeField] private string unlockedPrompt = "Press E to open";

    [SerializeField] private GameObject doorBlocker;    // the physical door mesh/collider to disable on open
    [SerializeField] private string openFlagToSet;      // flag saved so door stays open on return

    private bool isOpen;

    void Start()
    {
        // Restore open state from save
        if (!string.IsNullOrEmpty(openFlagToSet) && GameStateManager.Instance.HasFlag(openFlagToSet))
            Open(silent: true);
    }

    public override void Interact(OverworldPlayerController player)
    {
        if (isOpen) return;

        if (CanUnlock())
            Open(silent: false);
        else
            Debug.Log($"[LockedDoor] Locked — {GetPromptReason()}");
    }

    private bool CanUnlock()
    {
        return unlockMode == UnlockMode.Flag
            ? GameStateManager.Instance.HasFlag(requiredFlag)
            : InventoryManager.Instance.HasItem(requiredItemID);
    }

    private string GetPromptReason() =>
        unlockMode == UnlockMode.Flag ? $"Need flag: {requiredFlag}" : $"Need item: {requiredItemID}";

    private void Open(bool silent)
    {
        isOpen = true;

        if (doorBlocker != null) doorBlocker.SetActive(false);

        if (!silent && !string.IsNullOrEmpty(openFlagToSet))
            GameStateManager.Instance.SetFlag(openFlagToSet);
    }

    public override string GetPromptDisplayText() => GetInteractPrompt();

    // Override prompt based on lock state
    public string GetInteractPrompt() =>
        isOpen ? string.Empty : CanUnlock() ? unlockedPrompt : lockedPrompt;
}
