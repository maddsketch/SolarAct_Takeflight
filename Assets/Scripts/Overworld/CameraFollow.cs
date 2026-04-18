using UnityEngine;

// Attach to the Main Camera in the overworld or arena scene.
// Follows the player on XZ with a look-ahead offset in the direction of movement.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height      = 15f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 3f;  // how far ahead the camera shifts
    [SerializeField] private float lookAheadSpeed     = 4f; // how quickly the offset tracks movement

    private Vector3 currentLookAhead;
    private Vector3 lastTargetPos;

    void Start()
    {
        if (target != null) lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Derive movement direction from how far the target moved this frame
        Vector3 delta = target.position - lastTargetPos;
        lastTargetPos = target.position;

        Vector3 targetLookAhead = Vector3.zero;
        if (delta.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = new Vector3(delta.x, 0f, delta.z).normalized;
            targetLookAhead = moveDir * lookAheadDistance;
        }

        // Smoothly blend the look-ahead offset
        currentLookAhead = Vector3.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        Vector3 desired = new Vector3(
            target.position.x + currentLookAhead.x,
            height,
            target.position.z + currentLookAhead.z);

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
