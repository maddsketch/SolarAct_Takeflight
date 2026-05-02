using UnityEngine;

// Arena bullet — direction and target are set at spawn time via Init().
// Works for both player bullets (targetTag = "Enemy")
// and enemy bullets (targetTag = "Player").
public class ArenaBullet : MonoBehaviour
{
    [SerializeField] private float speed  = 18f;
    [SerializeField] private int   damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField, Min(0f)] private float impactMagnitude = 1f;
    [Header("On-Hit Effects")]
    [SerializeField] private bool appliesSlow;
    [SerializeField, Range(0f, 0.95f)] private float slowPercent;
    [SerializeField, Min(0f)] private float slowDuration;
    [SerializeField] private string slowSourceId = "ArenaBullet";

    private Vector3 direction;
    private string  targetTag = "Enemy";

    // Called immediately after Instantiate by whoever fires this bullet.
    public void Init(Vector3 worldDirection, string target, int dmg = -1)
    {
        direction = worldDirection.normalized;
        targetTag = target;
        if (dmg >= 0) damage = dmg;

        // Rotate to face travel direction (optional — makes sprite/mesh align)
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Destroy(gameObject, lifetime);
    }

    public void ConfigureEffects(bool enableSlow, float slowAmount, float duration, string sourceId)
    {
        appliesSlow = enableSlow;
        slowPercent = Mathf.Clamp01(slowAmount);
        slowDuration = Mathf.Max(0f, duration);
        if (!string.IsNullOrEmpty(sourceId))
            slowSourceId = sourceId;
    }

    void Update()
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("[ArenaBullet] direction is zero — Init() was not called. Using transform.forward as fallback.");
            direction = transform.forward;
        }

        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        other.GetComponent<Health>()?.TakeDamage(damage, impactMagnitude);
        if (appliesSlow)
            other.GetComponent<MovementStatusEffects>()?.ApplySlow(slowPercent, slowDuration, slowSourceId);
        Destroy(gameObject);
    }
}
