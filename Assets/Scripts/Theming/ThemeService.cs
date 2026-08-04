using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Holds the list of available color themes and the player's current selection (persisted via
/// PlayerPrefs so it survives between scenes/sessions). Index 0 ("Original") is special: it means
/// "use whatever colors are already configured in the Inspector / USS files" - nothing gets
/// overridden - so players who never touch the theme setting see zero visual change from before
/// this feature existed. Selecting any other theme applies that palette on top.
/// </summary>
public static class ThemeService
{
    private const string PlayerPrefsKey = "SelectedThemeIndex";

    public static readonly GameTheme[] Themes =
    {
        // Placeholder entry for index 0 - values here are never read (see IsOriginal/Current).
        new GameTheme("Original", Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white),
        new GameTheme(
            "Ocean",
            accent: new Color32(39, 110, 158, 255),
            correct: new Color32(46, 138, 127, 255),
            wrongSpot: new Color32(214, 158, 54, 255),
            incorrect: new Color32(45, 58, 74, 255),
            tileEmpty: new Color32(16, 26, 36, 255),
            tileOccupied: new Color32(28, 42, 56, 255),
            keyDefault: new Color32(99, 126, 150, 255)),
        new GameTheme(
            "Sunset",
            accent: new Color32(196, 84, 64, 255),
            correct: new Color32(198, 120, 58, 255),
            wrongSpot: new Color32(214, 168, 60, 255),
            incorrect: new Color32(58, 42, 46, 255),
            tileEmpty: new Color32(32, 20, 24, 255),
            tileOccupied: new Color32(48, 32, 36, 255),
            keyDefault: new Color32(150, 104, 96, 255)),
        new GameTheme(
            "Forest",
            accent: new Color32(58, 110, 66, 255),
            correct: new Color32(72, 128, 74, 255),
            wrongSpot: new Color32(168, 142, 54, 255),
            incorrect: new Color32(42, 50, 42, 255),
            tileEmpty: new Color32(20, 28, 20, 255),
            tileOccupied: new Color32(32, 42, 32, 255),
            keyDefault: new Color32(110, 126, 104, 255)),
        new GameTheme(
            "Midnight Contrast",
            accent: new Color32(230, 196, 40, 255),
            correct: new Color32(60, 180, 110, 255),
            wrongSpot: new Color32(230, 196, 40, 255),
            incorrect: new Color32(60, 60, 66, 255),
            tileEmpty: new Color32(10, 10, 12, 255),
            tileOccupied: new Color32(26, 26, 30, 255),
            keyDefault: new Color32(150, 150, 158, 255)),
    };

    /// <summary>Raised when the selected theme changes, so already-open screens (e.g. Category
    /// Select) can restyle themselves immediately without needing a scene reload.</summary>
    public static event Action ThemeChanged;

    public static int SelectedIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PlayerPrefsKey, 0), 0, Themes.Length - 1);
        set
        {
            int clamped = Mathf.Clamp(value, 0, Themes.Length - 1);
            if (clamped == SelectedIndex)
            {
                return;
            }

            PlayerPrefs.SetInt(PlayerPrefsKey, clamped);
            PlayerPrefs.Save();
            ThemeChanged?.Invoke();
        }
    }

    /// <summary>True when the player has the special "Original" theme selected, meaning nothing
    /// should be overridden - callers should leave their existing Inspector/USS colors alone.</summary>
    public static bool IsOriginal => SelectedIndex == 0;

    public static GameTheme Current => Themes[SelectedIndex];

    /// <summary>Recolors every element under <paramref name="root"/> matching <paramref
    /// name="className"/> to the current theme's accent color (e.g. USS class "modal-button").
    /// When the "Original" theme is selected, inline overrides are cleared instead so each
    /// element falls back to its normal USS-defined color.</summary>
    public static void ApplyAccent(VisualElement root, string className)
    {
        if (root == null)
        {
            return;
        }

        Color? accent = IsOriginal ? (Color?)null : Current.accent;
        root.Query<VisualElement>(className: className).ForEach(element =>
        {
            element.style.backgroundColor = accent.HasValue ? new StyleColor(accent.Value) : new StyleColor(StyleKeyword.Null);
        });
    }
}
