using UnityEngine;

// Base class for anything the player can interact with in the overworld.
// Subclass this for ZoneTriggers, NPCs, SavePoints, etc.
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private string promptText = "Press E to interact";

    public string PromptText => promptText;

    /// <summary>String shown on the interaction prompt UI (override for dynamic prompts, e.g. locked doors).</summary>
    public virtual string GetPromptDisplayText() => promptText;

    public abstract void Interact(OverworldPlayerController player);
}
