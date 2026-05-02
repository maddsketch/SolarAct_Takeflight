using UnityEngine;
using UnityEngine.UI;

// Used by MinimapUI in the Arena scene.
// Tracks an enemy transform relative to the player within the circular arena bounds.
public class ArenaDot : MonoBehaviour
{
    private Transform target;
    private Transform player;
    private float arenaRadius;
    private float minimapRadius;
    private RectTransform rectTransform;

    public void Init(Transform target, Transform player, float arenaRadius, float minimapRadius, Color color)
    {
        this.target       = target;
        this.player       = player;
        this.arenaRadius  = arenaRadius;
        this.minimapRadius = minimapRadius;

        rectTransform = GetComponent<RectTransform>();

        var img = GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void Update()
    {
        if (target == null || player == null || rectTransform == null) return;

        Vector3 offset = target.position - player.position;
        float nx = offset.x / arenaRadius;
        float ny = offset.z / arenaRadius;
        Vector2 normalized = new(nx, ny);

        // Keep direction but clamp any out-of-range enemies to radar edge.
        if (normalized.sqrMagnitude > 1f)
            normalized = normalized.normalized;

        rectTransform.anchoredPosition = normalized * minimapRadius;
    }
}
