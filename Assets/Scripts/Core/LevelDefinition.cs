using UnityEngine;

// At least this many kills → this mission completion payout. On win, the row with the
// largest minKills that the player still qualifies for is used. If none qualify, creditReward/xpReward apply.
[System.Serializable]
public struct KillPerformanceTier
{
    public int minKills;
    public int credits;
    public int xp;
}

// Create via Assets > Create > Shmup > Level Definition
// One asset per level. ShmupSceneBootstrap reads this to configure the level at runtime.
[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Shmup/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    public string levelID;          // unique key, e.g. "level_01_opening_assault"
    public string displayName;      // shown to player, e.g. "Opening Assault"
    public string sceneName;        // exact Unity scene name to load, e.g. "Level_01_OpeningAssault"
    public WaveDefinition[] waves;

    [Header("Pre-Mission Display")]
    [Range(1, 5)]
    public int difficulty = 1;
    [TextArea(2, 4)]
    public string briefingText = "Defeat all waves to complete this sector.";
    public int creditReward = 200;
    public int xpReward = 150;

    [Header("Kill performance (optional)")]
    public KillPerformanceTier[] killPerformanceTiers;

    /// <summary>Mission completion credits and XP after a win. Uses highest kill tier if configured; otherwise flat fields.</summary>
    /// <param name="qualifyingTierMinKills">minKills of the tier that set the payout, or -1 when flat creditReward/xpReward apply.</param>
    public static void GetMissionRewards(LevelDefinition level, int enemiesKilled, out int credits, out int xp, out int qualifyingTierMinKills)
    {
        credits = 0;
        xp = 0;
        qualifyingTierMinKills = -1;
        if (level == null) return;

        var tiers = level.killPerformanceTiers;
        if (tiers == null || tiers.Length == 0)
        {
            credits = level.creditReward;
            xp = level.xpReward;
            return;
        }

        int bestMin = -1;
        int bestCredits = level.creditReward;
        int bestXp = level.xpReward;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (enemiesKilled >= tiers[i].minKills && tiers[i].minKills >= bestMin)
            {
                bestMin = tiers[i].minKills;
                bestCredits = tiers[i].credits;
                bestXp = tiers[i].xp;
            }
        }

        credits = bestCredits;
        xp = bestXp;
        if (bestMin >= 0)
            qualifyingTierMinKills = bestMin;
    }
}
