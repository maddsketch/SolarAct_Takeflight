using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private string targetTag = "Enemy";
    [SerializeField, Min(0f)] private float impactMagnitude = 1f;
    // +Z for player bullets (moving up screen), -Z for enemy bullets (moving down)
    [SerializeField] private Vector3 direction = Vector3.forward;
    [Header("On-Hit Effects")]
    [SerializeField] private bool appliesSlow;
    [SerializeField, Range(0f, 0.95f)] private float slowPercent;
    [SerializeField, Min(0f)] private float slowDuration;
    [SerializeField] private string slowSourceId = "Bullet";

    public void SetTargetTag(string tag) => targetTag = tag;
    public void SetDirection(Vector3 dir) => direction = dir.normalized;
    public void ConfigureEffects(bool enableSlow, float slowAmount, float duration, string sourceId)
    {
        appliesSlow = enableSlow;
        slowPercent = Mathf.Clamp01(slowAmount);
        slowDuration = Mathf.Max(0f, duration);
        if (!string.IsNullOrEmpty(sourceId))
            slowSourceId = sourceId;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        // Enemy bullets (target Player) should pass through other enemies.
        if (targetTag == "Player" && other.CompareTag("Enemy"))
            return;

        if (other.CompareTag(targetTag))
        {
            other.GetComponent<Health>()?.TakeDamage(damage, impactMagnitude);
            if (appliesSlow)
                other.GetComponent<MovementStatusEffects>()?.ApplySlow(slowPercent, slowDuration, slowSourceId);
        }

        Destroy(gameObject);
    }
}
