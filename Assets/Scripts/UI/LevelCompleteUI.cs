using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI killsValueText;
    [SerializeField] private TextMeshProUGUI xpValueText;
    [SerializeField] private TextMeshProUGUI creditsValueText;

    [Header("Kill tier visuals")]
    [Tooltip("Parent for instantiated tier row images (e.g. empty RectTransform with Vertical Layout Group).")]
    [SerializeField] private RectTransform killTiersContainer;
    [SerializeField] private KillTierRowUI killTierRowPrefab;
    [SerializeField] private Sprite killTierLockedSprite;
    [SerializeField] private Sprite killTierReachedSprite;
    [SerializeField] private Sprite killTierRewardAppliedSprite;
    [Tooltip("Optional: shown when tiers exist but payout used base creditReward/xpReward.")]
    [SerializeField] private GameObject baseCompletionNotice;

    [Header("Button")]
    [SerializeField] private Button continueButton;

    public event Action onContinue;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Hide();

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(int score, int enemiesKilled, int xpReward, int creditReward, string levelName, LevelDefinition level, int qualifyingTierMinKills)
    {
        if (levelNameText != null) levelNameText.text = levelName;
        if (scoreValueText != null) scoreValueText.text = score.ToString("D6");
        if (killsValueText != null) killsValueText.text = enemiesKilled.ToString();
        if (xpValueText != null) xpValueText.text = $"+{xpReward}";
        if (creditsValueText != null) creditsValueText.text = $"+{creditReward}";

        PopulateKillTierImages(level, enemiesKilled, qualifyingTierMinKills);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    void PopulateKillTierImages(LevelDefinition level, int enemiesKilled, int qualifyingTierMinKills)
    {
        var tiers = level != null ? level.killPerformanceTiers : null;
        bool hasTiers = tiers != null && tiers.Length > 0;

        if (killTiersContainer != null)
            ClearKillTierRows();

        if (!hasTiers)
        {
            if (baseCompletionNotice != null)
                baseCompletionNotice.SetActive(false);
            return;
        }

        if (baseCompletionNotice != null)
            baseCompletionNotice.SetActive(qualifyingTierMinKills < 0);

        if (killTiersContainer == null || killTierRowPrefab == null)
            return;

        var sorted = new List<KillPerformanceTier>(tiers);
        sorted.Sort((a, b) => a.minKills.CompareTo(b.minKills));

        foreach (var t in sorted)
        {
            bool reached = enemiesKilled >= t.minKills;
            bool applied = qualifyingTierMinKills >= 0 && t.minKills == qualifyingTierMinKills;
            var row = Instantiate(killTierRowPrefab, killTiersContainer);
            row.SetState(killTierLockedSprite, killTierReachedSprite, killTierRewardAppliedSprite, reached, applied);
        }
    }

    void ClearKillTierRows()
    {
        if (killTiersContainer == null) return;
        for (int i = killTiersContainer.childCount - 1; i >= 0; i--)
            Destroy(killTiersContainer.GetChild(i).gameObject);
    }

    void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void OnContinueClicked()
    {
        onContinue?.Invoke();
    }
}
