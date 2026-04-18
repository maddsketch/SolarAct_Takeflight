using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Scene-level singleton. Lives in the Overworld scene.
// Drives the dialogue sequence and reads advance input.
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public bool IsOpen { get; private set; }

    public event System.Action onDialogueStart;
    public event System.Action<DialogueLine, Sprite, DialoguePortraitSide> onLineChanged;
    public event System.Action onDialogueEnd;

    [Header("End of conversation")]
    [Tooltip("When the last line is shown, close the UI after this many seconds (0 = next frame only).")]
    [SerializeField] private float pauseBeforeCloseAfterLastLine = 0.75f;

    private DialogueData current;
    private int lineIndex;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private Coroutine _autoCloseRoutine;

    // Prevents the keypress that opened dialogue from immediately advancing it
    private const float AdvanceCooldown = 0.15f;
    private float openTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        TryResolveInteractAction();
    }

    void Update()
    {
        if (!IsOpen) return;
        if (Time.time < openTime + AdvanceCooldown) return;
        if (playerInput == null || !playerInput.isActiveAndEnabled)
        {
            TryResolveInteractAction();
            if (playerInput == null || !playerInput.isActiveAndEnabled)
                return;
        }
        if (interactAction == null)
            TryResolveInteractAction();

        if (interactAction != null && interactAction.WasPerformedThisFrame())
            Advance();
    }

    public void StartDialogue(DialogueData data)
    {
        if (IsOpen || data == null || data.lines.Length == 0) return;

        StopAutoClose();
        current = data;
        lineIndex = 0;
        IsOpen = true;
        openTime = Time.time;

        onDialogueStart?.Invoke();
        var first = current.lines[lineIndex];
        onLineChanged?.Invoke(first, ResolvePortrait(first), first.portraitSide);
        MaybeScheduleAutoCloseAfterLastLine();
    }

    void Advance()
    {
        if (lineIndex >= current.lines.Length - 1)
        {
            StopAutoClose();
            CloseDialogue();
            return;
        }

        lineIndex++;
        var line = current.lines[lineIndex];
        onLineChanged?.Invoke(line, ResolvePortrait(line), line.portraitSide);
        MaybeScheduleAutoCloseAfterLastLine();
    }

    void MaybeScheduleAutoCloseAfterLastLine()
    {
        if (lineIndex != current.lines.Length - 1)
            return;
        StopAutoClose();
        _autoCloseRoutine = StartCoroutine(AutoCloseAfterLastLineRoutine());
    }

    IEnumerator AutoCloseAfterLastLineRoutine()
    {
        if (pauseBeforeCloseAfterLastLine > 0f)
            yield return new WaitForSecondsRealtime(pauseBeforeCloseAfterLastLine);
        else
            yield return null;

        _autoCloseRoutine = null;
        if (IsOpen && current != null)
            CloseDialogue();
    }

    void StopAutoClose()
    {
        if (_autoCloseRoutine != null)
        {
            StopCoroutine(_autoCloseRoutine);
            _autoCloseRoutine = null;
        }
    }

    void CloseDialogue()
    {
        StopAutoClose();
        IsOpen = false;

        // Apply completion effects
        if (GameStateManager.Instance != null && current != null)
        {
            foreach (var flag in current.flagsToSetOnComplete)
                GameStateManager.Instance.SetFlag(flag);

            if (!string.IsNullOrEmpty(current.questToStartOnComplete))
                GameStateManager.Instance.StartQuest(current.questToStartOnComplete);
        }

        onDialogueEnd?.Invoke();
        current = null;
    }

    Sprite ResolvePortrait(DialogueLine line)
    {
        if (line == null)
            return null;
        if (line.speakerPortrait != null)
            return line.speakerPortrait;
        if (current == null)
            return null;
        if (line.portraitSide == DialoguePortraitSide.Right)
        {
            if (current.defaultPortraitRight != null)
                return current.defaultPortraitRight;
            return current.defaultPortrait;
        }
        return current.defaultPortrait;
    }

    private void TryResolveInteractAction()
    {
        if (playerInput == null || !playerInput)
            playerInput = FindAnyObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            interactAction = null;
            return;
        }

        var actions = playerInput.actions;
        interactAction = actions != null ? actions["Interact"] : null;
    }
}