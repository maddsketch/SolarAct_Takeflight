using UnityEngine;

// Attach to the player GameObject in shmup and arena scenes.
// On Start, reads cumulative stat bonuses from SaveData and applies them
// to the player's components. Re-applies if ShipUpgradeManager fires onStatsChanged.
public class ShipUpgradeApplicator : MonoBehaviour
{
    [Header("Base values — match what's set in each component's Inspector")]
    [SerializeField] private float baseSpeed      = 8f;
    [SerializeField] private int   baseMaxHealth  = 3;
    [SerializeField] private float baseFireRate   = 0.15f;
    [SerializeField] private float baseShield     = 0f;   // base invincibility duration

    private PlayerController playerController;
    private Health           health;
    private PlayerShooter    shooter;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        health           = GetComponent<Health>();
        shooter          = GetComponent<PlayerShooter>();
    }

    void Start()
    {
        Apply();

        if (ShipUpgradeManager.Instance != null)
            ShipUpgradeManager.Instance.onStatsChanged += Apply;
    }

    void OnDestroy()
    {
        if (ShipUpgradeManager.Instance != null)
            ShipUpgradeManager.Instance.onStatsChanged -= Apply;
    }

    private void Apply()
    {
        var data = GameStateManager.Instance?.Current;
        if (data == null) return;

        if (playerController != null)
            playerController.SetSpeed(baseSpeed + data.speedBonus);

        if (health != null)
        {
            health.SetMaxHealth(baseMaxHealth + data.maxHealthBonus);

            // Restore saved HP — if no saved HP yet, default to full
            int savedHP = data.playerHealth > 0 ? data.playerHealth : health.Max;
            health.SetCurrentHealth(savedHP);
        }

        if (shooter != null)
            shooter.SetFireRate(Mathf.Max(0.05f, baseFireRate + data.fireRateBonus));

        if (health != null)
            health.SetInvincible(baseShield);
    }
}
