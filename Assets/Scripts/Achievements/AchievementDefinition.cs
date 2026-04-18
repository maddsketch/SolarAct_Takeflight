using UnityEngine;

[CreateAssetMenu(fileName = "Achievement_", menuName = "TakeFlight/Achievement")]
public class AchievementDefinition : ScriptableObject
{
    [Header("Identity")]
    public string achievementID;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Display")]
    [Tooltip("Hidden achievements show ??? in the menu until unlocked.")]
    public bool isHidden = false;

    [Header("Condition")]
    public AchievementConditionType conditionType;

    [Tooltip("Flag ID, Level ID, or Quest ID — used by Flag, LevelComplete, QuestComplete types.")]
    public string conditionStringValue;

    [Tooltip("Target count — used by KillCount, PlayerLevel, Currency types.")]
    public int conditionThreshold;
}
