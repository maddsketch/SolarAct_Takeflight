using System.Collections.Generic;
using UnityEngine;

// Persistent singleton. Hooks into game events and checks achievement conditions.
// Add all AchievementDefinition assets to the achievements list in the Inspector.
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<AchievementDefinition> achievements = new();

    public event System.Action<AchievementDefinition> onAchievementUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        GameStateManager.OnFlagSet          += OnFlagSet;
        GameStateManager.OnLevelCompleted   += OnLevelCompleted;
        GameStateManager.OnQuestCompleted   += OnQuestCompleted;

        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp += OnPlayerLevelUp;
    }

    void OnDisable()
    {
        GameStateManager.OnFlagSet          -= OnFlagSet;
        GameStateManager.OnLevelCompleted   -= OnLevelCompleted;
        GameStateManager.OnQuestCompleted   -= OnQuestCompleted;

        if (XPManager.Instance != null)
            XPManager.Instance.onLevelUp -= OnPlayerLevelUp;
    }

    // --- Called externally by EnemyController and InventoryManager ---

    public void RegisterKill()
    {
        var data = GameStateManager.Instance.Current;
        data.totalKills++;
        CheckThresholdAchievements(AchievementConditionType.KillCount, data.totalKills);
    }

    public void RegisterCurrencyEarned(int amount)
    {
        var data = GameStateManager.Instance.Current;
        data.totalCurrency += amount;
        CheckThresholdAchievements(AchievementConditionType.Currency, data.totalCurrency);
    }

    // --- Event handlers ---

    private void OnFlagSet(string flag)
    {
        foreach (var a in achievements)
        {
            if (a.conditionType == AchievementConditionType.Flag &&
                a.conditionStringValue == flag)
                TryUnlock(a);
        }
    }

    private void OnLevelCompleted(string levelID)
    {
        foreach (var a in achievements)
        {
            if (a.conditionType == AchievementConditionType.LevelComplete &&
                a.conditionStringValue == levelID)
                TryUnlock(a);
        }
    }

    private void OnQuestCompleted(string questID)
    {
        foreach (var a in achievements)
        {
            if (a.conditionType == AchievementConditionType.QuestComplete &&
                a.conditionStringValue == questID)
                TryUnlock(a);
        }
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        CheckThresholdAchievements(AchievementConditionType.PlayerLevel, newLevel);
    }

    private void CheckThresholdAchievements(AchievementConditionType type, int currentValue)
    {
        foreach (var a in achievements)
        {
            if (a.conditionType == type && currentValue >= a.conditionThreshold)
                TryUnlock(a);
        }
    }

    // --- Unlock ---

    private void TryUnlock(AchievementDefinition achievement)
    {
        var data = GameStateManager.Instance.Current;

        if (data.unlockedAchievements.Contains(achievement.achievementID)) return;

        data.unlockedAchievements.Add(achievement.achievementID);
        onAchievementUnlocked?.Invoke(achievement);

        Debug.Log($"[AchievementManager] Unlocked: {achievement.displayName}");
    }

    public bool IsUnlocked(string achievementID) =>
        GameStateManager.Instance.Current.unlockedAchievements.Contains(achievementID);

    public List<AchievementDefinition> GetAll() => achievements;
}
