using System.Collections.Generic;
using UnityEngine;

// Movement is handled by EnemyMover. This handles damage and death.
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyMover))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private int xpValue = 25;
    [SerializeField] private GameObject creditDropPrefab; // optional drop choice
    [SerializeField] private GameObject healthDropPrefab; // optional drop choice
    [SerializeField] private GameObject ammoDropPrefab;   // optional; prefab should have AmmoPickup
    [SerializeField] private float dropChance = 1f;       // 0–1, chance to drop any pickup
    [SerializeField] private GameObject explosionVfxPrefab;
    [SerializeField] private float explosionVfxLifetime = 3f; // <= 0: do not auto-destroy
    [Header("Weapon")]
    [SerializeField] private EnemyWeaponDefinition enemyWeapon;
    [SerializeField] private Transform[] weaponMuzzles;
    [SerializeField] private float attackRadius = 10f;
    [SerializeField] private bool aimAtPlayer = false;
    [SerializeField] private Vector3 localFireDirection = Vector3.back;

    private Transform playerTransform;
    private EnemyPatternShooter patternShooter;
    private float nextFireTime;

    void Awake()
    {
        GetComponent<Health>().onDeath.AddListener(OnDeath);
        patternShooter = GetComponent<EnemyPatternShooter>();
    }

    void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        TryFireAtPlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<Health>()?.TakeDamage(1);
    }

    void OnDeath()
    {
        GameManager.Instance.AddScore(scoreValue);
        GameManager.Instance.RegisterKill();
        XPManager.Instance?.AddXP(xpValue);
        AchievementManager.Instance?.RegisterKill();

        TrySpawnPickupDrop();

        if (explosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            if (explosionVfxLifetime > 0f)
                Destroy(vfx, explosionVfxLifetime);
        }

        Destroy(gameObject);
    }

    private void TrySpawnPickupDrop()
    {
        if (Random.value > dropChance) return;

        var options = new List<GameObject>(3);
        if (creditDropPrefab != null) options.Add(creditDropPrefab);
        if (healthDropPrefab != null) options.Add(healthDropPrefab);
        if (ammoDropPrefab != null) options.Add(ammoDropPrefab);
        if (options.Count == 0) return;

        GameObject dropPrefab = options[Random.Range(0, options.Count)];
        Instantiate(dropPrefab, transform.position, Quaternion.identity);
    }

    private void TryFireAtPlayer()
    {
        if (enemyWeapon == null || patternShooter == null || playerTransform == null)
            return;

        if (Time.time < nextFireTime)
            return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > attackRadius * attackRadius)
            return;

        Vector3 baseDirection = aimAtPlayer
            ? toPlayer.normalized
            : transform.TransformDirection(localFireDirection.normalized);
        if (baseDirection.sqrMagnitude <= 0.001f)
            baseDirection = Vector3.back;

        patternShooter.FirePattern(enemyWeapon, weaponMuzzles, baseDirection, "Player");
        nextFireTime = Time.time + enemyWeapon.fireRate;
    }
}
