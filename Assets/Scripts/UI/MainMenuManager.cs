using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Attach to a manager GameObject in the MainMenu scene.
// Requires: GameStateManager and SceneTransitionManager prefabs also present in the scene.
public class MainMenuManager : MonoBehaviour
{
    // --- Scene ---
    [Header("Scene")]
    [SerializeField] private string overworldSceneName = "Sector001_map";

    // --- Panels ---
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject saveSlotPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    // --- Start screen ---
    [Header("Start Screen")]
    [SerializeField] private Button startButton;

    // --- Title buttons ---
    [Header("Title Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    // --- Save slot panel ---
    [Header("Save Slot Panel")]
    [SerializeField] private TextMeshProUGUI saveSlotPanelTitle; // "NEW GAME" / "CONTINUE"
    [SerializeField] private SaveSlotUI[] saveSlots;             // 3 elements
    [SerializeField] private Button saveSlotBackButton;

    // --- Settings panel ---
    [Header("Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button settingsBackButton;

    // --- Credits panel ---
    [Header("Credits")]
    [SerializeField] private Button creditsBackButton;

    // --- Fade ---
    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isNewGame;  // true = new game slot selection, false = continue

    private Coroutine selectionCoroutine;

    // ---------------------------------------------------------------

    void Start()
    {
        // Load saved volume prefs
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeSlider.value  = PlayerPrefs.GetFloat("MusicVolume",  1f);

        masterVolumeSlider.onValueChanged.AddListener(v => { AudioListener.volume = v; PlayerPrefs.SetFloat("MasterVolume", v); });
        musicVolumeSlider.onValueChanged.AddListener( v => { PlayerPrefs.SetFloat("MusicVolume", v); });

        // Apply saved master volume immediately
        AudioListener.volume = masterVolumeSlider.value;

        // Title buttons
        newGameButton.onClick.AddListener(OpenNewGame);
        continueButton.onClick.AddListener(OpenContinue);
        settingsButton.onClick.AddListener(OpenSettings);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(() => Application.Quit());

        // Save slot back
        saveSlotBackButton.onClick.AddListener(() => ShowPanel(titlePanel));

        // Settings back
        settingsBackButton.onClick.AddListener(() => ShowPanel(titlePanel));

        // Credits back
        creditsBackButton.onClick.AddListener(() => ShowPanel(titlePanel));

        // Disable continue if no saves exist
        bool anySave = false;
        for (int i = 0; i < 3; i++)
            if (GameStateManager.Instance != null && GameStateManager.Instance.HasSave(i))
                anySave = true;
        continueButton.interactable = anySave;

        // Start button
        startButton.onClick.AddListener(() => ShowPanel(titlePanel));

        // Start fully visible then fade in
        SetFadeAlpha(1f);
        ShowPanel(startPanel);
        StartCoroutine(FadeTo(0f));
    }

    // ---------------------------------------------------------------
    // Panel navigation

    void ShowPanel(GameObject target)
    {
        startPanel.SetActive(target == startPanel);
        titlePanel.SetActive(target == titlePanel);
        saveSlotPanel.SetActive(target == saveSlotPanel);
        settingsPanel.SetActive(target == settingsPanel);
        creditsPanel.SetActive(target == creditsPanel);

        ScheduleUISelection(ResolveDefaultSelectable(target));
    }

    static GameObject ResolveButtonGameObject(Button button)
    {
        return button != null ? button.gameObject : null;
    }

    GameObject ResolveDefaultSelectable(GameObject targetPanel)
    {
        if (targetPanel == startPanel)
            return ResolveButtonGameObject(startButton);
        if (targetPanel == titlePanel)
            return ResolveButtonGameObject(newGameButton);
        if (targetPanel == saveSlotPanel)
            return FirstInteractableSaveSlot() ?? ResolveButtonGameObject(saveSlotBackButton);
        if (targetPanel == settingsPanel)
        {
            if (masterVolumeSlider != null)
                return masterVolumeSlider.gameObject;
            return ResolveButtonGameObject(settingsBackButton);
        }
        if (targetPanel == creditsPanel)
            return ResolveButtonGameObject(creditsBackButton);

        return null;
    }

    GameObject FirstInteractableSaveSlot()
    {
        if (saveSlots == null) return null;
        for (int i = 0; i < saveSlots.Length; i++)
        {
            var btn = saveSlots[i] != null ? saveSlots[i].SelectButton : null;
            if (btn != null && btn.interactable && btn.gameObject.activeInHierarchy)
                return btn.gameObject;
        }

        return null;
    }

    void ScheduleUISelection(GameObject selection)
    {
        if (selectionCoroutine != null)
            StopCoroutine(selectionCoroutine);
        selectionCoroutine = StartCoroutine(CoApplyUISelection(selection));
    }

    IEnumerator CoApplyUISelection(GameObject selection)
    {
        yield return null;

        if (selection == null || !selection.activeInHierarchy)
            yield break;

        EventSystem es = EventSystem.current;
        if (es == null)
            yield break;

        es.SetSelectedGameObject(selection);
    }

    void OpenNewGame()
    {
        isNewGame = true;
        if (saveSlotPanelTitle != null) saveSlotPanelTitle.text = "NEW GAME — Select Slot";
        PopulateSaveSlots();
        ShowPanel(saveSlotPanel);
    }

    void OpenContinue()
    {
        isNewGame = false;
        if (saveSlotPanelTitle != null) saveSlotPanelTitle.text = "CONTINUE — Select Slot";
        PopulateSaveSlots();
        ShowPanel(saveSlotPanel);
    }

    void OpenSettings()  => ShowPanel(settingsPanel);
    void OpenCredits()   => ShowPanel(creditsPanel);

    // ---------------------------------------------------------------
    // Save slots

    void PopulateSaveSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            int slot = i; // capture for lambda
            saveSlots[i].Populate(i, OnSlotSelected);

            // In Continue mode, disable slots that have no save
            if (!isNewGame)
            {
                bool hasSave = GameStateManager.Instance != null && GameStateManager.Instance.HasSave(slot);
                var sel = saveSlots[i].SelectButton;
                if (sel != null)
                    sel.interactable = hasSave;
            }
        }
    }

    void OnSlotSelected(int slot)
    {
        if (isNewGame)
        {
            GameStateManager.Instance.NewGame();
            GameStateManager.Instance.Save(slot);
        }
        else
        {
            if (!GameStateManager.Instance.Load(slot)) return;
        }

        SceneTransitionManager.Instance.ActiveSaveSlot = slot;
        StartCoroutine(LoadOverworld());
    }

    // ---------------------------------------------------------------
    // Transition

    IEnumerator LoadOverworld()
    {
        yield return StartCoroutine(FadeTo(1f));

        string scene = GameStateManager.Instance != null
            ? GameStateManager.Instance.Current.lastOverworldSceneName
            : null;
        if (string.IsNullOrEmpty(scene) || !Application.CanStreamedLevelBeLoaded(scene))
            scene = overworldSceneName;

        yield return SceneManager.LoadSceneAsync(scene);
    }

    IEnumerator FadeTo(float target)
    {
        float start   = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetFadeAlpha(Mathf.Lerp(start, target, elapsed / fadeDuration));
            yield return null;
        }

        SetFadeAlpha(target);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}
