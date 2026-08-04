using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [System.Serializable]
    public class State
    {
        public Color fillColor;
        public Color outlineColor;
    }

    public State state { get; private set; }
    public char letter { get; private set; }

    /// <summary>Raised whenever <see cref="SetLetter"/> is called - lets an external renderer
    /// (e.g. BoardUIToolkitController) mirror this tile's letter without Board needing to know
    /// about it.</summary>
    public event Action<char> LetterChanged;

    /// <summary>Raised whenever <see cref="SetState"/> is called - lets an external renderer
    /// mirror this tile's fill/outline colors without Board needing to know about it.</summary>
    public event Action<State> StateChanged;

    private Image fill;
    private Outline outline;
    private TextMeshProUGUI text;

    private void Awake()
    {
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString();
        LetterChanged?.Invoke(letter);
    }

    public void SetState(State state)
    {
        this.state = state;
        fill.color = state.fillColor;
        outline.effectColor = state.outlineColor;
        StateChanged?.Invoke(state);
    }

    /// <summary>
    /// Hides this tile's legacy uGUI rendering (Image/Outline/Text) without disabling the
    /// GameObject or MonoBehaviour, so Board's game logic keeps working untouched while a
    /// UI Toolkit view (BoardUIToolkitController) renders the tile instead.
    /// </summary>
    public void HideLegacyVisuals()
    {
        if (fill != null) fill.enabled = false;
        if (outline != null) outline.enabled = false;
        if (text != null) text.enabled = false;
    }

}
