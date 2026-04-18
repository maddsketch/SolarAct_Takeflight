using UnityEngine;

// Place on each ambient plane prefab.
// AmbientPlaneSpawner calls Init() after instantiating.
public class AmbientPlaneMover : MonoBehaviour
{
    [Tooltip("Base travel direction. Will be normalized at runtime.")]
    public Vector3 direction = Vector3.right;

    [Tooltip("Speed is randomized between these values on spawn.")]
    public Vector2 speedRange = new Vector2(3f, 6f);

    [Tooltip("50% chance to flip direction on the X axis when spawned.")]
    public bool randomizeDirectionFlip = true;

    private float speed;
    private Bounds loopBounds;

    // Called by AmbientPlaneSpawner after Instantiate.
    public void Init(Bounds bounds)
    {
        loopBounds = bounds;
        speed = Random.Range(speedRange.x, speedRange.y);

        if (randomizeDirectionFlip && Random.value < 0.5f)
        {
            direction.x *= -1f;
            direction.z *= -1f;
        }

        direction.Normalize();

        // Face the travel direction (Y-up assumed).
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        WrapIfOutOfBounds();
    }

    private void WrapIfOutOfBounds()
    {
        Vector3 pos = transform.position;

        if (pos.x > loopBounds.max.x) pos.x = loopBounds.min.x;
        else if (pos.x < loopBounds.min.x) pos.x = loopBounds.max.x;

        if (pos.z > loopBounds.max.z) pos.z = loopBounds.min.z;
        else if (pos.z < loopBounds.min.z) pos.z = loopBounds.max.z;

        transform.position = pos;
    }
}
