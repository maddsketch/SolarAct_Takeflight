using UnityEngine;

[CreateAssetMenu(fileName = "AmbientPlane_", menuName = "TakeFlight/Ambient Plane Definition")]
public class AmbientPlaneDefinition : ScriptableObject
{
    public GameObject prefab;

    [Tooltip("Random altitude range for this plane type.")]
    public Vector2 altitudeRange = new Vector2(2f, 8f);

    [Tooltip("Higher weight = spawns more often relative to other types.")]
    [Min(0.1f)] public float weight = 1f;
}
