using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the overworld player alongside OverworldPlayerController.
// Detects the nearest Interactable within range and triggers it on input.
[RequireComponent(typeof(OverworldPlayerController))]
public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    // UI listens to this to show/hide the interaction prompt
    public event System.Action<Interactable> onNearestChanged;

    private Interactable nearest;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private OverworldPlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<OverworldPlayerController>();
        playerInput = GetComponent<PlayerInput>();
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

        // Hand control to DialogueManager or ShopManager while they are active
        bool uiBlocking = (DialogueManager.Instance      != null && DialogueManager.Instance.IsOpen)
                       || (ShopManager.Instance         != null && ShopManager.Instance.IsOpen)
                       || (InventoryUI.Instance         != null && InventoryUI.Instance.IsOpen)
                       || (EnvironmentStoryUI.Instance  != null && EnvironmentStoryUI.Instance.IsOpen)
                       || (CosmeticMenuUI.Instance      != null && CosmeticMenuUI.Instance.IsOpen);
        if (uiBlocking)
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
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer, QueryTriggerInteraction.Collide);

        Interactable found = null;
        float closest = float.MaxValue;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<Interactable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closest)
            {
                closest = dist;
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
