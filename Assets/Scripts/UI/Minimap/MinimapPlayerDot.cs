using UnityEngine;

// Attach to the PlayerDot Image inside the minimap.
// Tracks the player's world position and rotates to match facing direction.
public class MinimapPlayerDot : MonoBehaviour
{
    private Transform player;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    void Update()
    {
        if (player == null || MinimapController.Instance == null) return;

        rectTransform.anchoredPosition = MinimapController.Instance.UsesPlayerCenter
            ? Vector2.zero
            : MinimapController.Instance.WorldToMinimapPosition(player.position);

        // Rotate arrow to match player's facing direction (Y rotation → minimap Z rotation)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
    }
}
