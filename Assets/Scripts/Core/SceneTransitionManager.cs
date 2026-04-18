using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Persistent singleton. Handles fades and scene loading between overworld and shmup.
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string defaultOverworldSceneName = "Sector001_map";

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    public LevelDefinition CurrentLevel { get; private set; }

    private Image fadeImage;
    private bool isTransitioning;

    // Which save slot is active — needed to reload on loss
    public int ActiveSaveSlot { get; set; } = 0;

    // Set by RoomTransition before loading a hub — so we know which sector to return to
    public string ReturnScene { get; set; }

    // Set by hub exit RoomTransition — tells OverworldSceneBootstrap which spawn point to use
    public string RequestedSpawnID { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    // --- Public API ---

    public void LoadShmupLevel(LevelDefinition level)
    {
        if (isTransitioning) return;
        CurrentLevel = level;
        ReturnScene = SceneManager.GetActiveScene().name;
        GameStateManager.Instance.SetLevelToLoad(level.levelID);
        StartCoroutine(Transition(level.sceneName));
    }

    public void LoadArenaScene(string arenaSceneName)
    {
        if (isTransitioning) return;
        ReturnScene = SceneManager.GetActiveScene().name;
        GameStateManager.Instance?.Save(ActiveSaveSlot);
        StartCoroutine(Transition(arenaSceneName));
    }

    public void ReturnToOverworld(bool won)
    {
        if (isTransitioning) return;

        if (GameStateManager.Instance != null)
        {
            if (won)
            {
                GameStateManager.Instance.CompleteLevel(CurrentLevel.levelID);
                GameStateManager.Instance.Save(ActiveSaveSlot);
            }
            else
            {
                // Roll back to last save — discards any unsaved progress
                GameStateManager.Instance.Load(ActiveSaveSlot);
            }
        }

        string scene = !string.IsNullOrEmpty(ReturnScene) ? ReturnScene : defaultOverworldSceneName;
        StartCoroutine(Transition(scene));
    }

    // --- Internal ---

    IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;

        CapturePlayerHealth();

        yield return StartCoroutine(Fade(1f));             // fade to black
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return StartCoroutine(Fade(0f));             // fade in

        isTransitioning = false;
    }

    private void CapturePlayerHealth()
    {
        var player = GameObject.FindWithTag("Player");
        var health = player?.GetComponentInChildren<Health>();
        if (health != null && GameStateManager.Instance != null)
            GameStateManager.Instance.Current.playerHealth = health.Current;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    void CreateFadeCanvas()
    {
        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();

        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        var rect = imageGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;
    }
}
