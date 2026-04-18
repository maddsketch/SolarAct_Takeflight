using UnityEngine;

// Attach to any GameObject in your test scene.
// Right-click this component in the Inspector to call any method.
public class DebugPanel : MonoBehaviour
{
    [Header("XP")]
    public int xpAmount = 50;

    [Header("Credits")]
    public int creditAmount = 100;

    [Header("Damage")]
    public int damageAmount = 1;

    // ---------------------------------------------------------------
    // XP

    [ContextMenu("Add XP")]
    void AddXP()
    {
        XPManager.Instance?.AddXP(xpAmount);
        Debug.Log($"[Debug] Added {xpAmount} XP. Level: {XPManager.Instance?.PlayerLevel} XP: {XPManager.Instance?.CurrentXP}/{XPManager.Instance?.XPToNextLevel}");
    }

    [ContextMenu("Max Level")]
    void MaxLevel()
    {
        if (XPManager.Instance == null) return;
        while (XPManager.Instance.PlayerLevel < XPManager.MaxLevel)
            XPManager.Instance.AddXP(99999);
        Debug.Log("[Debug] Max level reached.");
    }

    // ---------------------------------------------------------------
    // Credits

    [ContextMenu("Add Credits")]
    void AddCredits()
    {
        InventoryManager.Instance?.AddCurrency(creditAmount);
        Debug.Log($"[Debug] Added {creditAmount} credits. Total: {InventoryManager.Instance?.Currency}");
    }

    [ContextMenu("Remove Credits")]
    void RemoveCredits()
    {
        InventoryManager.Instance?.SpendCurrency(creditAmount);
        Debug.Log($"[Debug] Removed {creditAmount} credits. Total: {InventoryManager.Instance?.Currency}");
    }

    // ---------------------------------------------------------------
    // Health

    [ContextMenu("Take Damage")]
    void TakeDamage()
    {
        var player = GameObject.FindWithTag("Player");
        player?.GetComponent<Health>()?.TakeDamage(damageAmount);
        Debug.Log($"[Debug] Player took {damageAmount} damage.");
    }

    [ContextMenu("Heal Player")]
    void HealPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        var health = player?.GetComponent<Health>();
        if (health != null) health.Heal(damageAmount);
        Debug.Log("[Debug] Player healed.");
    }

    [ContextMenu("Kill Player")]
    void KillPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        var health = player?.GetComponent<Health>();
        if (health != null) health.TakeDamage(9999);
        Debug.Log("[Debug] Player killed.");
    }

    // ---------------------------------------------------------------
    // Save / Load

    [ContextMenu("Save Slot 0")]
    void Save() => GameStateManager.Instance?.Save(0);

    [ContextMenu("Load Slot 0")]
    void Load() => GameStateManager.Instance?.Load(0);

    [ContextMenu("Print Current State")]
    void PrintState()
    {
        var d = GameStateManager.Instance?.Current;
        if (d == null) { Debug.Log("[Debug] No GameState."); return; }
        var health = GameObject.FindWithTag("Player")?.GetComponent<Health>();
        string hpStr = health != null ? $"{health.Current}/{health.Max}" : "N/A";
        Debug.Log($"[Debug] Level:{d.playerLevel} XP:{d.currentXP} Credits:{d.currency} HP:{hpStr}");
    }
}
