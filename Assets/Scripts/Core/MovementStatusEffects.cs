using UnityEngine;

public class MovementStatusEffects : MonoBehaviour
{
    [SerializeField, Range(0f, 0.95f)] private float activeSlowPercent;
    [SerializeField] private float slowEndTime;
    [SerializeField] private string slowSourceId;

    public void ApplySlow(float slowPercent, float duration, string sourceId)
    {
        if (duration <= 0f)
            return;

        float clampedSlow = Mathf.Clamp01(slowPercent);
        if (clampedSlow <= 0f)
            return;

        float requestedEndTime = Time.time + duration;
        bool hasActiveSlow = Time.time < slowEndTime && activeSlowPercent > 0f;

        if (!hasActiveSlow || clampedSlow > activeSlowPercent)
        {
            activeSlowPercent = clampedSlow;
            slowEndTime = requestedEndTime;
            slowSourceId = sourceId;
            return;
        }

        if (Mathf.Approximately(clampedSlow, activeSlowPercent) || slowSourceId == sourceId)
            slowEndTime = Mathf.Max(slowEndTime, requestedEndTime);
    }

    public float GetSpeedMultiplier()
    {
        if (Time.time >= slowEndTime || activeSlowPercent <= 0f)
            return 1f;

        return 1f - activeSlowPercent;
    }

    void Update()
    {
        if (Time.time < slowEndTime || activeSlowPercent <= 0f)
            return;

        activeSlowPercent = 0f;
        slowEndTime = 0f;
        slowSourceId = string.Empty;
    }
}
