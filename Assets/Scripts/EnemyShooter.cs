using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float firstShotDelay = 0.5f;

    void Start()
    {
        InvokeRepeating(nameof(Fire), firstShotDelay, fireRate);
    }

    void Fire()
    {
        if (bulletPrefab != null && firePoint != null)
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
    }
}
