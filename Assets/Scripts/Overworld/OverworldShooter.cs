using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the overworld player alongside OverworldPlayerController.
// Fires bullets in the direction the player is facing when Fire is pressed.
[RequireComponent(typeof(OverworldPlayerController))]
public class OverworldShooter : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;       // empty child at the front of the ship
    [SerializeField] private float fireRate = 0.25f;    // seconds between shots

    private float fireTimer;

    void OnFire(InputValue value)
    {
        if (!value.isPressed) return;
        TryShoot();
    }

    void Update()
    {
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;
    }

    private void TryShoot()
    {
        if (fireTimer > 0f) return;
        if (bulletPrefab == null) return;

        fireTimer = fireRate;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        var go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Point bullet in the direction the player is facing (forward on X/Z plane)
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            var dir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            bullet.SetDirection(dir);
            bullet.SetTargetTag("Enemy");
        }
    }
}
