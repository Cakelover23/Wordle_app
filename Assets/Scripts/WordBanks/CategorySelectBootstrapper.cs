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
        // fall back to sane runtime defaults for a portrait phone screen (matches this project's
        // existing 20x9 aspect ratio target).
        PanelSettings existing = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
        if (existing != null)
        {
            return existing;
        }

        var settings = ScriptableObject.CreateInstance<PanelSettings>();
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(1080, 1920);
        settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        settings.match = 0.5f;
        return settings;
    }
}
