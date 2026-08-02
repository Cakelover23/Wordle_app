using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Auto-creates the UI Toolkit Game HUD (Pause/Stats/Leaderboard/Username) the moment a game
/// scene loads - no manual scene wiring required. Mirrors <see cref="CategorySelectBootstrapper"/>.
/// </summary>
public static class GameHudBootstrapper
{
    private static readonly string[] TargetSceneNames = { "FiveLetterWordle", "SixLetterWordle" };

    private const string PanelSettingsResourcePath = "UI/GameHud/GameHudPanelSettings";
    private const string ThemeResourcePath = "UI/GameHud/GameHudTheme";
    private const string VisualTreeResourcePath = "UI/GameHud/GameHud";

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
        if (Object.FindFirstObjectByType<GameHudController>() != null)
        {
            return; // Already built for this scene load.
        }

        var visualTreeAsset = Resources.Load<VisualTreeAsset>(VisualTreeResourcePath);
        if (visualTreeAsset == null)
        {
            Debug.LogError($"GameHudBootstrapper: missing VisualTreeAsset at Resources/{VisualTreeResourcePath}");
            return;
        }

        var host = new GameObject("GameHudUISystem");
        var uiDocument = host.AddComponent<UIDocument>();
        uiDocument.panelSettings = LoadOrCreatePanelSettings();
        uiDocument.visualTreeAsset = visualTreeAsset;
        host.AddComponent<GameHudController>();
    }

    private static PanelSettings LoadOrCreatePanelSettings()
    {
        PanelSettings existing = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
        if (existing != null)
        {
            return existing;
        }

        var settings = ScriptableObject.CreateInstance<PanelSettings>();
        // ScaleWithScreenSize against this project's target phone resolution (1440x3200
        // portrait) keeps the authored GameHud.uss px sizes correct on the actual device while
        // still adapting proportionally to whatever size the Editor Game View happens to be.
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(1440, 3200);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0.5f;

        var theme = Resources.Load<ThemeStyleSheet>(ThemeResourcePath);
        if (theme != null)
        {
            settings.themeStyleSheet = theme;
        }

        return settings;
    }
}
