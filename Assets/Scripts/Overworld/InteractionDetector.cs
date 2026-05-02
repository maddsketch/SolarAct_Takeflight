using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the overworld player alongside OverworldPlayerController.
// Detects the nearest Interactable within range and triggers it on input.
[RequireComponent(typeof(OverworldPlayerController))]
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private int maxDetectedColliders = 24;

    // UI listens to this to show/hide the interaction prompt
    public event System.Action<Interactable> onNearestChanged;

    private Interactable nearest;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private OverworldPlayerController playerController;
    private Collider[] overlapResults;

    void Awake()
    {
        playerController = GetComponent<OverworldPlayerController>();
        playerInput = GetComponent<PlayerInput>();
        overlapResults = new Collider[Mathf.Max(4, maxDetectedColliders)];
        TryResolveInteractAction();
    }

    void OnEnable()
    {
        TryResolveInteractAction();
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;
        if (interactAction == null)
            TryResolveInteractAction();
        if (interactAction == null)
            return;

        if (OverworldUiBlocker.IsBlocking)
        {
            // Clear nearest so the prompt hides during dialogue
            if (nearest != null)
            {
                nearest = null;
                onNearestChanged?.Invoke(null);
            }
            return;
        }

        RefreshNearest();

        if (nearest != null && interactAction.WasPerformedThisFrame())
            nearest.Interact(playerController);
    }

    private void TryResolveInteractAction()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        interactAction = actions["Interact"];
    }

    void RefreshNearest()
    {
        if (overlapResults == null || overlapResults.Length != Mathf.Max(4, maxDetectedColliders))
            overlapResults = new Collider[Mathf.Max(4, maxDetectedColliders)];

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            overlapResults,
            interactableLayer,
            QueryTriggerInteraction.Collide);

        Interactable found = null;
        float closestSqr = float.MaxValue;
        Vector3 origin = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapResults[i];
            var interactable = hit.GetComponent<Interactable>();
            if (interactable == null) continue;

            float distSqr = (hit.transform.position - origin).sqrMagnitude;
            if (distSqr < closestSqr)
            {
                closestSqr = distSqr;
                found = interactable;
            }
        }

        if (found != nearest)
        {
            nearest = found;
            onNearestChanged?.Invoke(nearest);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
