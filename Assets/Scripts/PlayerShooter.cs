using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;

    private PlayerInput playerInput;
    private InputAction attackAction;
    private float nextFireTime;

    public void SetFireRate(float value) => fireRate = value;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        TryResolveAttackAction();
    }

    void OnEnable()
    {
        TryResolveAttackAction();
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;
        if (attackAction == null)
            TryResolveAttackAction();
        if (attackAction == null)
            return;

        if (attackAction.IsPressed() && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        }
    }

    private void TryResolveAttackAction()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        attackAction = actions["Attack"];
    }
}
