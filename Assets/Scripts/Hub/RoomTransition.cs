using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Place as a trigger volume at doorways or zone exits.
// Supports: room-to-room teleport, hub-to-overworld, overworld-to-overworld (sector travel).
// Optional level gate: blocks transition and shows a message if the required flag is missing.
// Optional press-to-enter: shows a prompt and waits for E key instead of auto-transitioning.
public class RoomTransition : MonoBehaviour
{
    public enum TransitionType { RoomTeleport, ExitToOverworld, LoadSector, LoadHub }

    [Header("Transition")]
    [SerializeField] private TransitionType transitionType = TransitionType.RoomTeleport;
    [SerializeField] private Transform linkedSpawn;         // RoomTeleport only
    [SerializeField] private string targetSceneName;        // LoadSector / LoadHub
    [SerializeField] private string spawnPointID;           // optional — overworld spawn point to use on return

    [Header("Press To Enter")]
    [SerializeField] private bool requireButtonPress = false;
    [SerializeField] private GameObject enterPrompt;        // e.g. "[ E ] Enter"
    [SerializeField] private string enterPromptMessage = "[ E ]  Enter";
    [SerializeField] private TextMeshProUGUI enterPromptText;

    [Header("Level Gate")]
    [SerializeField] private string requiredFlag;
    [SerializeField] private string lockedMessage = "Higher level required.";

    [Header("Locked Message UI")]
    [SerializeField] private GameObject lockedPrompt;
    [SerializeField] private TextMeshProUGUI lockedText;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.4f;

    private bool transitioning;
    private bool playerInRange;
    private GameObject playerInTrigger;
    private Coroutine hidePromptCoroutine;

    void Update()
    {
        if (!requireButtonPress || !playerInRange || transitioning) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            TryTransition(playerInTrigger);
    }

    void OnTriggerEnter(Collider other)
    {
        if (transitioning) return;
        if (!other.CompareTag("Player")) return;

        playerInTrigger = other.gameObject;

        if (requireButtonPress)
        {
            playerInRange = true;
            ShowEnterPrompt(true);
            return;
        }

        TryTransition(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        playerInTrigger = null;
        ShowEnterPrompt(false);
    }

    private void TryTransition(GameObject player)
    {
        // Check level gate
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.HasFlag(requiredFlag))
            {
                ShowLockedMessage();
                return;
            }
        }

        ShowEnterPrompt(false);
        StartCoroutine(DoTransition(player));
    }

    private void ShowEnterPrompt(bool show)
    {
        if (enterPrompt == null) return;
        if (enterPromptText != null) enterPromptText.text = enterPromptMessage;

        var fade = enterPrompt.GetComponent<UIFadeController>();
        if (show)
        {
            enterPrompt.SetActive(true);
            if (fade != null) fade.Show();
        }
        else
        {
            if (fade != null) fade.Hide();
            enterPrompt.SetActive(false);
        }
    }

    private IEnumerator DoTransition(GameObject player)
    {
        transitioning = true;

        yield return StartCoroutine(Fade(1f));

        switch (transitionType)
        {
            case TransitionType.ExitToOverworld:
                if (!string.IsNullOrEmpty(spawnPointID) && SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.RequestedSpawnID = spawnPointID;
                GameStateManager.Instance?.CaptureOverworldPosition(player.transform.position);
                GameStateManager.Instance?.Save(SceneTransitionManager.Instance?.ActiveSaveSlot ?? 0);
                transitioning = false;
                SceneTransitionManager.Instance?.ReturnToOverworld(won: false);
                yield break;

            case TransitionType.LoadSector:
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    GameStateManager.Instance?.CaptureOverworldPosition(player.transform.position);
                    GameStateManager.Instance?.Save(SceneTransitionManager.Instance?.ActiveSaveSlot ?? 0);
                    transitioning = false;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                }
                yield break;

            case TransitionType.LoadHub:
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    if (SceneTransitionManager.Instance != null)
                        SceneTransitionManager.Instance.ReturnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    GameStateManager.Instance?.Save(SceneTransitionManager.Instance?.ActiveSaveSlot ?? 0);
                    transitioning = false;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
                }
                yield break;

            case TransitionType.RoomTeleport:
                if (linkedSpawn != null)
                    player.transform.position = linkedSpawn.position;
                yield return StartCoroutine(Fade(0f));
                break;
        }

        transitioning = false;
    }

    private void ShowLockedMessage()
    {
        if (lockedPrompt == null) return;

        if (lockedText != null) lockedText.text = lockedMessage;
        lockedPrompt.SetActive(true);

        if (hidePromptCoroutine != null) StopCoroutine(hidePromptCoroutine);
        hidePromptCoroutine = StartCoroutine(HidePromptAfterDelay(2.5f));
    }

    private IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lockedPrompt != null) lockedPrompt.SetActive(false);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        yield return new WaitForSeconds(fadeDuration);
    }
}
