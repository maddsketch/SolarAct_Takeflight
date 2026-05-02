using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Lives on the Shop UI canvas (e.g. Station scene). Queued fade toast when the player buys an item.
public class ShopPurchasePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeDuration = 0.4f;

    private readonly Queue<ItemDefinition> queue = new();
    private bool isShowing;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (panel == null)
            BuildDefaultUi();

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        panel.SetActive(false);
        transform.SetAsLastSibling();
    }

    public void Enqueue(ItemDefinition item)
    {
        if (item == null) return;

        queue.Enqueue(item);
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
            var item = queue.Dequeue();

            if (iconImage != null) iconImage.sprite = item.icon;
            if (titleText != null) titleText.text = "Purchased";
            if (nameText != null)  nameText.text  = item.displayName;

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

    private void BuildDefaultUi()
    {
        var panelGo = new GameObject("PurchaseToastPanel", typeof(RectTransform));
        panelGo.transform.SetParent(transform, false);
        var panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 1f);
        panelRt.anchorMax = new Vector2(0.5f, 1f);
        panelRt.pivot = new Vector2(0.5f, 1f);
        panelRt.anchoredPosition = new Vector2(0f, -100f);
        panelRt.sizeDelta = new Vector2(668f, 137f);

        var bg = panelGo.AddComponent<Image>();
        bg.sprite = null;
        bg.color = new Color(0.08f, 0.08f, 0.1f, 0.88f);
        bg.raycastTarget = false;
        panelGo.AddComponent<CanvasGroup>();
        panel = panelGo;

        iconImage = CreateIconImage(panelRt);
        titleText = CreateTmpText("Title", "Purchased", new Vector2(53f, 25f), new Vector2(438f, 50f), 36);
        nameText  = CreateTmpText("Name", "", new Vector2(53f, -33f), new Vector2(455f, 53f), 36);
        titleText.transform.SetParent(panelRt, false);
        nameText.transform.SetParent(panelRt, false);
    }

    private Image CreateIconImage(RectTransform parentRt)
    {
        var go = new GameObject("Icon", typeof(RectTransform));
        go.transform.SetParent(parentRt, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-254f, 0f);
        rt.sizeDelta = new Vector2(100f, 100f);
        var img = go.AddComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    private TextMeshProUGUI CreateTmpText(string objectName, string text, Vector2 anchoredPosition, Vector2 size, float fontSize)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
            tmp.material = TMP_Settings.defaultFontAsset.material;
        }
        return tmp;
    }
}
