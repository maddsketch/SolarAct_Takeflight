using UnityEngine;
using UnityEngine.InputSystem;

// Requires a PlayerInput component set to "Send Messages" behaviour.
// Action map: "Player" with Move (Vector2) and Attack (Button).
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float moveSmoothTime = 0.1f;
    [SerializeField] private Vector2 boundsX = new Vector2(-4f, 4f);
    [SerializeField] private Vector2 boundsZ = new Vector2(-6f, 4f);
    [SerializeField] private float maxBankAngle = 25f;
    [SerializeField] private GameObject hitSparkVfxPrefab;
    [SerializeField] private float hitSparkVfxLifetime = 2f;
    [SerializeField] private GameObject deathExplosionVfxPrefab;
    [SerializeField] private float deathExplosionVfxLifetime = 3f;

    private Vector2 moveInput;
    private Vector2 smoothedMoveInput;
    private Vector2 moveInputSmoothVelocity;
    private Health health;
    private CharacterController characterController;
    private MovementStatusEffects movementStatusEffects;

    void Awake()
    {
        health = GetComponent<Health>();
        characterController = GetComponent<CharacterController>();
        movementStatusEffects = GetComponent<MovementStatusEffects>();
        health.onDeath.AddListener(OnDeath);
        health.onDamaged.AddListener(OnDamaged);
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.onDeath.RemoveListener(OnDeath);
        health.onDamaged.RemoveListener(OnDamaged);
    }

    // Called by PlayerInput (Send Messages)
    public void SetSpeed(float value) => speed = value;

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        float smooth = Mathf.Max(0.0001f, moveSmoothTime);
        smoothedMoveInput = Vector2.SmoothDamp(
            smoothedMoveInput,
            moveInput,
            ref moveInputSmoothVelocity,
            smooth,
            Mathf.Infinity,
            Time.deltaTime);

        float slowMultiplier = movementStatusEffects != null ? movementStatusEffects.GetSpeedMultiplier() : 1f;
        float currentSpeed = speed * slowMultiplier;
        Vector3 move = new Vector3(smoothedMoveInput.x, 0f, smoothedMoveInput.y) * (currentSpeed * Time.deltaTime);
        Vector3 next = transform.position + move;
        next.x = Mathf.Clamp(next.x, boundsX.x, boundsX.y);
        next.z = Mathf.Clamp(next.z, boundsZ.x, boundsZ.y);
        Vector3 clampedDelta = next - transform.position;
        characterController.Move(clampedDelta);

        transform.rotation = Quaternion.Euler(0f, 0f, -smoothedMoveInput.x * maxBankAngle);
    }

    void OnDeath()
    {
        SpawnVfx(deathExplosionVfxPrefab, deathExplosionVfxLifetime);
        GameManager.Instance.OnPlayerDeath();
        gameObject.SetActive(false);
    }

    void OnDamaged()
    {
        SpawnVfx(hitSparkVfxPrefab, hitSparkVfxLifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
            health.TakeDamage(1);
    }

    // CharacterController collisions are reported here, so keep enemy-contact damage working
    // after removing Rigidbody from the player root.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            health.TakeDamage(1);
    }

    private void SpawnVfx(GameObject prefab, float lifetime)
    {
        if (prefab == null) return;

        GameObject vfx = Instantiate(prefab, transform.position, Quaternion.identity);
        if (lifetime > 0f)
            Destroy(vfx, lifetime);
    }
}
