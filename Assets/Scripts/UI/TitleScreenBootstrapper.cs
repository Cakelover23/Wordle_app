using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-attaches <see cref="TitleScreenController"/> to the Menu scene's "OpenAnimation" title
/// screen GameObject (the one with the Animator + TitleBar components) the moment the "Menu"
/// scene loads - no manual scene wiring required, matching how CategorySelectBootstrapper/
/// GameHudBootstrapper attach their controllers.
/// </summary>
public static class TitleScreenBootstrapper
{
    private const string TargetSceneName = "Menu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == TargetSceneName)
            {
                Attach();
            }
        };

        if (SceneManager.GetActiveScene().name == TargetSceneName)
        {
            Attach();
        }
    }

    private static void Attach()
    {
        var titleBar = Object.FindFirstObjectByType<TitleBar>();
        if (titleBar == null)
        {
            Debug.LogWarning("TitleScreenBootstrapper: no TitleBar found in the Menu scene - title screen open/close animation will not run.");
            return;
        }

        if (titleBar.GetComponent<TitleScreenController>() == null)
        {
            titleBar.gameObject.AddComponent<TitleScreenController>();
        }
    }
}
