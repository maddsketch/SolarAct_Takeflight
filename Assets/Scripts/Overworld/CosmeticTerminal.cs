using UnityEngine;

public class CosmeticTerminal : Interactable
{
    public override void Interact(OverworldPlayerController player)
    {
        if (CosmeticMenuUI.Instance == null)
        {
            Debug.LogWarning("[CosmeticTerminal] No CosmeticMenuUI in scene.");
            return;
        }

        CosmeticMenuUI.Instance.Open();
    }
}
