using System.Collections;
using UnityEngine;

// Attach to arena enemy prefabs alongside a Health component.
// States: Idle (wander) -> Chase (move to player) -> Attack (shoot at player).
[RequireComponent(typeof(Health))]
public class ArenaEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float deaggroRadius   = 12f;  // must leave this range to return to idle

    [Header("Movement")]
    [SerializeField] private float chaseSpeed  = 3.5f;
    [SerializeField] private float idleSpeed   = 1.2f;
    [SerializeField] private float wanderChangeInterval = 2f;

    [Header("Attack")]
    [SerializeField] private float attackRadius = 4f;
    [SerializeField] private float fireRate     = 1.5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EnemyWeaponDefinition enemyWeapon;
    [SerializeField] private Transform[] weaponMuzzles;

    [Header("Stats")]
    [SerializeField] private int scoreValue = 150;
    [SerializeField] private int xpValue    = 40;
    [SerializeField] private GameObject explosionVfxPrefab;
    [SerializeField] private float explosionVfxLifetime = 3f; // <= 0: do not auto-destroy

    public enum AIState { Idle, Chase, Attack }
    public AIState State { get; private set; } = AIState.Idle;

    private Transform player;
    private float nextFireTime;
    private Vector3 wanderDirection;
    private float wanderTimer;
    private EnemyPatternShooter patternShooter;
    private Transform[] fallbackFirePointMuzzles;
    private Transform[] fallbackSelfMuzzles;

    void Awake()
    {
        var health = GetComponent<Health>();
        health.onDeath.AddListener(OnDeath);
        patternShooter = GetComponent<EnemyPatternShooter>();
        fallbackFirePointMuzzles = new Transform[1];
        fallbackSelfMuzzles = new[] { transform };
        MinimapUI.Register(this);
    }

    void OnDestroy() => MinimapUI.Unregister(this);

    void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        PickWanderDirection();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        UpdateState(dist);

        switch (State)
        {
            case AIState.Idle:   DoIdle();   break;
            case AIState.Chase:  DoChase();  break;
            case AIState.Attack: DoAttack(); break;
        }
    }

    private void UpdateState(float dist)
    {
        switch (State)
        {
            case AIState.Idle:
                if (dist <= detectionRadius) State = AIState.Chase;
                break;
            case AIState.Chase:
                if (dist <= attackRadius)   State = AIState.Attack;
                else if (dist > deaggroRadius) State = AIState.Idle;
                break;
            case AIState.Attack:
                if (dist > attackRadius)    State = AIState.Chase;
                break;
        }
    }

    private void DoIdle()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f) PickWanderDirection();

        Vector3 next = transform.position + wanderDirection * idleSpeed * Time.deltaTime;
        if (CircleBoundary.Instance != null)
        {
            Vector3 clamped = CircleBoundary.Instance.Clamp(next);
            if (clamped != next) PickWanderDirection(); // bounce off boundary
            next = clamped;
        }

        transform.position = next;
        if (wanderDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(wanderDirection, Vector3.up);
    }

    private void DoChase()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * chaseSpeed * Time.deltaTime;
        transform.rotation  = Quaternion.LookRotation(dir, Vector3.up);
    }

    private void DoAttack()
    {
        // Face the player while standing still
        Vector3 dir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        if (enemyWeapon != null && patternShooter != null)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + enemyWeapon.fireRate;
                patternShooter.FirePattern(enemyWeapon, ResolveMuzzles(), dir, "Player");
            }
            return;
        }

        if (Time.time >= nextFireTime && bulletPrefab != null && firePoint != null)
        {
            nextFireTime = Time.time + fireRate;
            var bullet = Instantiate(bulletPrefab, firePoint.position,
                Quaternion.LookRotation(dir, Vector3.up));

            // Flip the target tag so enemy bullets only hit the player
            var b = bullet.GetComponent<Bullet>();
            if (b != null) b.SetTargetTag("Player");
        }
    }

    private Transform[] ResolveMuzzles()
    {
        if (weaponMuzzles != null && weaponMuzzles.Length > 0)
            return weaponMuzzles;

        if (firePoint != null)
        {
            fallbackFirePointMuzzles[0] = firePoint;
            return fallbackFirePointMuzzles;
        }

        return fallbackSelfMuzzles;
    }

    private void PickWanderDirection()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        wanderDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        wanderTimer = wanderChangeInterval;
    }

    private void OnDeath()
    {
        GameManager.Instance?.AddScore(scoreValue);
        XPManager.Instance?.AddXP(xpValue);
        ArenaGameManager.Instance?.OnEnemyKilled();

        if (explosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            if (explosionVfxLifetime > 0f)
                Destroy(vfx, explosionVfxLifetime);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
#endif
}
