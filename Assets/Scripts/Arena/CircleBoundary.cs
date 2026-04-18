using UnityEngine;

// Place one in the arena scene. Defines the play area radius.
// Used by ArenaPlayerController to clamp position and ArenaWaveSpawner to place enemies.
public class CircleBoundary : MonoBehaviour
{
    public static CircleBoundary Instance { get; private set; }

    [SerializeField] private float radius = 15f;
    public float Radius => radius;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Clamps a world position to inside the circle (Y is preserved).
    public Vector3 Clamp(Vector3 position)
    {
        Vector3 flat = new Vector3(position.x - transform.position.x, 0f, position.z - transform.position.z);
        if (flat.magnitude > radius)
            flat = flat.normalized * radius;
        return new Vector3(transform.position.x + flat.x, position.y, transform.position.z + flat.z);
    }

    // Returns a random world position on the perimeter.
    public Vector3 RandomPerimeterPoint()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.3f);
        DrawCircle(transform.position, radius, 64);
    }

    private void DrawCircle(Vector3 center, float r, int segments)
    {
        float step = Mathf.PI * 2f / segments;
        Vector3 prev = center + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}
