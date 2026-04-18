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
    [Header("Banking")]
    [SerializeField] private float maxBankAngle = 30f;
    [SerializeField] private float bankSmooth = 5f;
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
    private float currentBankAngle;
    private bool thrustersPlaying;
    private bool wasSprinting;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        TryResolveSprintAction();
    }

    void OnEnable()
    {
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

        if (planar.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(planar, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        float bankTarget = 0f;
        if (planar.sqrMagnitude > 0.01f)
        {
            float signedAngle = Vector3.SignedAngle(transform.forward, planar, Vector3.up);
            bankTarget = Mathf.Clamp(signedAngle / 90f, -1f, 1f) * -maxBankAngle;
        }
        currentBankAngle = Mathf.Lerp(currentBankAngle, bankTarget, bankSmooth * Time.deltaTime);
        transform.rotation *= Quaternion.Euler(0f, 0f, currentBankAngle);

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

    private void SetThrusterIntensity(bool sprint)
    {
        float lifetime = sprint ? sprintLifetime : normalLifetime;

        foreach (var ps in thrusterParticles)
        {
            var main = ps.main;
            main.startLifetime = lifetime;
        }
    }
}
