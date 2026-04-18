using UnityEngine;

[System.Serializable]
public struct BulletSlot
{
    public GameObject bulletPrefab;
    [Tooltip("Index into WeaponController's Barrels array")]
    public int barrelIndex;
    [Tooltip("Yaw offset in degrees from the barrel's forward direction")]
    public float angleOffset;
    [Tooltip("Local offset from the selected barrel")]
    public Vector3 positionOffset;
    [Tooltip("Delay before this slot fires (seconds)")]
    public float spawnDelay;
    [Header("On-Hit Effects")]
    public bool appliesSlow;
    [Range(0f, 0.95f)] public float slowPercent;
    [Min(0f)] public float slowDuration;
}

[CreateAssetMenu(fileName = "New Item", menuName = "TakeFlight/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("General")]
    public string itemID;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemCategory category;
    public int maxStackSize = 1;
    public int shopPrice;

    [Header("Level Gate")]
    public int requiredLevel = 1;

    [Header("Weapon")]
    public BulletSlot[] bulletSlots;
    public float fireRate = 0.15f;
    public int ammoCapacity = 100;
    public bool infiniteAmmo = false;
    [Min(1)] public int burstCount = 1;
    [Min(0f)] public float burstInterval = 0f;

    [Header("Upgrade")]
    public UpgradeType upgradeType;
    public float upgradeValue;

    [Header("Consumable")]
    public ConsumableEffect consumableEffect;
    public float effectValue;

    [Header("Cosmetic")]
    public bool includesCosmetic;
    public CosmeticType cosmeticType;
    public GameObject hullPrefab;
    public Material skinMaterial;
    public GameObject accessoryPrefab;
    [Tooltip("Index into ShipCosmeticApplicator's accessoryMountPoints array")]
    public int accessorySlot;
}
