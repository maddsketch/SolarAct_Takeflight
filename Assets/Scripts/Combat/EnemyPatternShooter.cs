using System.Collections;
using UnityEngine;

public class EnemyPatternShooter : MonoBehaviour
{
    public void FirePattern(EnemyWeaponDefinition weapon, Transform[] muzzles, Vector3 baseDirection, string targetTagOverride = null)
    {
        if (weapon == null || weapon.shotSlots == null || weapon.shotSlots.Length == 0)
            return;

        Vector3 direction = baseDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.forward;
        direction.Normalize();

        StartCoroutine(FirePatternRoutine(weapon, muzzles, direction, targetTagOverride));
    }

    private IEnumerator FirePatternRoutine(EnemyWeaponDefinition weapon, Transform[] muzzles, Vector3 baseDirection, string targetTagOverride)
    {
        int bursts = Mathf.Max(1, weapon.burstCount);
        for (int burstIndex = 0; burstIndex < bursts; burstIndex++)
        {
            for (int i = 0; i < weapon.shotSlots.Length; i++)
            {
                EnemyShotSlot slot = weapon.shotSlots[i];
                if (slot.spawnDelay > 0f)
                    StartCoroutine(FireSlotAfterDelay(weapon, slot, muzzles, baseDirection, targetTagOverride));
                else
                    FireSlot(weapon, slot, muzzles, baseDirection, targetTagOverride);
            }

            if (burstIndex < bursts - 1 && weapon.burstInterval > 0f)
                yield return new WaitForSeconds(weapon.burstInterval);
        }
    }

    private IEnumerator FireSlotAfterDelay(EnemyWeaponDefinition weapon, EnemyShotSlot slot, Transform[] muzzles, Vector3 baseDirection, string targetTagOverride)
    {
        yield return new WaitForSeconds(slot.spawnDelay);
        FireSlot(weapon, slot, muzzles, baseDirection, targetTagOverride);
    }

    private void FireSlot(EnemyWeaponDefinition weapon, EnemyShotSlot slot, Transform[] muzzles, Vector3 baseDirection, string targetTagOverride)
    {
        if (slot.bulletPrefab == null)
            return;

        Transform muzzle = ResolveMuzzle(muzzles, slot.muzzleIndex);
        Vector3 shotDirection = Quaternion.Euler(0f, slot.angleOffset, 0f) * baseDirection;
        shotDirection.Normalize();
        Quaternion shotRotation = Quaternion.LookRotation(shotDirection, Vector3.up);
        Vector3 spawnPosition = muzzle.position + muzzle.TransformVector(slot.positionOffset);

        GameObject spawned = Instantiate(slot.bulletPrefab, spawnPosition, shotRotation);
        ConfigureProjectile(weapon, slot, spawned, shotDirection, targetTagOverride);
    }

    private Transform ResolveMuzzle(Transform[] muzzles, int muzzleIndex)
    {
        if (muzzles != null && muzzleIndex >= 0 && muzzleIndex < muzzles.Length && muzzles[muzzleIndex] != null)
            return muzzles[muzzleIndex];

        return transform;
    }

    private void ConfigureProjectile(EnemyWeaponDefinition weapon, EnemyShotSlot slot, GameObject spawned, Vector3 shotDirection, string targetTagOverride)
    {
        if (spawned == null)
            return;

        string targetTag = string.IsNullOrEmpty(targetTagOverride) ? weapon.defaultTargetTag : targetTagOverride;
        string sourceId = string.IsNullOrEmpty(weapon.sourceId) ? weapon.name : weapon.sourceId;

        if (spawned.TryGetComponent(out Bullet bullet))
        {
            bullet.SetDirection(shotDirection);
            bullet.SetTargetTag(targetTag);
            bullet.ConfigureEffects(slot.appliesSlow, slot.slowPercent, slot.slowDuration, sourceId);
        }

        if (spawned.TryGetComponent(out ArenaBullet arenaBullet))
        {
            arenaBullet.Init(shotDirection, targetTag);
            arenaBullet.ConfigureEffects(slot.appliesSlow, slot.slowPercent, slot.slowDuration, sourceId);
        }
    }
}
