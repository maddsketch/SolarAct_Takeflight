using UnityEngine;

// Added to enemy prefabs. Call Init() at spawn time from WaveManager.
// If no PathData is provided the enemy falls straight down.
public class EnemyMover : MonoBehaviour
{
    [SerializeField] private float despawnZ = -10f;

    [Header("Path rotation")]
    [SerializeField] private bool alignRotationToPath = true;
    [SerializeField] private float maxBankAngle = 25f;
    [SerializeField] private float bankGain = 80f;
    [SerializeField] private float rotationSmooth = 12f;
    [SerializeField] private float bankSmooth = 10f;

    private Vector3[] worldWaypoints;
    private float speed = 3f;
    private int currentIndex;
    private Vector3 lastDirection = Vector3.back;
    private bool pathComplete;

    private Vector3 prevHorizForward;
    private bool hasPrevHorizForward;
    private float currentBank;

    public void Init(PathData data, Vector3 spawnPos)
    {
        speed = data.moveSpeed;
        worldWaypoints = new Vector3[data.waypoints.Length];
        for (int i = 0; i < data.waypoints.Length; i++)
            worldWaypoints[i] = spawnPos + data.waypoints[i];
        hasPrevHorizForward = false;
        currentBank = 0f;
    }

    void Update()
    {
        if (pathComplete || worldWaypoints == null || worldWaypoints.Length == 0)
        {
            transform.Translate(lastDirection * speed * Time.deltaTime, Space.World);
            ApplyPathRotation(lastDirection);
            if (transform.position.z < despawnZ)
                Destroy(gameObject);
            return;
        }

        Vector3 target = worldWaypoints[currentIndex];
        lastDirection = (target - transform.position).normalized;

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target;
            currentIndex++;
            if (currentIndex >= worldWaypoints.Length)
                pathComplete = true;
        }

        ApplyPathRotation(lastDirection);
    }

    private void ApplyPathRotation(Vector3 desiredForward)
    {
        if (!alignRotationToPath)
            return;

        if (desiredForward.sqrMagnitude < 1e-8f)
            return;

        Vector3 fwd = desiredForward.normalized;
        if (Mathf.Abs(Vector3.Dot(fwd, Vector3.up)) > 0.98f)
        {
            Vector3 fallback = Vector3.ProjectOnPlane(fwd, Vector3.up);
            if (fallback.sqrMagnitude < 1e-8f)
                fallback = transform.forward;
            fwd = fallback.normalized;
        }

        Quaternion look = Quaternion.LookRotation(fwd, Vector3.up);

        Vector3 currHoriz = new Vector3(desiredForward.x, 0f, desiredForward.z);
        if (currHoriz.sqrMagnitude >= 1e-8f)
            currHoriz.Normalize();
        else
            currHoriz = Vector3.zero;

        float targetBank = 0f;
        if (currHoriz.sqrMagnitude >= 1e-8f && hasPrevHorizForward &&
            prevHorizForward.sqrMagnitude >= 1e-8f)
        {
            float turn = Vector3.Dot(Vector3.Cross(prevHorizForward, currHoriz), Vector3.up);
            targetBank = Mathf.Clamp(-turn * bankGain, -maxBankAngle, maxBankAngle);
        }

        if (currHoriz.sqrMagnitude >= 1e-8f)
        {
            prevHorizForward = currHoriz;
            hasPrevHorizForward = true;
        }

        float bankLerp = 1f - Mathf.Exp(-bankSmooth * Time.deltaTime);
        currentBank = Mathf.Lerp(currentBank, targetBank, bankLerp);

        Quaternion targetRot = look * Quaternion.Euler(0f, 0f, currentBank);
        float rotLerp = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotLerp);
    }
}
