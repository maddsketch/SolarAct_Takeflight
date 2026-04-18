using UnityEngine;

// Persistent singleton — survives scene loads.
// Reads and writes XP/level directly to GameStateManager.Current.
public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    public const int MaxLevel = 20;

    // Fired when XP changes: (currentXP, xpForNextLevel)
    public event System.Action<int, int> onXPChanged;

    // Fired when the player levels up: (newLevel)
    public event System.Action<int> onLevelUp;

    public int PlayerLevel => GameStateManager.Instance?.Current.playerLevel ?? 1;
    public int CurrentXP   => GameStateManager.Instance?.Current.currentXP ?? 0;
    public int XPToNextLevel => XPRequired(PlayerLevel);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddXP(int amount)
    {
        if (GameStateManager.Instance == null) return;
        if (PlayerLevel >= MaxLevel) return;

        var data = GameStateManager.Instance.Current;
        data.currentXP += amount;

        while (data.playerLevel < MaxLevel && data.currentXP >= XPRequired(data.playerLevel))
        {
            data.currentXP -= XPRequired(data.playerLevel);
            data.playerLevel++;
            GameStateManager.Instance.SetFlag($"player_level_{data.playerLevel}");
            onLevelUp?.Invoke(data.playerLevel);
            Debug.Log($"[XPManager] Level up! Now level {data.playerLevel}");
        }

        onXPChanged?.Invoke(data.currentXP, XPToNextLevel);
    }

    // XP needed to go from level n to n+1.
    // Level 1→2: 100, 2→3: 283, 3→4: 520, 4→5: 800 ...
    public static int XPRequired(int level) =>
        Mathf.RoundToInt(100f * Mathf.Pow(level, 1.5f));
}
