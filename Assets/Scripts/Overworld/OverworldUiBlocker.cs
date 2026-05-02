using UnityEngine;

// True when a modal overworld UI has focus (dialogue, shop, inventory, etc.).
// Keeps interaction blocking and movement lock in sync.
public static class OverworldUiBlocker
{
    public static bool IsBlocking =>
        (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        || (ShopManager.Instance != null && ShopManager.Instance.IsOpen)
        || (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
        || (EnvironmentStoryUI.Instance != null && EnvironmentStoryUI.Instance.IsOpen)
        || (CosmeticMenuUI.Instance != null && CosmeticMenuUI.Instance.IsOpen);
}
