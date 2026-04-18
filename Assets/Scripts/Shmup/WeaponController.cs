using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Reads primary/secondary weapons from InventoryManager and optional defaults.
// Primary = limited ammo typical; secondary = infinite fallback. Switch with SwitchWeapon input.
// Attach to the player GameObject alongside PlayerController.
public class WeaponController : MonoBehaviour
{
    [SerializeField] private Transform[] barrels;

    [Tooltip("Used when no primary is equipped or InventoryManager is unavailable.")]
    [SerializeField] private ItemDefinition defaultWeapon;
    [Tooltip("Used when no secondary is equipped or InventoryManager is unavailable.")]
    [SerializeField] private ItemDefinition defaultSecondaryWeapon;

    public int CurrentAmmo { get; private set; }
    public int MaxAmmo { get; private set; }
    public bool IsInfiniteAmmo { get; private set; }
    public ItemDefinition ActiveWeapon { get; private set; }
    public ItemDefinition PrimaryWeapon { get; private set; }
    public ItemDefinition SecondaryWeapon { get; private set; }
    public bool IsPrimaryActive => _activeIsPrimary;

    /// <summary>Current primary magazine count (0 if no primary or primary is infinite).</summary>
    public int PrimaryAmmo => _primaryDef != null && !_primaryDef.infiniteAmmo ? _primaryAmmo : 0;

    public int PrimaryMaxAmmo => _primaryDef != null ? _primaryDef.ammoCapacity : 0;
    public bool PrimaryIsInfinite => _primaryDef != null && _primaryDef.infiniteAmmo;

    public event System.Action OnAmmoChanged;

    private ItemDefinition _primaryDef;
    private ItemDefinition _secondaryDef;
    private int _primaryAmmo;
    private int _secondaryAmmo;
    private bool _activeIsPrimary = true;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction switchWeaponAction;
    private float nextFireTime;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        TryResolveActions();
    }

    void OnEnable()
    {
        TryResolveActions();
        if (switchWeaponAction != null)
        {
            switchWeaponAction.performed -= OnSwitchWeaponPerformed;
            switchWeaponAction.performed += OnSwitchWeaponPerformed;
        }
    }

    void OnDisable()
    {
        if (switchWeaponAction != null)
            switchWeaponAction.performed -= OnSwitchWeaponPerformed;
    }

    void Start()
    {
        LoadEquippedWeapon();
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;
        if (attackAction == null)
            TryResolveActions();
        if (attackAction == null)
            return;

        if (ActiveWeapon == null) return;
        if (attackAction.IsPressed() && Time.time >= nextFireTime)
            TryFire();
    }

    private void TryResolveActions()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        attackAction = actions["Attack"];
        if (actions.FindAction("SwitchWeapon") != null)
            switchWeaponAction = actions["SwitchWeapon"];
    }

    private void OnSwitchWeaponPerformed(InputAction.CallbackContext _)
    {
        TrySwitchWeapon();
    }

    public void TrySwitchWeapon()
    {
        if (_primaryDef == null || _secondaryDef == null)
            return;

        _activeIsPrimary = !_activeIsPrimary;
        SyncActiveWeaponFromSlot();
        GetComponent<ShipCosmeticApplicator>()?.Apply();
        OnAmmoChanged?.Invoke();
    }

    // Call when re-entering a level to refresh weapons from save data.
    public void LoadEquippedWeapon()
    {
        _primaryDef = null;
        _secondaryDef = null;

        if (InventoryManager.Instance != null)
        {
            string primaryId = InventoryManager.Instance.EquippedWeaponID;
            if (!string.IsNullOrEmpty(primaryId))
                _primaryDef = InventoryManager.Instance.GetDefinition(primaryId);

            string secondaryId = InventoryManager.Instance.EquippedSecondaryWeaponID;
            if (!string.IsNullOrEmpty(secondaryId))
                _secondaryDef = InventoryManager.Instance.GetDefinition(secondaryId);
        }

        if (_primaryDef == null)
            _primaryDef = defaultWeapon;
        if (_secondaryDef == null)
            _secondaryDef = defaultSecondaryWeapon;

        if (_primaryDef != null && _secondaryDef != null &&
            !string.IsNullOrEmpty(_primaryDef.itemID) && _primaryDef.itemID == _secondaryDef.itemID)
            _secondaryDef = null;

        PrimaryWeapon = _primaryDef;
        SecondaryWeapon = _secondaryDef;

        if (_primaryDef != null)
        {
            _primaryAmmo = _primaryDef.infiniteAmmo ? 0 : _primaryDef.ammoCapacity;
        }
        else
            _primaryAmmo = 0;

        if (_secondaryDef != null)
        {
            _secondaryAmmo = _secondaryDef.infiniteAmmo ? 0 : _secondaryDef.ammoCapacity;
        }
        else
            _secondaryAmmo = 0;

        _activeIsPrimary = _primaryDef != null;
        if (_primaryDef == null && _secondaryDef != null)
            _activeIsPrimary = false;

        SyncActiveWeaponFromSlot();

        if (_primaryDef == null && _secondaryDef == null)
            Debug.LogWarning("[WeaponController] No primary or secondary weapon resolved.");

        GetComponent<ShipCosmeticApplicator>()?.Apply();
        OnAmmoChanged?.Invoke();
    }

    private void SyncActiveWeaponFromSlot()
    {
        ActiveWeapon = _activeIsPrimary ? _primaryDef : _secondaryDef;

        if (ActiveWeapon == null)
        {
            MaxAmmo = 0;
            CurrentAmmo = 0;
            IsInfiniteAmmo = false;
            return;
        }

        MaxAmmo = ActiveWeapon.ammoCapacity;
        IsInfiniteAmmo = ActiveWeapon.infiniteAmmo;
        CurrentAmmo = _activeIsPrimary ? _primaryAmmo : _secondaryAmmo;
        if (IsInfiniteAmmo)
            CurrentAmmo = MaxAmmo;
    }

    /// <summary>Adds ammo to the primary weapon only (shmup pickups).</summary>
    public void AddAmmo(int amount)
    {
        if (_primaryDef == null || _primaryDef.infiniteAmmo)
            return;

        int prev = _primaryAmmo;
        _primaryAmmo = Mathf.Min(_primaryAmmo + amount, _primaryDef.ammoCapacity);
        if (_primaryAmmo == prev)
            return;

        if (_activeIsPrimary)
            CurrentAmmo = _primaryAmmo;
        OnAmmoChanged?.Invoke();
    }

    private void TryFire()
    {
        if (ActiveWeapon == null) return;

        if (_activeIsPrimary)
        {
            if (!_primaryDef.infiniteAmmo && _primaryAmmo <= 0) return;
        }
        else
        {
            if (!_secondaryDef.infiniteAmmo && _secondaryAmmo <= 0) return;
        }

        if (ActiveWeapon.bulletSlots == null || ActiveWeapon.bulletSlots.Length == 0)
        {
            Debug.LogWarning("[WeaponController] Weapon has no bullet slots assigned.");
            return;
        }

        nextFireTime = Time.time + ActiveWeapon.fireRate;
        StartCoroutine(FirePatternRoutine());

        if (_activeIsPrimary)
        {
            if (!_primaryDef.infiniteAmmo)
            {
                _primaryAmmo--;
                CurrentAmmo = _primaryAmmo;
            }
        }
        else
        {
            if (!_secondaryDef.infiniteAmmo)
            {
                _secondaryAmmo--;
                CurrentAmmo = _secondaryAmmo;
            }
        }

        OnAmmoChanged?.Invoke();
    }

    private IEnumerator FirePatternRoutine()
    {
        int burstCount = Mathf.Max(1, ActiveWeapon.burstCount);
        for (int burstIndex = 0; burstIndex < burstCount; burstIndex++)
        {
            foreach (var slot in ActiveWeapon.bulletSlots)
            {
                if (slot.spawnDelay > 0f)
                    StartCoroutine(FireSlotAfterDelay(slot));
                else
                    FireSlot(slot);
            }

            if (burstIndex < burstCount - 1 && ActiveWeapon.burstInterval > 0f)
                yield return new WaitForSeconds(ActiveWeapon.burstInterval);
        }
    }

    private IEnumerator FireSlotAfterDelay(BulletSlot slot)
    {
        yield return new WaitForSeconds(slot.spawnDelay);
        FireSlot(slot);
    }

    private void FireSlot(BulletSlot slot)
    {
        if (slot.bulletPrefab == null)
            return;

        if (slot.barrelIndex < 0 || slot.barrelIndex >= barrels.Length)
        {
            Debug.LogWarning($"[WeaponController] Barrel index {slot.barrelIndex} out of range.");
            return;
        }

        Transform barrel = barrels[slot.barrelIndex];
        if (barrel == null)
            return;

        Quaternion shotRotation = barrel.rotation * Quaternion.Euler(0f, slot.angleOffset, 0f);
        Vector3 spawnPosition = barrel.position + barrel.TransformVector(slot.positionOffset);
        GameObject spawned = Instantiate(slot.bulletPrefab, spawnPosition, shotRotation);
        ApplySlotEffects(spawned, slot, shotRotation);
    }

    private void ApplySlotEffects(GameObject spawned, BulletSlot slot, Quaternion shotRotation)
    {
        if (spawned == null)
            return;

        if (spawned.TryGetComponent(out Bullet bullet))
        {
            bullet.SetTargetTag("Enemy");
            bullet.SetDirection(shotRotation * Vector3.forward);
            bullet.ConfigureEffects(slot.appliesSlow, slot.slowPercent, slot.slowDuration, ActiveWeapon.itemID);
        }

        if (spawned.TryGetComponent(out ArenaBullet arenaBullet))
        {
            arenaBullet.Init(shotRotation * Vector3.forward, "Enemy");
            arenaBullet.ConfigureEffects(slot.appliesSlow, slot.slowPercent, slot.slowDuration, ActiveWeapon.itemID);
        }
    }
}
