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

    private static readonly HashSet<Transform> registeredTargets = new();
    private readonly Dictionary<Transform, ArenaDot> dots = new();
    private ArenaDot playerDot;

    // --- Static registration API called by ArenaEnemyAI ---

    public static void Register(ArenaEnemyAI enemy)
    {
        if (enemy == null) return;
        RegisterTarget(enemy.transform);
    }

    public static void RegisterTarget(Transform target)
    {
        if (target == null) return;
        registeredTargets.Add(target);
        _instance?.AddDot(target);
    }

    public static void Unregister(ArenaEnemyAI enemy)
    {
        if (enemy == null) return;
        UnregisterTarget(enemy.transform);
    }

    public static void UnregisterTarget(Transform target)
    {
        if (target == null) return;
        registeredTargets.Remove(target);

        if (_instance != null && _instance.dots.TryGetValue(target, out var dot))
        {
            if (dot != null) Destroy(dot.gameObject);
            _instance.dots.Remove(target);
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
        foreach (var target in registeredTargets)
            AddDot(target);
    }

    void OnDestroy()
    {
        if (_instance == this) _instance = null;
        registeredTargets.Clear();
    }

    void LateUpdate()
    {
        if (dots.Count == 0) return;

        // Clean up stale targets in case objects are destroyed before explicit unregister.
        var staleTargets = ListPool<Transform>.Get();
        foreach (var pair in dots)
        {
            if (pair.Key != null) continue;

            if (pair.Value != null)
                Destroy(pair.Value.gameObject);

            staleTargets.Add(pair.Key);
        }

        for (int i = 0; i < staleTargets.Count; i++)
            dots.Remove(staleTargets[i]);

        ListPool<Transform>.Release(staleTargets);
    }

    private void AddDot(Transform target)
    {
        if (dotPrefab == null || playerTransform == null) return;
        if (target == null || dots.ContainsKey(target)) return;

        var go  = Instantiate(dotPrefab, dotContainer);
        var dot = go.GetComponent<ArenaDot>();
        dot?.Init(target, playerTransform, arenaRadius, minimapRadius, enemyDotColor);
        dots[target] = dot;
    }
}

internal static class ListPool<T>
{
    private static readonly Stack<List<T>> Pool = new();

    public static List<T> Get() => Pool.Count > 0 ? Pool.Pop() : new List<T>();

    public static void Release(List<T> list)
    {
        list.Clear();
        Pool.Push(list);
    }
}
