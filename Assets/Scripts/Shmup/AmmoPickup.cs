using UnityEngine;

// Place on a trigger collider in a shmup level.
// When the player touches it, adds ammo to the primary weapon only (dual-weapon loadout).
// If primary has infiniteAmmo or no primary exists, the pickup is still consumed but has no effect.
public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private int ammoAmount = 20;
    [SerializeField] private GameObject pickupVfxPrefab;
    [SerializeField] private float pickupVfxLifetime = 2f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var weapon = other.GetComponent<WeaponController>();
        if (weapon == null) return;

        weapon.AddAmmo(ammoAmount);
        SpawnPickupVfx();
        Destroy(gameObject);
    }

    private void SpawnPickupVfx()
    {
        if (pickupVfxPrefab == null) return;

        GameObject vfx = Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);
        if (pickupVfxLifetime > 0f)
            Destroy(vfx, pickupVfxLifetime);
    }
}
