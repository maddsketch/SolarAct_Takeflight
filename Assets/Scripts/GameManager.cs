using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int EnemiesKilled { get; private set; }

    public event System.Action<int> onScoreChanged;
    public event System.Action<int> onKillCountChanged;
    public event System.Action onGameOver;

    [SerializeField] private float deathSlowTimeScale = 0.35f;
    [SerializeField] private float deathSlowDuration = 0.8f;

    private bool isDeathSequenceRunning;
    private float defaultFixedDeltaTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddScore(int amount)
    {
        Score += amount;
        onScoreChanged?.Invoke(Score);
    }

    public void RegisterKill()
    {
        EnemiesKilled++;
        onKillCountChanged?.Invoke(EnemiesKilled);
    }

    public void OnPlayerDeath()
    {
        if (isDeathSequenceRunning) return;
        StartCoroutine(RunDeathSlowMotionSequence());
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        onGameOver?.Invoke();
        isDeathSequenceRunning = false;
    }
}
