using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("Replacement")]
    [SerializeField] private GameObject _replacement;
    [SerializeField] private GameObject _explosionVfxPrefab;
    [SerializeField] private float _explosionVfxLifetime = 3f;
    [SerializeField] private float _destroyDelay = 0f;

    [Header("Break Settings")]
    [SerializeField] private float _breakForce = 2f;
    [SerializeField] private float _collisionMultiplier = 100f;
    [SerializeField] private float _explosionRadius = 2f;
    [SerializeField] private float _upwardModifier = 0f;

    [Header("State")]
    [SerializeField] private bool _broken;

    private void OnCollisionEnter(Collision collision)
    {
        if (_broken || collision == null)
        {
            return;
        }

        if (collision.gameObject == gameObject || collision.transform.root == transform.root)
        {
            return;
        }

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < _breakForce)
        {
            return;
        }

        Break(collision, impactSpeed);
    }

    private void Break(Collision collision, float impactSpeed)
    {
        _broken = true;

        if (_replacement == null)
        {
            Debug.LogWarning($"Breakable on '{name}' has no replacement prefab assigned.", this);
            Destroy(gameObject, _destroyDelay);
            return;
        }

        GameObject replacement = Instantiate(_replacement, transform.position, transform.rotation);

        Vector3 explosionPoint = transform.position;
        if (collision.contactCount > 0)
        {
            explosionPoint = collision.GetContact(0).point;
        }

        float explosionForce = impactSpeed * _collisionMultiplier;
        Rigidbody[] rigidbodies = GetFragmentRigidbodies(replacement);

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.AddExplosionForce(explosionForce, explosionPoint, _explosionRadius, _upwardModifier, ForceMode.Impulse);
        }

        SpawnExplosionVfx(explosionPoint);
        Destroy(gameObject, _destroyDelay);
    }

    private void SpawnExplosionVfx(Vector3 position)
    {
        if (_explosionVfxPrefab == null)
        {
            return;
        }

        GameObject vfxInstance = Instantiate(_explosionVfxPrefab, position, Quaternion.identity);
        if (_explosionVfxLifetime > 0f)
        {
            Destroy(vfxInstance, _explosionVfxLifetime);
        }
    }

    private Rigidbody[] GetFragmentRigidbodies(GameObject replacement)
    {
        Rigidbody[] rigidbodies = replacement.GetComponentsInChildren<Rigidbody>();
        if (rigidbodies.Length > 0)
        {
            return rigidbodies;
        }

        Collider[] colliders = replacement.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = col.gameObject.AddComponent<Rigidbody>();
            }
            rb.useGravity = false;
        }

        rigidbodies = replacement.GetComponentsInChildren<Rigidbody>();
        if (rigidbodies.Length == 0)
        {
            Debug.LogWarning($"Replacement '{replacement.name}' has no colliders to convert into fragments.", replacement);
        }

        return rigidbodies;
    }
}
