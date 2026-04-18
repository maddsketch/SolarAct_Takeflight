using UnityEngine;

// Attach to MinimapContainer.
// Defines the world bounds the minimap represents and converts world positions to minimap positions.
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("Follow")]
    [SerializeField] private bool centerOnPlayer = true;
    [SerializeField] private Transform playerOverride;

    [Header("World Bounds")]
    [SerializeField] private Vector2 mapWorldCenter = Vector2.zero;  // X/Z center when not centering on player
    [SerializeField] private float mapWorldSize = 200f;              // total width/height of world area

    [Header("Minimap Size")]
    [SerializeField] private float minimapPixelSize = 120f;          // match RectTransform width/height

    [Header("POI edge")]
    [SerializeField] private bool clampAndFadePoiDots = true;
    [Tooltip("0 = use half of minimap pixel size (inscribed circle).")]
    [SerializeField] private float radarRadiusPixels;
    [SerializeField] private float edgeFadeBandPixels = 28f;
    [SerializeField] private float offRadarMinAlpha = 0.35f;

    private Transform resolvedPlayer;

    public bool UsesPlayerCenter => centerOnPlayer;

    public bool ClampAndFadePoiDots => clampAndFadePoiDots;

    public float EffectiveRadarRadiusPixels =>
        radarRadiusPixels > 0.001f ? radarRadiusPixels : minimapPixelSize * 0.5f;

    public float EdgeFadeBandPixels => edgeFadeBandPixels;

    public float OffRadarMinAlpha => offRadarMinAlpha;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    Transform EffectivePlayerTransform
    {
        get
        {
            if (!centerOnPlayer) return null;
            if (playerOverride != null) return playerOverride;
            if (resolvedPlayer == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) resolvedPlayer = go.transform;
            }
            return resolvedPlayer;
        }
    }

    // Converts a world position (X/Z) to a local minimap position (pixels from center).
    public Vector2 WorldToMinimapPosition(Vector3 worldPos)
    {
        Vector2 origin = mapWorldCenter;
        var p = EffectivePlayerTransform;
        if (p != null)
            origin = new Vector2(p.position.x, p.position.z);

        float x = (worldPos.x - origin.x) / mapWorldSize * minimapPixelSize;
        float y = (worldPos.z - origin.y) / mapWorldSize * minimapPixelSize;
        return new Vector2(x, y);
    }
}
