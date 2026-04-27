using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class SubBossController : MonoBehaviour
{
    enum Phase { Enter, Phase1, Phase2, Phase3 }

    [Header("Identity")]
    [SerializeField] private string bossName = "SUB-BOSS";

    [Header("Movement")]
    [SerializeField] private float enterSpeed = 3f;
    [SerializeField] private float holdZ = 6f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 4f;

    [Header("Fire Points")]
    [SerializeField] private Transform firePointFront;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private Transform firePointRear;
    [SerializeField] private GameObject bulletPrefab;
    [Header("Pattern Weapons")]
    [SerializeField] private EnemyWeaponDefinition phase1Weapon;

    [Header("Phase Thresholds (fraction of max HP)")]
    [SerializeField] private float phase2Threshold = 0.6f;
    [SerializeField] private float phase3Threshold = 0.3f;

    [Header("Attack Timing")]
    [SerializeField] private float phase1FireRate = 1.2f;
    [SerializeField] private float phase2FireRate = 0.8f;
    [SerializeField] private float phase3FireRate = 0.4f;
    [SerializeField] private float phase3SpreadAngle = 30f;
    [SerializeField] private int phase3BulletsPerVolley = 3;

    [Header("Rewards")]
    [SerializeField] private int scoreValue = 1000;
    [SerializeField] private int xpValue = 200;
    [SerializeField] private GameObject creditDropPrefab;

    private Health health;
    private BossHealthUI healthUI;
    private Transform playerTransform;
    private Phase currentPhase = Phase.Enter;
    private Coroutine attackCoroutine;
    private EnemyPatternShooter patternShooter;
    private Transform[] phase1Muzzles;

    private float patrolOriginX;
    private float patrolTimer;

    void Awake()
    {
        health = GetComponent<Health>();
        patternShooter = GetComponent<EnemyPatternShooter>();
        phase1Muzzles = new Transform[4];
        RefreshPhase1Muzzles();
        health.onDeath.AddListener(OnDeath);
        health.onHealthChanged.AddListener(OnHealthChanged);
    }

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        healthUI = FindAnyObjectByType<BossHealthUI>();
        healthUI?.Show(bossName, health.Max);

        patrolOriginX = 0f;
    }

    void Update()
    {
        switch (currentPhase)
        {
            case Phase.Enter:
                UpdateEnter();
                break;
            case Phase.Phase1:
            case Phase.Phase2:
            case Phase.Phase3:
                UpdatePatrol();
                break;
        }

        healthUI?.UpdateHP(health.Current);
    }

    // ------------------------------------------------------------------
    // Movement
    // ------------------------------------------------------------------

    void UpdateEnter()
    {
        Vector3 holdPos = new Vector3(transform.position.x, transform.position.y, holdZ);
        transform.position = Vector3.MoveTowards(transform.position, holdPos, enterSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.z - holdZ) < 0.05f)
        {
            patrolOriginX = transform.position.x;
            TransitionTo(Phase.Phase1);
        }
    }

    void UpdatePatrol()
    {
        float speed = currentPhase switch
        {
            Phase.Phase2 => patrolSpeed * 1.5f,
            Phase.Phase3 => patrolSpeed * 2.2f,
            _ => patrolSpeed
        };

        patrolTimer += Time.deltaTime * speed;

        float xOffset;
        if (currentPhase == Phase.Phase3)
        {
            xOffset = Mathf.Sin(patrolTimer * 2.5f) * patrolRange;
        }
        else
        {
            xOffset = Mathf.Sin(patrolTimer) * patrolRange;
        }

        Vector3 pos = transform.position;
        pos.x = patrolOriginX + xOffset;
        transform.position = pos;
    }

    // ------------------------------------------------------------------
    // Phase transitions
    // ------------------------------------------------------------------

    void OnHealthChanged(int current, int max)
    {
        if (max <= 0) return;
        float ratio = (float)current / max;

        if (currentPhase == Phase.Phase1 && ratio <= phase2Threshold)
            TransitionTo(Phase.Phase2);
        else if (currentPhase == Phase.Phase2 && ratio <= phase3Threshold)
            TransitionTo(Phase.Phase3);
    }

    void TransitionTo(Phase newPhase)
    {
        currentPhase = newPhase;

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = newPhase switch
        {
            Phase.Phase1 => StartCoroutine(Phase1Attack()),
            Phase.Phase2 => StartCoroutine(Phase2Attack()),
            Phase.Phase3 => StartCoroutine(Phase3Attack()),
            _ => null
        };
    }

    // ------------------------------------------------------------------
    // Attack patterns
    // ------------------------------------------------------------------

    IEnumerator Phase1Attack()
    {
        while (true)
        {
            if (phase1Weapon != null && patternShooter != null)
            {
                RefreshPhase1Muzzles();
                patternShooter.FirePattern(
                    phase1Weapon,
                    phase1Muzzles,
                    Vector3.back,
                    "Player");
            }
            else
            {
                FireStraightDown(firePointFront);
                FireStraightDown(firePointLeft);
                FireStraightDown(firePointRight);
            }
            yield return new WaitForSeconds(phase1FireRate);
        }
    }

    void RefreshPhase1Muzzles()
    {
        phase1Muzzles[0] = firePointFront;
        phase1Muzzles[1] = firePointLeft;
        phase1Muzzles[2] = firePointRight;
        phase1Muzzles[3] = firePointRear;
    }

    IEnumerator Phase2Attack()
    {
        bool alternate = false;
        while (true)
        {
            FireAtPlayer(firePointFront);

            if (alternate)
            {
                FireAngled(firePointLeft, -20f);
                FireAngled(firePointRight, 20f);
            }
            else
            {
                FireStraightDown(firePointLeft);
                FireStraightDown(firePointRight);
            }

            FireStraightUp(firePointRear);

            alternate = !alternate;
            yield return new WaitForSeconds(phase2FireRate);
        }
    }

    IEnumerator Phase3Attack()
    {
        while (true)
        {
            FireFan(firePointFront, phase3BulletsPerVolley, phase3SpreadAngle);
            FireFan(firePointLeft, phase3BulletsPerVolley, phase3SpreadAngle);
            FireFan(firePointRight, phase3BulletsPerVolley, phase3SpreadAngle);
            FireFan(firePointRear, phase3BulletsPerVolley, phase3SpreadAngle * 0.5f, Vector3.forward);

            yield return new WaitForSeconds(phase3FireRate);
        }
    }

    // ------------------------------------------------------------------
    // Firing helpers
    // ------------------------------------------------------------------

    void FireStraightDown(Transform point)
    {
        if (point == null || bulletPrefab == null) return;
        SpawnBullet(point.position, Vector3.back);
    }

    void FireStraightUp(Transform point)
    {
        if (point == null || bulletPrefab == null) return;
        SpawnBullet(point.position, Vector3.forward);
    }

    void FireAtPlayer(Transform point)
    {
        if (point == null || bulletPrefab == null || playerTransform == null) return;
        Vector3 dir = (playerTransform.position - point.position).normalized;
        dir.y = 0f;
        SpawnBullet(point.position, dir.normalized);
    }

    void FireAngled(Transform point, float angleDegrees)
    {
        if (point == null || bulletPrefab == null) return;
        Vector3 dir = Quaternion.Euler(0f, angleDegrees, 0f) * Vector3.back;
        SpawnBullet(point.position, dir);
    }

    void FireFan(Transform point, int count, float totalSpread, Vector3? baseDir = null)
    {
        if (point == null || bulletPrefab == null) return;

        Vector3 center = baseDir ?? Vector3.back;
        float step = count > 1 ? totalSpread / (count - 1) : 0f;
        float startAngle = -totalSpread * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * center;
            SpawnBullet(point.position, dir);
        }
    }

    void SpawnBullet(Vector3 position, Vector3 direction)
    {
        var go = Instantiate(bulletPrefab, position, Quaternion.identity);
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDirection(direction);
            bullet.SetTargetTag("Player");
        }
    }

    // ------------------------------------------------------------------
    // Death
    // ------------------------------------------------------------------

    void OnDeath()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        GameManager.Instance?.AddScore(scoreValue);
        GameManager.Instance?.RegisterKill();
        XPManager.Instance?.AddXP(xpValue);
        AchievementManager.Instance?.RegisterKill();

        if (creditDropPrefab != null)
            Instantiate(creditDropPrefab, transform.position, Quaternion.identity);

        healthUI?.Hide();
        Destroy(gameObject);
    }
}
