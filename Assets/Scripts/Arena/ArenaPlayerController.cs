using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Twin-stick arena player.
// Left stick / WASD = move. Right stick / Mouse = aim. Right trigger / LMB = shoot.
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerInput))]
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

    void Awake()
    {
        health = GetComponent<Health>();
        movementStatusEffects = GetComponent<MovementStatusEffects>();
        health.onDeath.AddListener(OnDeath);
        health.onDamaged.AddListener(OnDamaged);

        playerInput = GetComponent<PlayerInput>();
        TryResolveActions();

        mainCam = Camera.main;
    }

    void OnEnable()
    {
        TryResolveActions();
    }

    void OnDestroy()
    {
        if (health == null) return;
        health.onDeath.RemoveListener(OnDeath);
        health.onDamaged.RemoveListener(OnDamaged);
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

    private void HandleMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = sprintAction.IsPressed() ? moveSpeed * sprintMultiplier : moveSpeed;
        currentSpeed *= movementStatusEffects != null ? movementStatusEffects.GetSpeedMultiplier() : 1f;
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y) * currentSpeed * Time.deltaTime;
        Vector3 next = transform.position + move;

        if (CircleBoundary.Instance != null)
            next = CircleBoundary.Instance.Clamp(next);

        transform.position = next;
    }

    private void HandleAim()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            // Gamepad: right stick gives direction directly
            Vector2 stick = gamepad.rightStick.ReadValue();
            if (stick.sqrMagnitude > 0.1f)
                aimDirection = new Vector3(stick.x, 0f, stick.y).normalized;
        }
        else
        {
            // Mouse: raycast screen position onto the Y=0 world plane
            Vector2 mouseScreen = aimAction.ReadValue<Vector2>();
            Ray ray = mainCam.ScreenPointToRay(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldPoint = ray.GetPoint(distance);
                aimDirection = (worldPoint - transform.position).normalized;
                aimDirection.y = 0f;
            }
        }

        // Rotate ship to face aim direction
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
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
