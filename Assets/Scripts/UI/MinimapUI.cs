using System.Collections.Generic;
using UnityEngine;

// Attach to the minimap panel in the arena scene HUD.
// ArenaEnemyAI calls Register/Unregister on spawn/death.
// Draws one dot per enemy, player dot stays centered.
public class MinimapUI : MonoBehaviour
{
    private static MinimapUI _instance;

    [SerializeField] private RectTransform dotContainer; // child of minimap panel
    [SerializeField] private GameObject dotPrefab;       // prefab with MinimapDot + Image
    [SerializeField] private float minimapRadius = 60f;  // pixels — match your panel's visual radius
    [SerializeField] private Color enemyDotColor  = Color.red;
    [SerializeField] private Color playerDotColor = Color.cyan;

    private Transform playerTransform;
    private float arenaRadius;

    private static readonly List<ArenaEnemyAI> registeredEnemies = new();
    private readonly Dictionary<ArenaEnemyAI, ArenaDot> dots = new();
    private ArenaDot playerDot;

    // --- Static registration API called by ArenaEnemyAI ---

    public static void Register(ArenaEnemyAI enemy)
    {
        if (!registeredEnemies.Contains(enemy))
            registeredEnemies.Add(enemy);

        _instance?.AddDot(enemy);
    }

    public static void Unregister(ArenaEnemyAI enemy)
    {
        registeredEnemies.Remove(enemy);

        if (_instance != null && _instance.dots.TryGetValue(enemy, out var dot))
        {
            if (dot != null) Destroy(dot.gameObject);
            _instance.dots.Remove(enemy);
        }

    }

    // --- Lifecycle ---

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        arenaRadius = CircleBoundary.Instance != null ? CircleBoundary.Instance.Radius : 15f;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // Player dot — always centered, just a visual marker
        if (dotPrefab != null && playerTransform != null)
        {
            var go  = Instantiate(dotPrefab, dotContainer);
            playerDot = go.GetComponent<ArenaDot>();
            playerDot?.Init(playerTransform, playerTransform, arenaRadius, 0f, playerDotColor);
        }

        // Catch any enemies that registered before Start ran
        foreach (var enemy in registeredEnemies)
            AddDot(enemy);
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
        registeredEnemies.Clear();
    }

    private void AddDot(ArenaEnemyAI enemy)
    {
        if (dotPrefab == null || playerTransform == null) return;
        if (dots.ContainsKey(enemy)) return;

        var go  = Instantiate(dotPrefab, dotContainer);
        var dot = go.GetComponent<ArenaDot>();
        dot?.Init(enemy.transform, playerTransform, arenaRadius, minimapRadius, enemyDotColor);
        dots[enemy] = dot;
    }
}
