using UnityEngine;
using UnityEngine.UI;

// Attach to any static dot on the minimap (zone, hub, point of interest).
// Positions itself from a world Transform reference; updates each frame when the minimap is player-centered.
public class MinimapDot : MonoBehaviour
{
    [SerializeField] private Transform worldTarget;  // the world object this dot represents

    private RectTransform rectTransform;
    private Image dotImage;
    private Color baseImageColor;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        dotImage = GetComponent<Image>();
        if (dotImage != null) baseImageColor = dotImage.color;
        PlaceDot();
    }

    void LateUpdate()
    {
        var c = MinimapController.Instance;
        if (c != null && c.UsesPlayerCenter)
            PlaceDot();
    }

    private void PlaceDot()
    {
        if (worldTarget == null || MinimapController.Instance == null) return;

        var c = MinimapController.Instance;
        Vector2 raw = c.WorldToMinimapPosition(worldTarget.position);

        if (dotImage != null && c.ClampAndFadePoiDots)
        {
            float R = c.EffectiveRadarRadiusPixels;
            float band = Mathf.Max(0.01f, c.EdgeFadeBandPixels);
            float d = raw.magnitude;
            Vector2 shown = d > 1e-4f ? raw * (Mathf.Min(d, R) / d) : raw;

            float a = 1f;
            if (d <= R - band)
                a = 1f;
            else if (d >= R)
                a = c.OffRadarMinAlpha;
            else
                a = Mathf.Lerp(1f, c.OffRadarMinAlpha, (d - (R - band)) / band);

            Color col = baseImageColor;
            col.a = baseImageColor.a * a;
            dotImage.color = col;
            rectTransform.anchoredPosition = shown;
            return;
        }

        rectTransform.anchoredPosition = raw;
        if (dotImage != null) dotImage.color = baseImageColor;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (dotImage == null) dotImage = GetComponent<Image>();
        if (dotImage != null) baseImageColor = dotImage.color;
        if (MinimapController.Instance != null && MinimapController.Instance.UsesPlayerCenter && !Application.isPlaying)
            return;
        PlaceDot();
    }
#endif
}
