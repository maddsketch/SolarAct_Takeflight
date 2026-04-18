using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public Vector3 overworldPosition;

    // When true, overworldPosition is used even at world origin (legacy saves infer from non-zero position only).
    public bool hasOverworldPosition;

    // Last overworld sector scene; empty = MainMenu default.
    public string lastOverworldSceneName;

    public Vector3 overworldEulerAngles;

    public int playerHealth;

    // Level IDs of completed shmup levels
    public List<string> completedLevels = new();

    // Story flags — presence in this list means the flag is set.
    // e.g. "met_admiral", "unlocked_zone_canyon", "level_03_complete"
    public List<string> storyFlags = new();

    // Quest IDs currently active
    public List<string> activeQuestIDs = new();

    // Inventory
    public List<InventoryEntry> inventory = new();
    public string equippedWeaponID;
    public string equippedSecondaryWeaponID;
    public string equippedConsumableID;
    public int currency;

    // Cosmetics
    public string equippedHullID;
    public string equippedSkinID;
    public List<string> equippedAccessoryIDs = new();

    // XP & level
    public int playerLevel = 1;
    public int currentXP = 0;

    // Cumulative stat bonuses from automatic level-up upgrades
    public float speedBonus        = 0f;
    public int   maxHealthBonus    = 0;
    public float fireRateBonus     = 0f;  // negative = faster fire rate
    public float shieldBonus       = 0f;  // added to invincibility duration

    // Achievements
    public List<string> unlockedAchievements = new();
    public int totalKills    = 0;
    public int totalCurrency = 0;   // lifetime earned, never decrements

    public string saveTimestamp;
}
