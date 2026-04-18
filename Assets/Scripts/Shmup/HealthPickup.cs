using UnityEngine;

// Trigger pickup that heals any collector with a Health component.
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float lifetime = 10f; // <= 0 disables auto-despawn
    [SerializeField] private GameObject pickupVfxPrefab;
    [SerializeField] private float pickupVfxLifetime = 2f;

    void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health == null) return;

        health.Heal(Mathf.Max(1, healAmount));
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
