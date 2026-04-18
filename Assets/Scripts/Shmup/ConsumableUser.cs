using UnityEngine;
using UnityEngine.InputSystem;

// Handles using the equipped consumable during a shmup level.
// Attach to the player GameObject alongside WeaponController.
// Requires a "UseConsumable" Button action in the Player action map.
[RequireComponent(typeof(Health))]
public class ConsumableUser : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction useAction;
    private Health health;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        TryResolveUseAction();
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        TryResolveUseAction();
        if (useAction != null)
            useAction.performed += OnUse;
    }

    void OnDisable()
    {
        if (useAction != null)
            useAction.performed -= OnUse;
    }

    private void OnUse(InputAction.CallbackContext _)
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
            return;

        if (InventoryManager.Instance == null) return;

        string id = InventoryManager.Instance.EquippedConsumableID;
        if (string.IsNullOrEmpty(id)) return;

        var def = InventoryManager.Instance.GetDefinition(id);
        if (def == null || def.category != ItemCategory.Consumable) return;

        // Consume one from inventory first — if we don't have it, abort.
        if (!InventoryManager.Instance.RemoveItem(id, 1)) return;

        ApplyEffect(def);

        // If we just used the last one, clear the equipped slot.
        if (!InventoryManager.Instance.HasItem(id))
            InventoryManager.Instance.EquipConsumable(null);
    }

    private void ApplyEffect(ItemDefinition def)
    {
        switch (def.consumableEffect)
        {
            case ConsumableEffect.Bomb:
                ExplodeAllEnemies(Mathf.RoundToInt(def.effectValue));
                break;

            case ConsumableEffect.ShieldBurst:
                health.SetInvincible(def.effectValue);
                break;

            case ConsumableEffect.Repair:
                health.Heal(Mathf.RoundToInt(def.effectValue));
                break;
        }
    }

    private void ExplodeAllEnemies(int damage)
    {
        // FindObjectsByType is Unity 2022+ — safe for Unity 6.
        foreach (var enemy in FindObjectsByType<EnemyController>())
            enemy.GetComponent<Health>()?.TakeDamage(damage);
    }

    private void TryResolveUseAction()
    {
        if (playerInput == null)
            return;

        var actions = playerInput.actions;
        if (actions == null)
            return;

        useAction = actions["UseConsumable"];
    }
}
