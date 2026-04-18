using UnityEngine;

// Place on a GameObject with a large trigger collider in an overworld sector scene.
// While the player is inside, periodically rolls a random chance to start a shmup encounter.
// On success, picks a random level from the list and opens the Pre-Mission Panel.
public class RandomEncounterZone : MonoBehaviour
{
    [Header("Level Pool")]
    [SerializeField] private LevelDefinition[] possibleLevels;

    [Header("Encounter Settings")]
    [SerializeField] private float checkInterval = 2f;
    [SerializeField] [Range(0f, 1f)] private float encounterChance = 0.15f;
    [SerializeField] private float cooldownAfterCancel = 5f;

    private bool playerInside;
    private float checkTimer;
    private float cooldownTimer;
    private bool waitingForPanelClose;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        checkTimer = 0f;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        checkTimer = 0f;
        waitingForPanelClose = false;
    }

    void Update()
    {
        if (!playerInside || possibleLevels == null || possibleLevels.Length == 0)
            return;

        if (waitingForPanelClose)
        {
            if (PreMissionPanel.Instance == null || !PreMissionPanel.Instance.IsOpen)
            {
                waitingForPanelClose = false;
                cooldownTimer = cooldownAfterCancel;
            }
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        bool panelOpen = PreMissionPanel.Instance != null && PreMissionPanel.Instance.IsOpen;
        if (panelOpen) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;
        checkTimer = 0f;

        if (Random.value > encounterChance) return;

        LevelDefinition level = possibleLevels[Random.Range(0, possibleLevels.Length)];

        if (PreMissionPanel.Instance != null)
        {
            PreMissionPanel.Instance.Open(level);
            waitingForPanelClose = true;
        }
        else
        {
            SceneTransitionManager.Instance.LoadShmupLevel(level);
        }
    }
}
