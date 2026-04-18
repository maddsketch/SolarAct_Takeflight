using UnityEngine;

public class NPCDialogue : Interactable
{
    [SerializeField] private DialogueData dialogueData;

    public override void Interact(OverworldPlayerController player)
    {
        DialogueManager.Instance.StartDialogue(dialogueData);
    }
}
