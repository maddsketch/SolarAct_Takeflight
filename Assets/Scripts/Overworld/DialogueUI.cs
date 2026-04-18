using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a Canvas panel in the Overworld scene.
// Wire up the references in the Inspector, then subscribe to DialogueManager events.
public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UIFadeController panelFade;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private float speakingAlpha = 1f;
    [SerializeField] private float listeningAlpha = 0.45f;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continuePrompt;

    void Start()
    {
        if (DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.onDialogueStart += OnDialogueStart;
        DialogueManager.Instance.onLineChanged += OnLineChanged;
        DialogueManager.Instance.onDialogueEnd += OnDialogueEnd;

        if (panelFade != null)
        {
            panelFade.SetVisibleImmediate(false);
        }
        else
        {
            panel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.onDialogueStart -= OnDialogueStart;
        DialogueManager.Instance.onLineChanged -= OnLineChanged;
        DialogueManager.Instance.onDialogueEnd -= OnDialogueEnd;
    }

    void OnDialogueStart()
    {
        if (panelFade != null)
        {
            panelFade.Show();
        }
        else
        {
            panel.SetActive(true);
        }
    }

    void OnLineChanged(DialogueLine line, Sprite portrait, DialoguePortraitSide side)
    {
        speakerNameText.text = line.speakerName;
        dialogueText.text = line.text;

        if (side == DialoguePortraitSide.Left)
            ApplyToSlot(leftPortrait, portrait);
        else
            ApplyToSlot(rightPortrait, portrait);

        ApplySpeakingHighlight(side);
    }

    static void ApplyToSlot(Image img, Sprite sprite)
    {
        if (img == null)
            return;

        bool show = sprite != null;
        img.sprite = sprite;
        img.gameObject.SetActive(show);
        if (show)
        {
            var c = img.color;
            c.r = c.g = c.b = 1f;
            img.color = c;
        }
    }

    void ApplySpeakingHighlight(DialoguePortraitSide speakingSide)
    {
        SetListeningVisual(leftPortrait, speakingSide != DialoguePortraitSide.Left);
        SetListeningVisual(rightPortrait, speakingSide != DialoguePortraitSide.Right);
    }

    void SetListeningVisual(Image img, bool isListening)
    {
        if (img == null || !img.gameObject.activeInHierarchy)
            return;

        var c = img.color;
        c.a = isListening ? listeningAlpha : speakingAlpha;
        img.color = c;
    }

    void OnDialogueEnd()
    {
        if (panelFade != null)
        {
            panelFade.Hide();
        }
        else
        {
            panel.SetActive(false);
        }
        ClearSlot(leftPortrait);
        ClearSlot(rightPortrait);
    }

    static void ClearSlot(Image img)
    {
        if (img == null)
            return;
        img.sprite = null;
        var c = img.color;
        c.a = 1f;
        img.color = c;
        img.gameObject.SetActive(false);
    }
}
