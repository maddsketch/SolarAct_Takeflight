using UnityEngine;

/// <summary>
/// Emitter prefab (no Bullet on this root): spins on world Y and periodically spawns four child
/// projectiles along horizontal forward/right axes. Assign as an EnemyWeaponDefinition shot slot
/// bullet prefab; put Bullet or ArenaBullet only on the child bullet prefab.
/// </summary>
public class EnemyRotatingQuadEmitter : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject childBulletPrefab;
    [Tooltip("Seconds between each 4-way burst.")]
    [SerializeField, Min(0.02f)] private float fireInterval = 0.35f;
    [Tooltip("Degrees per second around world Y.")]
    [SerializeField] private float spinDegreesPerSecond = 90f;

    [Header("Emitter lifetime")]
    [Tooltip("Destroy this GameObject after this many seconds.")]
    [SerializeField, Min(0.1f)] private float emitterLifetime = 4f;

    [Header("Targeting")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private string effectSourceId = "EnemyRotatingQuadEmitter";

    [Header("On-Hit Effects (child projectiles)")]
    [SerializeField] private bool appliesSlow;
    [SerializeField, Range(0f, 0.95f)] private float slowPercent;
    [SerializeField, Min(0f)] private float slowDuration;

    private float _fireCooldown;

    private void Start()
    {
        Destroy(gameObject, emitterLifetime);
        _fireCooldown = 0f;
    }

    private void Update()
    {
        transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);

        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown > 0f)
            return;

        _fireCooldown = fireInterval;
        FireQuadBurst();
    }

    private void FireQuadBurst()
    {
        if (childBulletPrefab == null)
            return;

        Vector3 f = transform.forward;
        f.y = 0f;
        if (f.sqrMagnitude > 0.001f)
            f.Normalize();
        else
            f = Vector3.forward;

        Vector3 r = transform.right;
        r.y = 0f;
        if (r.sqrMagnitude > 0.001f)
            r.Normalize();
        else
            r = Vector3.right;

        SpawnChild(f);
        SpawnChild(-f);
        SpawnChild(r);
        SpawnChild(-r);
    }

    private void SpawnChild(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude <= 0.001f)
            return;

        worldDirection.Normalize();
        Quaternion rot = Quaternion.LookRotation(worldDirection, Vector3.up);
        GameObject spawned = Instantiate(childBulletPrefab, transform.position, rot);
        ConfigureChild(spawned, worldDirection);
    }

    private void ConfigureChild(GameObject spawned, Vector3 shotDirection)
    {
        if (spawned == null)
            return;

        string tag = string.IsNullOrEmpty(targetTag) ? "Player" : targetTag;
        string sourceId = string.IsNullOrEmpty(effectSourceId) ? name : effectSourceId;

        if (spawned.TryGetComponent(out Bullet bullet))
        {
            bullet.SetDirection(shotDirection);
            bullet.SetTargetTag(tag);
            bullet.ConfigureEffects(appliesSlow, slowPercent, slowDuration, sourceId);
        }

        if (spawned.TryGetComponent(out ArenaBullet arenaBullet))
        {
            arenaBullet.Init(shotDirection, tag);
            arenaBullet.ConfigureEffects(appliesSlow, slowPercent, slowDuration, sourceId);
        }
    }
}
