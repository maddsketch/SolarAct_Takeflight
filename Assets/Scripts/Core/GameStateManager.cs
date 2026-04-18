using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent singleton that survives scene loads.
// Holds the active SaveData in memory and handles disk I/O.
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public SaveData Current { get; private set; } = new SaveData();

    // Set this before transitioning to the shmup scene so it knows what to load
    public string LevelToLoad { get; private set; }

    // Static events — AchievementManager subscribes to these across scenes
    public static event System.Action<string> OnFlagSet;
    public static event System.Action<string> OnLevelCompleted;
    public static event System.Action<string> OnQuestCompleted;

    private const int SlotCount = 3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Story flags ---

    public void SetFlag(string flag)
    {
        if (!Current.storyFlags.Contains(flag))
        {
            Current.storyFlags.Add(flag);
            OnFlagSet?.Invoke(flag);
        }
    }

    public bool HasFlag(string flag) => Current.storyFlags.Contains(flag);

    public void ClearFlag(string flag) => Current.storyFlags.Remove(flag);

    // --- Level completion ---

    public void CompleteLevel(string levelID)
    {
        if (!Current.completedLevels.Contains(levelID))
            Current.completedLevels.Add(levelID);

        // Also set as a flag so ZoneTriggers can check it like any other condition
        SetFlag(levelID);
        OnLevelCompleted?.Invoke(levelID);
    }

    public bool HasCompletedLevel(string levelID) => Current.completedLevels.Contains(levelID);

    // --- Quests ---

    public void StartQuest(string questID)
    {
        if (!Current.activeQuestIDs.Contains(questID))
            Current.activeQuestIDs.Add(questID);
    }

    public void CompleteQuest(string questID)
    {
        Current.activeQuestIDs.Remove(questID);
        SetFlag($"quest_complete_{questID}");
        OnQuestCompleted?.Invoke(questID);
    }

    // --- New game ---

    public void NewGame() => Current = new SaveData();

    // --- Transition helpers ---

    public void SetLevelToLoad(string levelID) => LevelToLoad = levelID;

    public void CaptureOverworldPosition(Vector3 position)
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            ApplyOverworldCaptureFrom(player.transform);
        else
        {
            Current.overworldPosition = position;
            Current.hasOverworldPosition = true;
            Current.lastOverworldSceneName = SceneManager.GetActiveScene().name;
            Current.overworldEulerAngles = Vector3.zero;
        }
    }

    void TryCaptureOverworldStateIfApplicable()
    {
        if (FindAnyObjectByType<OverworldSceneBootstrap>() == null)
            return;

        var player = GameObject.FindWithTag("Player");
        if (player == null)
            return;

        ApplyOverworldCaptureFrom(player.transform);
    }

    void ApplyOverworldCaptureFrom(Transform playerTransform)
    {
        Current.overworldPosition = playerTransform.position;
        Current.hasOverworldPosition = true;
        Current.lastOverworldSceneName = SceneManager.GetActiveScene().name;
        Current.overworldEulerAngles = playerTransform.eulerAngles;
    }

    // --- Save / Load ---

    public void Save(int slot)
    {
        TryCaptureOverworldStateIfApplicable();
        Current.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        File.WriteAllText(SavePath(slot), JsonUtility.ToJson(Current, prettyPrint: true));
        Debug.Log($"[GameStateManager] Saved to slot {slot}");
    }

    public bool Load(int slot)
    {
        string path = SavePath(slot);
        if (!File.Exists(path)) return false;

        Current = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        Debug.Log($"[GameStateManager] Loaded slot {slot}");
        return true;
    }

    public bool HasSave(int slot) => File.Exists(SavePath(slot));

    // Read save metadata for the menu without making it the active save
    public SaveData PeekSave(int slot)
    {
        string path = SavePath(slot);
        return File.Exists(path)
            ? JsonUtility.FromJson<SaveData>(File.ReadAllText(path))
            : null;
    }

    public void DeleteSave(int slot)
    {
        string path = SavePath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    private static string SavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
}
