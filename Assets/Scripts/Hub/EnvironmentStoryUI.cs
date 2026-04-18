using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Attach to a Canvas panel in the Hub scene.
// Shows title + body text for EnvironmentStory interactables.
public class EnvironmentStoryUI : MonoBehaviour
{
    public static EnvironmentStoryUI Instance { get; private set; }

    public bool IsOpen { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(string title, string body)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText  != null) bodyText.text  = body;
        panel.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        panel.SetActive(false);
        IsOpen = false;
    }

    void Update()
    {
        if (IsOpen && Keyboard.current.eKey.wasPressedThisFrame)
            Close();
    }
}
