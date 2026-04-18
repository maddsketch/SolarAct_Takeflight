using UnityEngine;
using UnityEngine.SceneManagement;

// Place in the Bootstrap scene alongside all persistent manager prefabs.
// Immediately loads the target scene after singletons are initialized.
public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string targetScene = "MainMenu";

#if UNITY_EDITOR
    // In editor, allow direct scene testing by skipping to the active scene
    [SerializeField] private bool skipToActiveScene = true;
#endif

    void Start()
    {
#if UNITY_EDITOR
        if (skipToActiveScene)
        {
            // If we're already in a non-bootstrap scene just let it run
            if (SceneManager.GetActiveScene().name != "Bootstrap")
                return;
        }
#endif
        SceneManager.LoadScene(targetScene);
    }
}
