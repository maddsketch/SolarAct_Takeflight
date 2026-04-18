using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a Canvas set to DontDestroyOnLoad.
// Shows a toast notification when an achievement is unlocked.
public class AchievementPopupUI : MonoBehaviour
{
    public static AchievementPopupUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Image             iconImage;
    [SerializeField] private TextMeshProUGUI   titleText;
    [SerializeField] private TextMeshProUGUI   nameText;
    [SerializeField] private float             displayDuration = 3f;
    [SerializeField] private float             fadeDuration    = 0.4f;

    private Queue<AchievementDefinition> queue = new();
    private bool isShowing;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = panel.GetComponent<CanvasGroup>();
        panel.SetActive(false);
    }

    void Start()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.onAchievementUnlocked += Enqueue;
    }

    void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.onAchievementUnlocked -= Enqueue;
    }

    private void Enqueue(AchievementDefinition achievement)
    {
        queue.Enqueue(achievement);
        if (!isShowing)
        {
            isShowing = true;
            StartCoroutine(ShowNext());
        }
    }

    private IEnumerator ShowNext()
    {
        while (queue.Count > 0)
        {
            var achievement = queue.Dequeue();

            if (iconImage != null) iconImage.sprite  = achievement.icon;
            if (titleText != null) titleText.text    = "Achievement Unlocked";
            if (nameText  != null) nameText.text     = achievement.displayName;

            panel.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f));
            yield return new WaitForSecondsRealtime(displayDuration);
            yield return StartCoroutine(Fade(1f, 0f));

            panel.SetActive(false);
        }

        isShowing = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
