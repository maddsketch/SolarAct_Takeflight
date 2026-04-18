using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the HUD canvas in the shmup scene.
// Shows current level, XP bar, and a brief level-up flash.
public class XPUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider xpBar;
    [SerializeField] private GameObject levelUpFlash;    // a panel/text that briefly shows "LEVEL UP"
    [SerializeField] private float levelUpFlashDuration = 2f;

    private float flashTimer;

    void Start()
    {
        if (XPManager.Instance != null)
        {
            XPManager.Instance.onXPChanged += OnXPChanged;
            XPManager.Instance.onLevelUp   += OnLevelUp;
        }

        if (levelUpFlash != null) levelUpFlash.SetActive(false);

        Refresh();
    }

    void OnDestroy()
    {
        if (XPManager.Instance == null) return;
        XPManager.Instance.onXPChanged -= OnXPChanged;
        XPManager.Instance.onLevelUp   -= OnLevelUp;
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.unscaledDeltaTime;
            if (flashTimer <= 0f && levelUpFlash != null)
                levelUpFlash.SetActive(false);
        }
    }

    private void OnXPChanged(int current, int required)
    {
        if (xpBar != null)
            xpBar.value = required > 0 ? (float)current / required : 1f;
    }

    private void OnLevelUp(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LVL {newLevel}";

        if (levelUpFlash != null)
        {
            levelUpFlash.SetActive(true);
            flashTimer = levelUpFlashDuration;
        }
    }

    private void Refresh()
    {
        if (XPManager.Instance == null) return;

        if (levelText != null)
            levelText.text = $"LVL {XPManager.Instance.PlayerLevel}";

        if (xpBar != null)
        {
            int req = XPManager.Instance.XPToNextLevel;
            xpBar.value = req > 0 ? (float)XPManager.Instance.CurrentXP / req : 1f;
        }
    }
}
