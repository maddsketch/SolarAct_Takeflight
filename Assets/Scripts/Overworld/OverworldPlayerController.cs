using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class OverworldPlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float moveSmoothTime = 0.1f;
    [SerializeField] private float rotationDeadzone = 0.01f;
    [Header("Movement Constraints")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float lockedYPosition;
    [Header("Thruster VFX")]
    [SerializeField] private ParticleSystem[] thrusterParticles;
    [SerializeField] private float normalLifetime = 0.5f;
    [SerializeField] private float sprintLifetime = 1.2f;

    private CharacterController characterController;
    private PlayerInput playerInput;
    private InputAction sprintAction;
    private Vector2 moveInput;
    private Vector2 smoothedMoveInput;
    private Vector2 moveInputSmoothVelocity;
    private Vector3 lastNonZeroMoveDir = Vector3.forward;
    private bool thrustersPlaying;
    private bool wasSprinting;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        lockedYPosition = transform.position.y;
        TryResolveSprintAction();
    }

    void OnEnable()
    {
        RefreshLockedYPosition();
        TryResolveSprintAction();
    }

    void Start()
    {
        foreach (var ps in thrusterParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // Called by PlayerInput (Send Messages)
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (OverworldUiBlocker.IsBlocking)
        {
            moveInput = Vector2.zero;
            StopThrustersEmitting();
            return;
        }

        bool shouldPlay = moveInput.sqrMagnitude > 0f;

        if (shouldPlay && !thrustersPlaying)
        {
            bool sprinting = sprintAction.IsPressed();
            SetThrusterIntensity(sprinting);
            wasSprinting = sprinting;
            foreach (var ps in thrusterParticles)
                ps.Play();
            thrustersPlaying = true;
        }
        else if (!shouldPlay && thrustersPlaying)
        {
            foreach (var ps in thrusterParticles)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            thrustersPlaying = false;
            wasSprinting = false;
        }
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;

        if (OverworldUiBlocker.IsBlocking)
        {
            StopThrustersEmitting();
            moveInput = Vector2.zero;
            smoothedMoveInput = Vector2.zero;
            moveInputSmoothVelocity = Vector2.zero;
            EnforceYLock();
            return;
        }

        if (sprintAction == null)
            TryResolveSprintAction();
        if (sprintAction == null)
            return;

        float smooth = Mathf.Max(0.0001f, moveSmoothTime);
        smoothedMoveInput = Vector2.SmoothDamp(
            smoothedMoveInput, moveInput,
            ref moveInputSmoothVelocity, smooth,
            Mathf.Infinity, Time.deltaTime);

        Vector3 planar = new Vector3(smoothedMoveInput.x, 0f, smoothedMoveInput.y);
        if (planar.sqrMagnitude > 1f)
            planar.Normalize();

        float rotationDeadzoneSqr = rotationDeadzone * rotationDeadzone;
        if (planar.sqrMagnitude > rotationDeadzoneSqr)
            lastNonZeroMoveDir = planar.normalized;

        // Use a cached meaningful direction so near-zero smoothed input does not snap facing.
        if (moveInput.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastNonZeroMoveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (thrustersPlaying)
        {
            bool sprinting = sprintAction.IsPressed();
            if (sprinting != wasSprinting)
            {
                SetThrusterIntensity(sprinting);
                wasSprinting = sprinting;
            }
        }

        float currentSpeed = sprintAction.IsPressed() ? speed * sprintMultiplier : speed;
        Vector3 motion = planar * (currentSpeed * Time.deltaTime);
        characterController.Move(motion);
        EnforceYLock();
    }

    private void TryResolveSprintAction()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        sprintAction = actions["Sprint"];
    }

    public void RefreshLockedYPosition() => lockedYPosition = transform.position.y;
    public void SetLockedYPosition(float y) => lockedYPosition = y;

    private void SetThrusterIntensity(bool sprint)
    {
        float lifetime = sprint ? sprintLifetime : normalLifetime;

        foreach (var ps in thrusterParticles)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
        }
    }

    private void StopThrustersEmitting()
    {
        if (!thrustersPlaying)
            return;

        foreach (var ps in thrusterParticles)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        thrustersPlaying = false;
        wasSprinting = false;
    }

    private void EnforceYLock()
    {
        if (!lockYPosition)
            return;

        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.y - lockedYPosition) <= 0.0001f)
            return;

        pos.y = lockedYPosition;
        transform.position = pos;
    }
}
