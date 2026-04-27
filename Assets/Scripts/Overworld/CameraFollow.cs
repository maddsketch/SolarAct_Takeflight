using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum CameraLookAheadSource
{
    [Tooltip("Shift camera based on mouse position vs screen center (Input System).")]
    MouseScreenSpace,
    [Tooltip("Shift camera in the direction the target is facing (XZ). Good for overworld.")]
    FacingDirection,
    [Tooltip("Mix mouse and facing; use Facing Blend for the ratio.")]
    BlendMouseAndFacing
}

// Attach to the Main Camera in the overworld or arena scene.
// Follows the player on XZ with optional look-ahead (mouse, facing, or blend).
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height      = 15f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Look Ahead")]
    [SerializeField] private CameraLookAheadSource lookAheadSource = CameraLookAheadSource.MouseScreenSpace;
    [Tooltip("0 = all mouse offset, 1 = all facing offset. Only used when Look Ahead Source is Blend.")]
    [SerializeField, Range(0f, 1f)] private float facingBlend = 0.5f;
    [FormerlySerializedAs("lookAheadDistance")]
    [SerializeField] private float maxLookAheadDistance = 3f; // max camera shift from target
    [SerializeField] private float lookAheadSpeed       = 4f; // how quickly offset tracks target look-ahead
    [SerializeField] private float deadZone             = 0.08f; // normalized center zone (mouse mode only)

    private Vector3 currentLookAhead;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 mouseLookAhead = ComputeMouseScreenSpaceLookAhead();
        Vector3 facingLookAhead = ComputeFacingLookAhead();

        Vector3 targetLookAhead = lookAheadSource switch
        {
            CameraLookAheadSource.MouseScreenSpace => mouseLookAhead,
            CameraLookAheadSource.FacingDirection => facingLookAhead,
            CameraLookAheadSource.BlendMouseAndFacing => Vector3.Lerp(mouseLookAhead, facingLookAhead, facingBlend),
            _ => mouseLookAhead
        };

        targetLookAhead = Vector3.ClampMagnitude(targetLookAhead, maxLookAheadDistance);

        currentLookAhead = Vector3.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        Vector3 desired = new Vector3(
            target.position.x + currentLookAhead.x,
            height,
            target.position.z + currentLookAhead.z);

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }

    private Vector3 ComputeMouseScreenSpaceLookAhead()
    {
        Vector2 half = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 mouse = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : half;

        Vector2 normalizedOffset = Vector2.zero;
        if (half.x > 0.001f && half.y > 0.001f)
        {
            normalizedOffset.x = Mathf.Clamp((mouse.x - half.x) / half.x, -1f, 1f);
            normalizedOffset.y = Mathf.Clamp((mouse.y - half.y) / half.y, -1f, 1f);
        }

        float mag = normalizedOffset.magnitude;
        if (mag < deadZone)
            normalizedOffset = Vector2.zero;
        else if (mag > 1e-5f)
        {
            float remapped = Mathf.Clamp01((mag - deadZone) / (1f - deadZone));
            normalizedOffset = normalizedOffset.normalized * remapped;
        }

        return new Vector3(
            normalizedOffset.x * maxLookAheadDistance,
            0f,
            normalizedOffset.y * maxLookAheadDistance);
    }

    private Vector3 ComputeFacingLookAhead()
    {
        Vector3 fwd = target.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f)
            return Vector3.zero;

        fwd.Normalize();
        return fwd * maxLookAheadDistance;
    }
}
