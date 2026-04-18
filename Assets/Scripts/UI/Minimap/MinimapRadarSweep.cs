using UnityEngine;

// Rotating sonar wedge on a UI Image (Filled / Radial 360). Place as last child under a masked map area.
[RequireComponent(typeof(RectTransform))]
public class MinimapRadarSweep : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 48f;

    void Update()
    {
        float z = -Time.unscaledTime * degreesPerSecond;
        transform.localRotation = Quaternion.Euler(0f, 0f, z);
    }
}
