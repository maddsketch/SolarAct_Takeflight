using UnityEngine;

// Place on a trigger collider prefab.
// Dropped by enemies on death — player collects by flying over it.
public class CreditPickup : MonoBehaviour
{
    [SerializeField] private int creditAmount = 50;
    [SerializeField] private float lifetime = 10f; // despawns if not collected
    [SerializeField] private GameObject pickupVfxPrefab;
    [SerializeField] private float pickupVfxLifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryManager.Instance?.AddCurrency(creditAmount);
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
