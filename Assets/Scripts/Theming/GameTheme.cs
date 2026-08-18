using UnityEngine;

/// <summary>
/// A single selectable color palette for the game. All tile/keyboard colors plus one shared
/// "accent" color used for buttons/active tabs/selected cards on the UI Toolkit menu and HUD
/// screens. See <see cref="ThemeService"/> for the list of available themes and how the current
/// selection is applied.
/// </summary>
[System.Serializable]
public class GameTheme
{
    public string name;

    /// <summary>Shared highlight color for buttons, active tabs, selected cards, etc. on the
    /// Category Select and Game HUD UI Toolkit screens.</summary>
    public Color accent;

    public Color correct;
    public Color wrongSpot;
    public Color incorrect;
    public Color tileEmpty;
    public Color tileOccupied;
    public Color keyDefault;

    public GameTheme(string name, Color accent, Color correct, Color wrongSpot, Color incorrect, Color tileEmpty, Color tileOccupied, Color keyDefault)
    {
        this.name = name;
        this.accent = accent;
        this.correct = correct;
        this.wrongSpot = wrongSpot;
        this.incorrect = incorrect;
        this.tileEmpty = tileEmpty;
        this.tileOccupied = tileOccupied;
        this.keyDefault = keyDefault;
    }
}
