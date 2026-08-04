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
        // Retro color palettes the player picked out (vintage poster/ad swatches) - accent,
        // correct and wrongSpot use the exact reference hex values from each swatch; the dark
        // neutrals (incorrect/tileEmpty/tileOccupied/keyDefault) are derived to match each
        // palette's warm/cool cast so the whole board still reads as one cohesive dark theme.
        new GameTheme(
            "Surf Poster",
            accent: new Color32(0x4B, 0x84, 0x8A, 255),
            correct: new Color32(0x73, 0xA6, 0x42, 255),
            wrongSpot: new Color32(0xFB, 0xC3, 0x59, 255),
            incorrect: new Color32(42, 42, 38, 255),
            tileEmpty: new Color32(20, 20, 16, 255),
            tileOccupied: new Color32(36, 36, 30, 255),
            keyDefault: new Color32(140, 132, 120, 255)),
        new GameTheme(
            "Terminal Green",
            accent: new Color32(0xB8, 0x3A, 0x2D, 255),
            correct: new Color32(0x4E, 0x68, 0x51, 255),
            wrongSpot: new Color32(0xDC, 0xC9, 0xA9, 255),
            incorrect: new Color32(42, 36, 32, 255),
            tileEmpty: new Color32(21, 18, 14, 255),
            tileOccupied: new Color32(36, 30, 24, 255),
            keyDefault: new Color32(140, 128, 112, 255)),
        new GameTheme(
            "Showa Diner",
            accent: new Color32(0xD1, 0x21, 0x28, 255),
            correct: new Color32(0x01, 0x34, 0x4F, 255),
            wrongSpot: new Color32(0xFA, 0xE3, 0xAC, 255),
            incorrect: new Color32(36, 28, 20, 255),
            tileEmpty: new Color32(18, 14, 10, 255),
            tileOccupied: new Color32(32, 24, 16, 255),
            keyDefault: new Color32(138, 127, 110, 255)),
        new GameTheme(
            "Torii Vermillion",
            accent: new Color32(0xFF, 0x46, 0x1F, 255),
            correct: new Color32(0x18, 0x5A, 0x56, 255),
            wrongSpot: new Color32(0xD9, 0xA4, 0x41, 255),
            incorrect: new Color32(42, 38, 34, 255),
            tileEmpty: new Color32(26, 23, 20, 255),
            tileOccupied: new Color32(34, 30, 26, 255),
            keyDefault: new Color32(140, 131, 120, 255)),
        new GameTheme(
            "Kissaten Crimson",
            accent: new Color32(0x81, 0x02, 0x1F, 255),
            correct: new Color32(0x0E, 0x84, 0x8E, 255),
            wrongSpot: new Color32(0xF2, 0xE3, 0xC6, 255),
            incorrect: new Color32(58, 52, 44, 255),
            tileEmpty: new Color32(20, 18, 16, 255),
            tileOccupied: new Color32(36, 31, 27, 255),
            keyDefault: new Color32(140, 126, 106, 255)),
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
