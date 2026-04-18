using UnityEngine;

[DisallowMultipleComponent]
public class PanelFadeToggle : MonoBehaviour
{
    [SerializeField] private UIFadeController fadeController;

    private void Awake()
    {
        if (fadeController == null)
        {
            fadeController = GetComponent<UIFadeController>();
        }
    }

    public void Toggle()
    {
        if (fadeController == null)
        {
            return;
        }

        fadeController.Toggle();
    }

    public void Open()
    {
        if (fadeController == null)
        {
            return;
        }

        fadeController.Show();
    }

    public void Close()
    {
        if (fadeController == null)
        {
            return;
        }

        fadeController.Hide();
    }

    public void SetVisibleImmediate(bool visible)
    {
        if (fadeController == null)
        {
            return;
        }

        fadeController.SetVisibleImmediate(visible);
    }
}
