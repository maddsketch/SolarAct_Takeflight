using UnityEngine;

// Create via Assets > Create > Shmup > Dialogue Data
[CreateAssetMenu(fileName = "DialogueData", menuName = "Shmup/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

    [Tooltip("Fallback portrait for left-side lines when speakerPortrait is empty.")]
    public Sprite defaultPortrait;

    [Tooltip("Fallback portrait for right-side lines when speakerPortrait is empty. If null, uses defaultPortrait.")]
    public Sprite defaultPortraitRight;

    [Tooltip("Story flags set in GameStateManager when this dialogue finishes")]
    public string[] flagsToSetOnComplete;

    [Tooltip("Quest ID to start when this dialogue finishes. Leave empty for none.")]
    public string questToStartOnComplete;
}
