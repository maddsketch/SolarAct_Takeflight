using UnityEngine;
using UnityEngine.SceneManagement;

// Scene-level singleton for the arena.
// Tracks enemy kill count and fires events for wave/level completion.
public class ArenaGameManager : MonoBehaviour
{
    public static ArenaGameManager Instance { get; private set; }

    public int EnemiesRemaining { get; private set; }

    public event System.Action<int> onEnemiesRemainingChanged; // remaining count
    public event System.Action onAllEnemiesKilled;
    public event System.Action onPlayerDeath;

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float deathSlowTimeScale = 0.35f;
    [SerializeField] private float deathSlowDuration = 0.8f;

    private bool isDeathSequenceRunning;
    private float defaultFixedDeltaTime;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterEnemies(int count)
    {
        EnemiesRemaining += count;
        onEnemiesRemainingChanged?.Invoke(EnemiesRemaining);
    }

    public void OnEnemyKilled()
    {
        EnemiesRemaining = Mathf.Max(0, EnemiesRemaining - 1);
        onEnemiesRemainingChanged?.Invoke(EnemiesRemaining);

        if (EnemiesRemaining == 0)
            onAllEnemiesKilled?.Invoke();
    }

    public void OnPlayerDeath()
    {
        if (isDeathSequenceRunning) return;
        StartCoroutine(RunDeathSlowMotionSequence());
    }

    // --- Button callbacks ---

    public void ReturnToOverworld()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance?.ReturnToOverworld(won: false);
    }

    public void RestartArena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private System.Collections.IEnumerator RunDeathSlowMotionSequence()
    {
        isDeathSequenceRunning = true;

        float clampedScale = Mathf.Clamp(deathSlowTimeScale, 0.01f, 1f);
        Time.timeScale = clampedScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * clampedScale;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, deathSlowDuration));

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        onPlayerDeath?.Invoke();
        isDeathSequenceRunning = false;
    }
}
