using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject gameOverPanel;
    private Health playerHealth;
    [SerializeField] private int lowHealthThreshold = 2;
    [SerializeField] private Image lowHealthWarningImage;
    [SerializeField] private Color lowHealthFlashColor = Color.red;
    [SerializeField] private float lowHealthPulseSpeed = 8f;

    private Coroutine lowHealthPulseRoutine;
    private bool isLowHealthActive;
    private bool gameManagerSubscribed;

    void Start()
    {
        ResolvePlayerHealth();
        TrySubscribeGameManager();

        UpdateScore(0);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        SetLowHealthVisuals(false, 0f);
    }

    void Update()
    {
        TrySubscribeGameManager();

        if (playerHealth == null)
            ResolvePlayerHealth();
    }

    void OnDestroy()
    {
        if (gameManagerSubscribed && GameManager.Instance != null)
        {
            GameManager.Instance.onScoreChanged -= UpdateScore;
            GameManager.Instance.onGameOver -= ShowGameOver;
        }

        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(UpdateHealth);

        StopLowHealthPulse();
        SetLowHealthVisuals(false, 0f);
    }

    private void TrySubscribeGameManager()
    {
        if (gameManagerSubscribed || GameManager.Instance == null)
            return;

        GameManager.Instance.onScoreChanged += UpdateScore;
        GameManager.Instance.onGameOver += ShowGameOver;
        gameManagerSubscribed = true;
    }

    private void ResolvePlayerHealth()
    {
        var resolved = GameObject.FindWithTag("Player")?.GetComponent<Health>();
        if (resolved == playerHealth)
            return;

        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(UpdateHealth);

        playerHealth = resolved;

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealth);
            UpdateHealth(playerHealth.Current, playerHealth.Max);
        }
    }

    void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{score:D6}";
    }

    void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";

        EvaluateLowHealth(current);
    }

    void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Wire this to a Restart button in the Game Over panel
    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartGame();
    }

    // Wire to a Return / Quit button on the Game Over panel (leaves shmup like old auto-loss)
    public void OnReturnToOverworldFromGameOver()
    {
        Time.timeScale = 1f;
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.ReturnToOverworld(won: false);
        else
            Debug.Log("[UIManager] No SceneTransitionManager — cannot return to overworld.");
    }

    private void EvaluateLowHealth(int currentHealth)
    {
        bool shouldBeLow = currentHealth > 0 && currentHealth <= lowHealthThreshold;
        if (shouldBeLow == isLowHealthActive) return;

        isLowHealthActive = shouldBeLow;
        if (shouldBeLow)
            StartLowHealthPulse();
        else
        {
            StopLowHealthPulse();
            SetLowHealthVisuals(false, 0f);
        }
    }

    private void StartLowHealthPulse()
    {
        if (lowHealthPulseRoutine != null) return;
        lowHealthPulseRoutine = StartCoroutine(LowHealthPulseRoutine());
    }

    private void StopLowHealthPulse()
    {
        if (lowHealthPulseRoutine == null) return;
        StopCoroutine(lowHealthPulseRoutine);
        lowHealthPulseRoutine = null;
    }

    private IEnumerator LowHealthPulseRoutine()
    {
        while (true)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
            SetLowHealthVisuals(true, pulse);
            yield return null;
        }
    }

    private void SetLowHealthVisuals(bool enabled, float pulse)
    {
        if (lowHealthWarningImage != null)
        {
            lowHealthWarningImage.gameObject.SetActive(enabled);
            Color warningColor = lowHealthWarningImage.color;
            warningColor.a = enabled ? Mathf.Lerp(0.35f, 1f, pulse) : 0f;
            lowHealthWarningImage.color = warningColor;
        }

        if (healthText != null)
            healthText.color = enabled ? Color.Lerp(Color.white, lowHealthFlashColor, pulse) : Color.white;
    }
}
