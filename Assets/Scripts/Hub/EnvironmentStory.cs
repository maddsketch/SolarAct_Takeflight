using UnityEngine;

// Attach to any readable object — terminals, plaques, data logs, wall signs.
// Opens a simple text panel when the player interacts. No dialogue system needed.
public class EnvironmentStory : Interactable
{
    [SerializeField] private string storyTitle;
    [TextArea(3, 8)]
    [SerializeField] private string storyBody;
    [SerializeField] private string flagToSetOnRead; // optional — mark as read in save

    public override void Interact(OverworldPlayerController player)
    {
        EnvironmentStoryUI.Instance?.Show(storyTitle, storyBody);

        if (!string.IsNullOrEmpty(flagToSetOnRead))
            GameStateManager.Instance.SetFlag(flagToSetOnRead);
    }
}
