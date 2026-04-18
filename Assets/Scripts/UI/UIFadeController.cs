using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeController : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private AnimationCurve fadeCurve = null;
    [SerializeField] private bool disableOnHidden = true;
    [SerializeField] private bool startHidden = false;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private bool isVisible;

    public bool IsVisible => isVisible;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (fadeCurve == null || fadeCurve.length == 0)
        {
            fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    private void Start()
    {
        SetVisibleImmediate(!startHidden);
    }

    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Show()
    {
        StartFade(true, fadeInDuration);
    }

    public void Hide()
    {
        StartFade(false, fadeOutDuration);
    }

    public void SetVisibleImmediate(bool visible)
    {
        StopRunningFade();
        if (visible)
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isVisible = true;
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isVisible = false;
            if (disableOnHidden)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void StartFade(bool show, float duration)
    {
        StopRunningFade();
        fadeRoutine = StartCoroutine(FadeRoutine(show, duration));
    }

    private IEnumerator FadeRoutine(bool show, float duration)
    {
        if (show)
        {
            gameObject.SetActive(true);
        }

        float startAlpha = canvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = fadeCurve.Evaluate(t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        isVisible = show;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        if (!show && disableOnHidden)
        {
            gameObject.SetActive(false);
        }

        fadeRoutine = null;
    }

    private void StopRunningFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }
}
