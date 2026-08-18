using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Auto-creates the Category Select UI Toolkit screen the moment the "Menu" scene loads - no
/// manual scene wiring (no UIDocument/PanelSettings to drag in) required. This keeps the feature
/// fully driven by committed C#/asset files, so it works the instant this branch is opened in
/// Unity 6, without any hand-edited scene YAML.
///
/// If you'd rather show Category Select behind a specific button instead of automatically, move
/// the call to <see cref="Show"/> into that button's onClick handler and remove the automatic
/// scene-load hook below.
/// </summary>
public static class CategorySelectBootstrapper
{
    private const string TargetSceneName = "Menu";
    private const string PanelSettingsResourcePath = "UI/CategorySelect/CategorySelectPanelSettings";
    private const string ThemeResourcePath = "UI/CategorySelect/CategorySelectTheme";
    private const string VisualTreeResourcePath = "UI/CategorySelect/CategorySelect";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == TargetSceneName)
            {
                Show();
            }
        };

        if (SceneManager.GetActiveScene().name == TargetSceneName)
        {
            Show();
        }
    }

    public static void Show()
    {
        if (Object.FindFirstObjectByType<CategorySelectUIController>() != null)
        {
            return; // Already built for this scene load.
        }

        var visualTreeAsset = Resources.Load<VisualTreeAsset>(VisualTreeResourcePath);
        if (visualTreeAsset == null)
        {
            Debug.LogError($"CategorySelectBootstrapper: missing VisualTreeAsset at Resources/{VisualTreeResourcePath}");
            return;
        }

        var host = new GameObject("CategorySelectUISystem");
        var uiDocument = host.AddComponent<UIDocument>();
        uiDocument.panelSettings = LoadOrCreatePanelSettings();
        uiDocument.visualTreeAsset = visualTreeAsset;
        host.AddComponent<CategorySelectUIController>();
    }

    private static PanelSettings LoadOrCreatePanelSettings()
    {
        // Prefer a hand-tuned PanelSettings asset if one is ever added at this path; otherwise
        // fall back to sane runtime defaults. ScaleWithScreenSize against the project's actual
        // target resolution (1440x3200 portrait phone) scales the whole panel proportionally to
        // fit whatever the current render target size is - so it renders pixel-accurate at
        // 1440x3200 (the real device) and scales down cleanly (no cropping/overflow) in a
        // smaller Editor Game View, as long as CategorySelect.uss values are sized for the
        // 1440x3200 reference (which they are).
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

        // Every element we use in CategorySelect.uss sets its own explicit colors, so this
        // screen renders fine without a theme. Without one, Unity logs a benign
        // "No Theme Style Sheet set" warning; to silence it, create Assets > Create > UI Toolkit
        // > Default Runtime Theme in the Editor and save it as a ThemeStyleSheet at
        // Assets/Resources/UI/CategorySelect/CategorySelectTheme.tss - it will be picked up here
        // automatically.
        var theme = Resources.Load<ThemeStyleSheet>(ThemeResourcePath);
        if (theme != null)
        {
            settings.themeStyleSheet = theme;
        }

        return settings;
    }
}
