using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Twin-stick arena player.
// Left stick / WASD = move. Right stick (when pushed) or Mouse = aim. Right trigger / LMB = shoot.
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInvincibilityPulse))]
public class ArenaPlayerController : MonoBehaviour
{
    [System.Serializable]
    private struct ArenaShotSlot
    {
        [Tooltip("Index into Fire Points array. Uses fallback firePoint when invalid.")]
        public int firePointIndex;
        [Tooltip("Yaw offset in degrees from the current aim direction.")]
        public float angleOffset;
        [Tooltip("Local offset from selected fire point.")]
        public Vector3 positionOffset;
        [Tooltip("Additional delay before this shot fires within a burst.")]
        public float spawnDelay;
    }

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private ArenaShotSlot[] shotSlots;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField, Min(1)] private int burstCount = 1;
    [SerializeField, Min(0f)] private float burstInterval;
    [Header("On-Hit Effects")]
    [SerializeField] private bool appliesSlow;
    [SerializeField, Range(0f, 0.95f)] private float slowPercent;
    [SerializeField, Min(0f)] private float slowDuration;
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
    [Header("Aim")]
    [SerializeField, Min(0f)] private float aimStickDeadZone = 0.1f;

    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction shootAction;
    private InputAction sprintAction;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector3 aimDirection;
    private float nextFireTime;
    private Camera mainCam;
    private Health health;
    private MovementStatusEffects movementStatusEffects;
    private CameraFollow cameraFollow;
    private PlayerInvincibilityPulse invincibilityPulse;
    private Vector3 knockbackVelocity;

    void Awake()
    {
        health = GetComponent<Health>();
        movementStatusEffects = GetComponent<MovementStatusEffects>();
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : FindAnyObjectByType<CameraFollow>();
        invincibilityPulse = GetComponent<PlayerInvincibilityPulse>();
        lockedYPosition = transform.position.y;
        health.onDeath.AddListener(OnDeath);
        health.onDamaged.AddListener(OnDamaged);
        health.onDamagedWithImpact.AddListener(OnDamagedWithImpact);

        playerInput = GetComponent<PlayerInput>();
        TryResolveActions();

        mainCam = Camera.main;
    }

    void OnEnable()
    {
        RefreshLockedYPosition();
        TryResolveActions();
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.onDeath.RemoveListener(OnDeath);
        health.onDamaged.RemoveListener(OnDamaged);
        health.onDamagedWithImpact.RemoveListener(OnDamagedWithImpact);
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;
        if (moveAction == null || aimAction == null || shootAction == null || sprintAction == null)
            TryResolveActions();
        if (moveAction == null || aimAction == null || shootAction == null || sprintAction == null)
            return;

        HandleMovement();
        HandleAim();
        HandleShoot();
    }

    private void TryResolveActions()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        moveAction = actions["Move"];
        aimAction = actions["Aim"];
        shootAction = actions["ArenaShoot"];
        sprintAction = actions["Sprint"];
    }

    public void RefreshLockedYPosition() => lockedYPosition = transform.position.y;
    public void SetLockedYPosition(float y) => lockedYPosition = y;

    private void HandleMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = sprintAction.IsPressed() ? moveSpeed * sprintMultiplier : moveSpeed;
        currentSpeed *= movementStatusEffects != null ? movementStatusEffects.GetSpeedMultiplier() : 1f;
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * currentSpeed * Time.deltaTime;
        move += knockbackVelocity * Time.deltaTime;
        knockbackVelocity *= Mathf.Exp(-8f * Time.deltaTime);
        Vector3 next = transform.position + move;

        if (CircleBoundary.Instance != null)
            next = CircleBoundary.Instance.Clamp(next);

        transform.position = next;
        EnforceYLock();
    }

    private void HandleAim()
    {
        // Right stick aims when pushed past dead zone; otherwise mouse (so K&M works with a gamepad plugged in).
        var gamepad = Gamepad.current;
        float deadZoneSq = aimStickDeadZone * aimStickDeadZone;
        Vector2 stick = gamepad != null ? gamepad.rightStick.ReadValue() : Vector2.zero;
        bool useRightStick = gamepad != null && stick.sqrMagnitude > deadZoneSq;

        if (useRightStick)
        {
            aimDirection = new Vector3(stick.x, 0f, stick.y).normalized;
        }
        else
        {
            if (mainCam == null)
                mainCam = Camera.main;

            if (mainCam != null && aimAction != null)
            {
                float aimPlaneY = lockYPosition ? lockedYPosition : transform.position.y;
                Vector3 planePoint = new Vector3(transform.position.x, aimPlaneY, transform.position.z);
                Plane groundPlane = new Plane(Vector3.up, planePoint);

                Vector2 mouseScreen = aimAction.ReadValue<Vector2>();
                Ray ray = mainCam.ScreenPointToRay(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 worldPoint = ray.GetPoint(distance);
                    Vector3 flat = worldPoint - transform.position;
                    flat.y = 0f;
                    if (flat.sqrMagnitude > 1e-6f)
                        aimDirection = flat.normalized;
                }
            }
        }

        if (aimDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(aimDirection, Vector3.up);
    }

    private void HandleShoot()
    {
        if (shootAction.IsPressed() && Time.time >= nextFireTime && bulletPrefab != null)
        {
            nextFireTime = Time.time + fireRate;
            StartCoroutine(FirePatternRoutine());
        }
    }

    private IEnumerator FirePatternRoutine()
    {
        int bursts = Mathf.Max(1, burstCount);
        for (int burstIndex = 0; burstIndex < bursts; burstIndex++)
        {
            if (shotSlots != null && shotSlots.Length > 0)
            {
                for (int i = 0; i < shotSlots.Length; i++)
                {
                    ArenaShotSlot slot = shotSlots[i];
                    if (slot.spawnDelay > 0f)
                        StartCoroutine(FireSlotAfterDelay(slot));
                    else
                        FireSlot(slot);
                }
            }
            else
            {
                FireSingleFallbackShot();
            }

            if (burstIndex < bursts - 1 && burstInterval > 0f)
                yield return new WaitForSeconds(burstInterval);
        }
    }

    private IEnumerator FireSlotAfterDelay(ArenaShotSlot slot)
    {
        yield return new WaitForSeconds(slot.spawnDelay);
        FireSlot(slot);
    }

    private void FireSlot(ArenaShotSlot slot)
    {
        Transform origin = ResolveFirePoint(slot.firePointIndex);
        Vector3 baseDirection = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : transform.forward;
        Vector3 shotDirection = Quaternion.Euler(0f, slot.angleOffset, 0f) * baseDirection;
        if (shotDirection.sqrMagnitude <= 0.001f)
            shotDirection = transform.forward;
        shotDirection.Normalize();

        Vector3 spawnPosition = origin.position + origin.TransformVector(slot.positionOffset);
        GameObject spawned = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shotDirection, Vector3.up));
        ConfigureSpawnedProjectile(spawned, shotDirection);
    }

    private void FireSingleFallbackShot()
    {
        Transform origin = ResolveFirePoint(0);
        Vector3 shotDirection = aimDirection.sqrMagnitude > 0.001f ? aimDirection.normalized : transform.forward;
        GameObject spawned = Instantiate(bulletPrefab, origin.position, Quaternion.LookRotation(shotDirection, Vector3.up));
        ConfigureSpawnedProjectile(spawned, shotDirection);
    }

    private Transform ResolveFirePoint(int index)
    {
        if (firePoints != null && index >= 0 && index < firePoints.Length && firePoints[index] != null)
            return firePoints[index];

        if (firePoint != null)
            return firePoint;

        return transform;
    }

    private void ConfigureSpawnedProjectile(GameObject spawned, Vector3 shotDirection)
    {
        if (spawned == null)
            return;

        if (spawned.TryGetComponent(out ArenaBullet arenaBullet))
            arenaBullet.Init(shotDirection, "Enemy");

        if (spawned.TryGetComponent(out Bullet bullet))
        {
            bullet.SetDirection(shotDirection);
            bullet.SetTargetTag("Enemy");
        }

        ApplySlowPayload(spawned);
    }

    private void ApplySlowPayload(GameObject spawned)
    {
        if (spawned == null)
            return;

        if (spawned.TryGetComponent(out Bullet bullet))
            bullet.ConfigureEffects(appliesSlow, slowPercent, slowDuration, "ArenaPlayerWeapon");

        if (spawned.TryGetComponent(out ArenaBullet arenaBullet))
            arenaBullet.ConfigureEffects(appliesSlow, slowPercent, slowDuration, "ArenaPlayerWeapon");
    }

    private void OnDeath()
    {
        SpawnVfx(deathExplosionVfxPrefab, deathExplosionVfxLifetime);
        ArenaGameManager.Instance?.OnPlayerDeath();
        gameObject.SetActive(false);
    }

    private void OnDamaged()
    {
        SpawnVfx(hitSparkVfxPrefab, hitSparkVfxLifetime);
    }

    private void OnDamagedWithImpact(float impactMagnitude)
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
