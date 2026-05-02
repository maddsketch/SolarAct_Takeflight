using UnityEngine;

public class ShipCosmeticApplicator : MonoBehaviour
{
    [Header("Hull")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private GameObject defaultHullPrefab;

    [Header("Skin")]
    [SerializeField] private Renderer shipRenderer;
    [SerializeField] private Material defaultMaterial;

    [Header("Accessories")]
    [SerializeField] private Transform[] accessoryMountPoints;

    private GameObject currentHullInstance;
    private Material skinMaterialInstance;
    private Renderer skinMaterialRenderer;
    private Material skinSourceAsset;

    void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCosmeticChanged += Apply;

        Apply();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnCosmeticChanged -= Apply;

        if (skinMaterialInstance != null)
        {
            Destroy(skinMaterialInstance);
            skinMaterialInstance = null;
        }
        skinMaterialRenderer = null;
        skinSourceAsset = null;
    }

    public void Apply()
    {
        var weaponCosmetic = ResolveWeaponCosmetic();

        ApplyHull(weaponCosmetic);
        ApplySkin(weaponCosmetic);
        ApplyAccessories(weaponCosmetic);
    }

    /// <summary>
    /// Weapon-mounted hull/skin/accessory from items: on shmup player, follows the active
    /// weapon slot (primary/secondary). If that weapon has no cosmetic, do not fall back to
    /// the other slot. Overworld (no WeaponController) uses inventory primary only.
    /// </summary>
    private ItemDefinition ResolveWeaponCosmetic()
    {
        var weaponController = GetComponent<WeaponController>();
        if (weaponController != null)
        {
            var active = weaponController.ActiveWeapon;
            if (active != null)
                return active.includesCosmetic ? active : null;
        }

        return ResolveEquippedWeaponCosmetic();
    }

    private ItemDefinition ResolveEquippedWeaponCosmetic()
    {
        if (InventoryManager.Instance == null)
            return null;

        string weaponID = InventoryManager.Instance.EquippedWeaponID;
        if (string.IsNullOrEmpty(weaponID))
            return null;

        var def = InventoryManager.Instance.GetDefinition(weaponID);
        return (def != null && def.includesCosmetic) ? def : null;
    }

    private void ApplyHull(ItemDefinition weaponCosmetic)
    {
        string hullID = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedHullID : null;
        GameObject prefab = defaultHullPrefab;

        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(hullID))
        {
            var def = InventoryManager.Instance.GetDefinition(hullID);
            if (def != null && def.hullPrefab != null)
                prefab = def.hullPrefab;
        }
        else if (weaponCosmetic != null && weaponCosmetic.hullPrefab != null)
        {
            prefab = weaponCosmetic.hullPrefab;
        }

        if (prefab == gameObject)
        {
            Debug.LogWarning("[ShipCosmeticApplicator] defaultHullPrefab points to the player root; skipping self-instantiation.");
            return;
        }

        if (prefab == null || modelParent == null) return;

        if (currentHullInstance != null)
            Destroy(currentHullInstance);

        currentHullInstance = Instantiate(prefab, modelParent);
        currentHullInstance.transform.localPosition = Vector3.zero;
        currentHullInstance.transform.localRotation = Quaternion.identity;

        var newRenderer = currentHullInstance.GetComponentInChildren<Renderer>();
        if (newRenderer != null)
            shipRenderer = newRenderer;
    }

    private void ApplySkin(ItemDefinition weaponCosmetic)
    {
        if (shipRenderer == null) return;

        string skinID = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedSkinID : null;
        Material mat = defaultMaterial;

        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(skinID))
        {
            var def = InventoryManager.Instance.GetDefinition(skinID);
            if (def != null && def.skinMaterial != null)
                mat = def.skinMaterial;
        }
        else if (weaponCosmetic != null && weaponCosmetic.skinMaterial != null)
        {
            mat = weaponCosmetic.skinMaterial;
        }

        if (mat == null) return;

        if (skinMaterialRenderer != shipRenderer || skinSourceAsset != mat)
        {
            if (skinMaterialInstance != null)
            {
                Destroy(skinMaterialInstance);
                skinMaterialInstance = null;
            }

            skinMaterialRenderer = shipRenderer;
            skinSourceAsset = mat;
            skinMaterialInstance = new Material(mat);
            shipRenderer.material = skinMaterialInstance;
        }
    }

    private void ApplyAccessories(ItemDefinition weaponCosmetic)
    {
        if (accessoryMountPoints == null) return;

        foreach (var mount in accessoryMountPoints)
        {
            if (mount == null) continue;
            for (int i = mount.childCount - 1; i >= 0; i--)
                Destroy(mount.GetChild(i).gameObject);
        }

        var ids = InventoryManager.Instance != null ? InventoryManager.Instance.EquippedAccessoryIDs : null;
        if (ids != null)
        {
            foreach (string accID in ids)
            {
                var def = InventoryManager.Instance.GetDefinition(accID);
                if (def == null || def.accessoryPrefab == null) continue;

                int slot = def.accessorySlot;
                if (slot < 0 || slot >= accessoryMountPoints.Length)
                {
                    Debug.LogWarning($"[ShipCosmeticApplicator] Accessory '{accID}' slot {slot} out of range.");
                    continue;
                }

                AttachAccessory(def.accessoryPrefab, slot);
            }
        }

        if (weaponCosmetic != null && weaponCosmetic.accessoryPrefab != null)
        {
            int slot = weaponCosmetic.accessorySlot;
            if (slot >= 0 && slot < accessoryMountPoints.Length)
            {
                Transform mount = accessoryMountPoints[slot];
                if (mount != null)
                {
                    // Weapon cosmetic should reflect the active weapon loadout.
                    // If an equipped accessory already occupies this slot, replace it.
                    for (int i = mount.childCount - 1; i >= 0; i--)
                        Destroy(mount.GetChild(i).gameObject);

                    AttachAccessory(weaponCosmetic.accessoryPrefab, slot);
                }
            }
        }
    }

    private void AttachAccessory(GameObject prefab, int slot)
    {
        Transform mount = accessoryMountPoints[slot];
        if (mount == null) return;

        var go = Instantiate(prefab, mount);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
