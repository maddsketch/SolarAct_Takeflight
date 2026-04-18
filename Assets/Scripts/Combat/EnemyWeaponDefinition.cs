using UnityEngine;

[System.Serializable]
public struct EnemyShotSlot
{
    public GameObject bulletPrefab;
    [Tooltip("Index into the shooter-provided muzzle array.")]
    public int muzzleIndex;
    [Tooltip("Yaw offset in degrees from the base aim direction.")]
    public float angleOffset;
    [Tooltip("Local offset from the selected muzzle transform.")]
    public Vector3 positionOffset;
    [Tooltip("Delay before this slot fires in the current volley.")]
    public float spawnDelay;

    [Header("On-Hit Effects")]
    public bool appliesSlow;
    [Range(0f, 0.95f)] public float slowPercent;
    [Min(0f)] public float slowDuration;
}

[CreateAssetMenu(fileName = "EnemyWeapon", menuName = "TakeFlight/Enemy Weapon Definition")]
public class EnemyWeaponDefinition : ScriptableObject
{
    [Header("Pattern")]
    public EnemyShotSlot[] shotSlots;
    [Min(0.05f)] public float fireRate = 1f;
    [Min(1)] public int burstCount = 1;
    [Min(0f)] public float burstInterval = 0f;

    [Header("Targeting")]
    public string defaultTargetTag = "Player";
    public string sourceId = "EnemyWeapon";
}
