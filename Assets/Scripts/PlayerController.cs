using UnityEngine;
using UnityEngine.InputSystem;

// Requires a PlayerInput component set to "Send Messages" behaviour.
// Action map: "Player" with Move (Vector2) and Attack (Button).
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInvincibilityPulse))]
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
    [SerializeField, Min(0f)] private float enemyContactImpactMagnitude = 1f;
    [SerializeField, Min(0f)] private float enemyContactInvincibilityDuration = 3f;
    [Header("Enemy Contact Knockback")]
    [SerializeField, Min(0f)] private float enemyContactKnockbackDistance = 1.2f;
    [SerializeField, Min(0.01f)] private float enemyContactKnockbackDuration = 0.12f;
    [Header("Movement Constraints")]
    [SerializeField] private bool lockYPosition = true;
    [SerializeField] private float lockedYPosition;

    private Vector2 moveInput;
    private Vector2 smoothedMoveInput;
    private Vector2 moveInputSmoothVelocity;
    private Health health;
    private CharacterController characterController;
    private MovementStatusEffects movementStatusEffects;
    private CameraFollow cameraFollow;
    private PlayerInvincibilityPulse invincibilityPulse;
    private Vector3 knockbackVelocity;

    void Awake()
    {
        health = GetComponent<Health>();
        characterController = GetComponent<CharacterController>();
        movementStatusEffects = GetComponent<MovementStatusEffects>();
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : FindAnyObjectByType<CameraFollow>();
        invincibilityPulse = GetComponent<PlayerInvincibilityPulse>();
        lockedYPosition = transform.position.y;
        health.onDeath.AddListener(OnDeath);
        health.onDamaged.AddListener(OnDamaged);
        health.onDamagedWithImpact.AddListener(OnDamagedWithImpact);
    }

    void OnEnable()
    {
        RefreshLockedYPosition();
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.onDeath.RemoveListener(OnDeath);
        health.onDamaged.RemoveListener(OnDamaged);
        health.onDamagedWithImpact.RemoveListener(OnDamagedWithImpact);
    }

    // Called by PlayerInput (Send Messages)
    public void SetSpeed(float value) => speed = value;
    public void RefreshLockedYPosition() => lockedYPosition = transform.position.y;
    public void SetLockedYPosition(float y) => lockedYPosition = y;

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
        move += knockbackVelocity * Time.deltaTime;
        knockbackVelocity *= Mathf.Exp(-8f * Time.deltaTime);
        Vector3 next = transform.position + move;
        next.x = Mathf.Clamp(next.x, boundsX.x, boundsX.y);
        next.z = Mathf.Clamp(next.z, boundsZ.x, boundsZ.y);
        Vector3 clampedDelta = next - transform.position;
        characterController.Move(clampedDelta);
        EnforceYLock();

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

    void OnDamagedWithImpact(float impactMagnitude)
    {
        if (cameraFollow == null)
            cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : FindAnyObjectByType<CameraFollow>();
        cameraFollow?.TriggerHeavyHitShake(impactMagnitude);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            health.TakeDamage(1, enemyContactImpactMagnitude);
            ApplyContactKnockback(other.transform.position);
            ApplyEnemyContactInvincibility();
        }
    }

    // CharacterController collisions are reported here, so keep enemy-contact damage working
    // after removing Rigidbody from the player root.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            health.TakeDamage(1, Mathf.Max(enemyContactImpactMagnitude, hit.moveLength));
            ApplyContactKnockback(hit.collider.transform.position);
            ApplyEnemyContactInvincibility();
        }
    }

    private void ApplyContactKnockback(Vector3 enemyPosition)
    {
        if (enemyContactKnockbackDistance <= 0f || enemyContactKnockbackDuration <= 0f)
            return;

        Vector3 away = transform.position - enemyPosition;
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f)
            away = -transform.forward;
        else
            away.Normalize();

        float initialSpeed = enemyContactKnockbackDistance / enemyContactKnockbackDuration;
        knockbackVelocity = away * initialSpeed;
    }

    private void ApplyEnemyContactInvincibility()
    {
        if (enemyContactInvincibilityDuration <= 0f)
            return;

        health.SetInvincible(enemyContactInvincibilityDuration);
        if (invincibilityPulse == null)
            invincibilityPulse = GetComponent<PlayerInvincibilityPulse>();
        invincibilityPulse?.StartPulse(enemyContactInvincibilityDuration);
    }

    private void SpawnVfx(GameObject prefab, float lifetime)
    {
        if (prefab == null) return;

        GameObject vfx = Instantiate(prefab, transform.position, Quaternion.identity);
        if (lifetime > 0f)
            Destroy(vfx, lifetime);
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
