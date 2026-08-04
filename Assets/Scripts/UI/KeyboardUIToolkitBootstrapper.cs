using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Auto-creates the UI Toolkit on-screen keyboard (Keyboard.uxml/.uss) the moment a game scene
/// loads - no manual scene wiring required. Mirrors <see cref="BoardUIToolkitBootstrapper"/> and
/// <see cref="GameHudBootstrapper"/>.
/// </summary>
public static class KeyboardUIToolkitBootstrapper
{
    private static readonly string[] TargetSceneNames = { "FiveLetterWordle", "SixLetterWordle" };

    private const string PanelSettingsResourcePath = "UI/Keyboard/KeyboardPanelSettings";
    private const string ThemeResourcePath = "UI/Keyboard/KeyboardTheme";
    private const string VisualTreeResourcePath = "UI/Keyboard/Keyboard";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (IsTargetScene(scene.name))
            {
                Show();
            }
        };

        if (IsTargetScene(SceneManager.GetActiveScene().name))
        {
            Show();
        }
    }

    private static bool IsTargetScene(string sceneName)
    {
        foreach (string name in TargetSceneNames)
        {
            if (name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    public static void Show()
    {
        if (Object.FindFirstObjectByType<KeyboardUIToolkitController>() != null)
        {
            return; // Already built for this scene load.
        }

        var visualTreeAsset = Resources.Load<VisualTreeAsset>(VisualTreeResourcePath);
        if (visualTreeAsset == null)
        {
            Debug.LogError($"KeyboardUIToolkitBootstrapper: missing VisualTreeAsset at Resources/{VisualTreeResourcePath}");
            return;
        }

        var host = new GameObject("KeyboardUISystem");
        var uiDocument = host.AddComponent<UIDocument>();
        uiDocument.panelSettings = LoadOrCreatePanelSettings();
        uiDocument.visualTreeAsset = visualTreeAsset;
        host.AddComponent<KeyboardUIToolkitController>();
    }

    private static PanelSettings LoadOrCreatePanelSettings()
    {
        PanelSettings existing = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
        if (existing != null)
        {
            return existing;
        }

        var settings = ScriptableObject.CreateInstance<PanelSettings>();
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(1440, 3200);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0.5f;
        // Same layer as BoardUIToolkitBootstrapper (0) since they occupy different screen
        // regions (board on top, keyboard at the bottom) and never overlap; both stay below
        // GameHud's sortingOrder (10) so its Pause/Stats/Leaderboard modals draw on top.
        settings.sortingOrder = 0;

        var theme = Resources.Load<ThemeStyleSheet>(ThemeResourcePath);
        if (theme != null)
        {
            settings.themeStyleSheet = theme;
        }

        return settings;
    }
}
