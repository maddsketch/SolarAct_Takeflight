using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityDuration = 0f;

    private int currentHealth;
    private float invincibilityTimer;

    public int Current => currentHealth;
    public int Max => maxHealth;

    public UnityEvent onDeath = new UnityEvent();
    public UnityEvent onDamaged = new UnityEvent();
    public UnityEvent<float> onDamagedWithImpact = new UnityEvent<float>();
    public UnityEvent<int, int> onHealthChanged = new UnityEvent<int, int>(); // current, max

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetInvincible(float duration)
    {
        invincibilityTimer = Mathf.Max(invincibilityTimer, duration);
    }

    public void SetMaxHealth(int newMax)
    {
        int diff = newMax - maxHealth;
        maxHealth = newMax;
        currentHealth = Mathf.Clamp(currentHealth + diff, 1, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Directly set current HP — used when restoring from save data.
    public void SetCurrentHealth(int amount)
    {
        currentHealth = Mathf.Clamp(amount, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, 0f);
    }

    public void TakeDamage(int amount, float impactMagnitude)
    {
        if (invincibilityTimer > 0f) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invincibilityTimer = invincibilityDuration;

        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
            onDeath?.Invoke();
        else
        {
            onDamaged?.Invoke();
            onDamagedWithImpact?.Invoke(Mathf.Max(0f, impactMagnitude));
        }
    }
}
