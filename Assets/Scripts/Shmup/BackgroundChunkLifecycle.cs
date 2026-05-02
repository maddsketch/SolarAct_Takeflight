using UnityEngine;

// Attach to background chunks spawned under a moving parent (e.g. BackgroundEventPlayer.spawnParent).
// Destroys this GameObject when its position crosses a threshold along an axis (local space of scrollRoot by default).
public class BackgroundChunkLifecycle : MonoBehaviour
{
    public enum LifecycleAxis
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    [SerializeField] private Transform scrollRoot;
    [SerializeField] private LifecycleAxis axis = LifecycleAxis.Z;
    [Tooltip("If true, destroy when coordinate is less than threshold; if false, when greater than threshold.")]
    [SerializeField] private bool destroyWhenLessThan = true;
    [SerializeField] private float destroyLocalThreshold;

    [Header("Optional world space")]
    [SerializeField] private bool useWorldSpace;
    [SerializeField] private float destroyWorldThreshold;

    private Transform resolvedRoot;
    private bool warnedMissingRoot;

    void Awake()
    {
        resolvedRoot = scrollRoot != null ? scrollRoot : transform.parent;
        if (resolvedRoot == null && !warnedMissingRoot)
        {
            warnedMissingRoot = true;
            Debug.LogWarning($"[BackgroundChunkLifecycle] '{name}' has no scrollRoot and no parent — disabling.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (resolvedRoot == null) return;

        float coord;
        float threshold;
        bool lessThan;

        if (useWorldSpace)
        {
            coord = GetAxisComponent(transform.position, axis);
            threshold = destroyWorldThreshold;
            lessThan = destroyWhenLessThan;
        }
        else
        {
            Vector3 local = resolvedRoot.InverseTransformPoint(transform.position);
            coord = GetAxisComponent(local, axis);
            threshold = destroyLocalThreshold;
            lessThan = destroyWhenLessThan;
        }

        bool past = lessThan ? coord < threshold : coord > threshold;
        if (past)
            Destroy(gameObject);
    }

    private static float GetAxisComponent(Vector3 v, LifecycleAxis a) =>
        a switch
        {
            LifecycleAxis.X => v.x,
            LifecycleAxis.Y => v.y,
            _ => v.z
        };
}
