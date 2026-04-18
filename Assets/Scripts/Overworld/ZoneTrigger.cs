using UnityEngine;

// Place in the overworld where a shmup level should be accessible.
// Assign a LevelDefinition and any story flags that must be set before this zone is active.
public class ZoneTrigger : Interactable
{
    [SerializeField] private LevelDefinition levelDefinition;
    [SerializeField] private string[] requiredFlags;

    public override void Interact(OverworldPlayerController player)
    {
        if (levelDefinition == null) return;

        foreach (var flag in requiredFlags)
        {
            if (!GameStateManager.Instance.HasFlag(flag))
            {
                Debug.Log($"[ZoneTrigger] Missing required flag: {flag}");
                return;
            }
        }

        if (PreMissionPanel.Instance != null)
            PreMissionPanel.Instance.Open(levelDefinition);
        else
            SceneTransitionManager.Instance.LoadShmupLevel(levelDefinition);
    }
}
