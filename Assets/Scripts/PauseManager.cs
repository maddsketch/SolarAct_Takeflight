using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

// Place in the Shmup level scene alongside UIManager.
// Handles Escape to pause, Resume/Restart/Quit buttons, and timeScale.
public class PauseManager : MonoBehaviour
{
    private enum RestartBehavior
    {
        ReloadActiveScene = 0,
        UseGameManagerRestart = 1
    }

    public static PauseManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;   // reference so we can't pause while dead
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private RestartBehavior restartBehavior = RestartBehavior.ReloadActiveScene;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string pauseActionName = "Pause";
    [SerializeField] private TextMeshProUGUI saveStatusText;
    [SerializeField] private float saveStatusSeconds = 1.5f;

    private InputAction pauseAction;
    private float saveStatusHideTime = -1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (saveStatusText != null) saveStatusText.text = string.Empty;

        // Unpause cleanly in case timeScale was left at 0 from a previous session
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (WasPauseRequested())
            Toggle();

        UpdateSaveStatusText();
    }

    // --- Public API (wired to buttons) ---

    public void Toggle()
    {
        // Don't allow pausing while game over is showing
        if (!IsPaused && gameOverPanel != null && gameOverPanel.activeSelf) return;

        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (restartBehavior == RestartBehavior.UseGameManagerRestart && GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            return;
        }

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SaveState()
    {
        int slot = SceneTransitionManager.Instance?.ActiveSaveSlot ?? 0;
        if (GameStateManager.Instance == null)
        {
            ShowSaveStatus("Save unavailable");
            return;
        }

        GameStateManager.Instance.Save(slot);
        ShowSaveStatus("Saved");
    }

    private bool WasPauseRequested()
    {
        if (pauseAction == null && playerInput != null && playerInput.actions != null)
            pauseAction = playerInput.actions.FindAction(pauseActionName, false);

        if (pauseAction != null && pauseAction.WasPressedThisFrame())
            return true;

        return (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
               (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame);
    }

    private void ShowSaveStatus(string message)
    {
        if (saveStatusText == null)
            return;

        saveStatusText.text = message;
        saveStatusHideTime = Time.unscaledTime + Mathf.Max(0.1f, saveStatusSeconds);
    }

    private void UpdateSaveStatusText()
    {
        if (saveStatusText == null || saveStatusHideTime < 0f)
            return;

        if (Time.unscaledTime < saveStatusHideTime)
            return;

        saveStatusText.text = string.Empty;
        saveStatusHideTime = -1f;
    }
}
