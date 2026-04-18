using UnityEngine;

// Attach alongside NPCDialogue on an NPC that gives quests.
// Handles quest state: offer -> active -> complete, with dialogue per state.
public class HubQuestGiver : Interactable
{
    [Header("Quest")]
    [SerializeField] private string questID;
    [SerializeField] private string questDisplayName;

    [Header("Dialogues")]
    [SerializeField] private DialogueData introDialogue;      // before quest offered
    [SerializeField] private DialogueData offerDialogue;      // quest offer
    [SerializeField] private DialogueData activeDialogue;     // quest in progress
    [SerializeField] private DialogueData completeDialogue;   // on completion

    [Header("Completion condition")]
    [SerializeField] private string requiredFlagToComplete;   // flag that signals quest done

    [Header("Reward")]
    [SerializeField] private int xpReward;
    [SerializeField] private int currencyReward;
    [SerializeField] private string itemRewardID;             // optional item grant

    [Header("Gate")]
    [SerializeField] private string requiredFlagToOffer;      // optional — must be set before quest is offered

    public override void Interact(OverworldPlayerController player)
    {
        var gsm = GameStateManager.Instance;

        bool questComplete  = gsm.HasFlag($"quest_complete_{questID}");
        bool questActive    = gsm.Current.activeQuestIDs.Contains(questID);
        bool canOffer       = string.IsNullOrEmpty(requiredFlagToOffer) || gsm.HasFlag(requiredFlagToOffer);

        if (questComplete)
        {
            // Already done — replay completion line
            PlayDialogue(completeDialogue);
            return;
        }

        if (questActive)
        {
            if (!string.IsNullOrEmpty(requiredFlagToComplete) && gsm.HasFlag(requiredFlagToComplete))
                CompleteQuest();
            else
                PlayDialogue(activeDialogue);
            return;
        }

        if (!canOffer)
        {
            PlayDialogue(introDialogue);
            return;
        }

        // Offer the quest
        PlayDialogue(offerDialogue);
        gsm.StartQuest(questID);
    }

    private void CompleteQuest()
    {
        GameStateManager.Instance.CompleteQuest(questID);

        if (xpReward > 0)       XPManager.Instance?.AddXP(xpReward);
        if (currencyReward > 0)  InventoryManager.Instance?.AddCurrency(currencyReward);
        if (!string.IsNullOrEmpty(itemRewardID)) InventoryManager.Instance?.AddItem(itemRewardID, 1);

        GameStateManager.Instance.Save(SceneTransitionManager.Instance?.ActiveSaveSlot ?? 0);

        PlayDialogue(completeDialogue);
    }

    private void PlayDialogue(DialogueData data)
    {
        if (data == null) return;
        DialogueManager.Instance?.StartDialogue(data);
    }
}
