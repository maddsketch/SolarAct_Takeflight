using UnityEngine;

// Attach to any object for a gentle floating bob and rocking motion.
public class IdleHover : MonoBehaviour
{
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rockAngle = 5f;
    [SerializeField] private float rockSpeed = 1.5f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float t = Time.time;

        float zOffset = Mathf.Sin(t * bobSpeed) * bobAmplitude;
        transform.localPosition = startPos + new Vector3(0f, 0f, zOffset);

        float zRock = Mathf.Sin(t * rockSpeed) * rockAngle;
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, zRock);
    }
}
