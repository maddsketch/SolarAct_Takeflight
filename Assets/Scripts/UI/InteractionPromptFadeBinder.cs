using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionPromptFadeBinder : MonoBehaviour
{
    [SerializeField] private InteractionDetector detector;
    [SerializeField] private UIFadeController fadeController;
    [SerializeField] private TextMeshProUGUI promptLabel;
    [SerializeField] private bool hideWhenNoInteractable = true;

    private void Awake()
    {
        if (fadeController == null)
        {
            fadeController = GetComponent<UIFadeController>();
        }
    }

    private void OnEnable()
    {
        if (detector != null)
        {
            detector.onNearestChanged += HandleNearestChanged;
        }
    }

    private void OnDisable()
    {
        if (detector != null)
        {
            detector.onNearestChanged -= HandleNearestChanged;
        }
    }

    private void Start()
    {
        if (hideWhenNoInteractable && fadeController != null)
        {
            fadeController.SetVisibleImmediate(false);
        }
    }

    private void HandleNearestChanged(Interactable nearest)
    {
        if (promptLabel != null)
        {
            if (nearest != null)
                promptLabel.text = nearest.GetPromptDisplayText() ?? string.Empty;
            else
                promptLabel.text = string.Empty;
        }

        if (fadeController == null)
            return;

        if (nearest != null)
            fadeController.Show();
        else if (hideWhenNoInteractable)
            fadeController.Hide();
    }
}
