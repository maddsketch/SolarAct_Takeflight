using UnityEngine;

// Persistent singleton. Listens to XPManager.onLevelUp and applies
// matching entries from ShipUpgradeTable to the SaveData bonus values.
public class ShipUpgradeManager : MonoBehaviour
{
    public static ShipUpgradeManager Instance { get; private set; }

    [SerializeField] private ShipUpgradeTable upgradeTable;

    // Fired after bonuses are updated so scene components can re-apply them.
    public event System.Action onStatsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp += OnLevelUp;
    }

    void OnDisable()
    {
        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp -= OnLevelUp;
    }

    private void OnLevelUp(int newLevel)
    {
        if (upgradeTable == null) return;

        var data = GameStateManager.Instance.Current;
        bool changed = false;

        foreach (var entry in upgradeTable.entries)
        {
            if (entry.atLevel != newLevel) continue;

            switch (entry.stat)
            {
                case UpgradeType.Speed:
                    data.speedBonus += entry.value;
                    Debug.Log($"[ShipUpgradeManager] Speed +{entry.value} at level {newLevel}");
                    break;
                case UpgradeType.MaxHealth:
                    data.maxHealthBonus += Mathf.RoundToInt(entry.value);
                    Debug.Log($"[ShipUpgradeManager] MaxHealth +{entry.value} at level {newLevel}");
                    break;
                case UpgradeType.FireRate:
                    data.fireRateBonus += entry.value;
                    Debug.Log($"[ShipUpgradeManager] FireRate {entry.value} at level {newLevel}");
                    break;
                case UpgradeType.Shield:
                    data.shieldBonus += entry.value;
                    Debug.Log($"[ShipUpgradeManager] Shield +{entry.value}s at level {newLevel}");
                    break;
            }
            changed = true;
        }

        if (changed) onStatsChanged?.Invoke();
    }
}
