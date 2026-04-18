using UnityEngine;

// Place one instance of this in the ShmupLevel scene.
// It reads the current LevelDefinition from SceneTransitionManager and starts the level.
public class ShmupSceneBootstrap : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private BackgroundEventPlayer backgroundEventPlayer; // optional
    [SerializeField] private LevelCompleteUI levelCompleteUI;
    [SerializeField] private LevelDefinition debugLevel; // assigned in Inspector for direct Play-mode testing

    void Start()
    {
        var level = SceneTransitionManager.Instance?.CurrentLevel ?? debugLevel;

        if (level == null)
        {
            Debug.LogWarning("[ShmupSceneBootstrap] No LevelDefinition found. Assign a Debug Level in the Inspector for direct testing.");
            return;
        }

        // Hook into level end conditions
        waveManager.onAllWavesComplete += OnLevelWon;
        GameManager.Instance.onGameOver += OnLevelLost;

        waveManager.StartWaves(level.waves);
        backgroundEventPlayer?.Play();
    }

    void OnDestroy()
    {
        if (waveManager != null)
            waveManager.onAllWavesComplete -= OnLevelWon;

        if (GameManager.Instance != null)
            GameManager.Instance.onGameOver -= OnLevelLost;

        if (levelCompleteUI != null)
            levelCompleteUI.onContinue -= OnContinueAfterResults;
    }

    void OnLevelWon()
    {
        waveManager.onAllWavesComplete -= OnLevelWon;
        GameManager.Instance.onGameOver -= OnLevelLost;

        var level = SceneTransitionManager.Instance?.CurrentLevel ?? debugLevel;

        LevelDefinition.GetMissionRewards(
            level,
            GameManager.Instance.EnemiesKilled,
            out int creditReward,
            out int xpReward,
            out int qualifyingTierMinKills);
        string levelName = level != null ? level.displayName : "Unknown";

        XPManager.Instance?.AddXP(xpReward);
        InventoryManager.Instance?.AddCurrency(creditReward);

        if (levelCompleteUI != null)
        {
            levelCompleteUI.Show(
                GameManager.Instance.Score,
                GameManager.Instance.EnemiesKilled,
                xpReward,
                creditReward,
                levelName,
                level,
                qualifyingTierMinKills);
            levelCompleteUI.onContinue += OnContinueAfterResults;
        }
        else
        {
            ReturnToOverworld();
        }
    }

    void OnContinueAfterResults()
    {
        levelCompleteUI.onContinue -= OnContinueAfterResults;
        ReturnToOverworld();
    }

    void ReturnToOverworld()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ReturnToOverworld(won: true);
        else
            Debug.Log("[ShmupSceneBootstrap] Level won (debug play — no transition manager).");
    }

    void OnLevelLost()
    {
        waveManager.onAllWavesComplete -= OnLevelWon;
        GameManager.Instance.onGameOver -= OnLevelLost;

        // Stay in scene: UIManager shows game-over panel; player uses Return / Restart there.
        if (SceneTransitionManager.Instance == null)
            Debug.Log("[ShmupSceneBootstrap] Level lost (debug play — no transition manager). Use game-over panel.");
    }
}
