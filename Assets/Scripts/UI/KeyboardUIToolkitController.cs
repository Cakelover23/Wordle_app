using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Renders the on-screen keyboard using UI Toolkit (Keyboard.uxml/.uss), mirroring the state of
/// the existing legacy keyboard <see cref="Button"/>s (via Board's letterButtonMap) instead of
/// replacing them. Board keeps 100% of its game logic (guess handling, letter coloring)
/// completely unchanged; this controller only forwards key presses to Board's public
/// PressLetterKey/PressBackspaceKey/PressEnterKey methods and mirrors each key's color via
/// Board's LetterKeyColorChanged event, then hides the legacy button rendering so only the new
/// keyboard is visible. Created automatically by <see cref="KeyboardUIToolkitBootstrapper"/>.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class KeyboardUIToolkitController : MonoBehaviour
{
    // Matches the legacy keyboard's layout exactly (see the screenshot in the scene's keyboard
    // prefab): standard QWERTY rows, with wide Enter/Back keys bookending the bottom row.
    private static readonly string[] Row1 = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
    private static readonly string[] Row2 = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
    private static readonly string[] Row3 = { "ENTER", "Z", "X", "C", "V", "B", "N", "M", "BACK" };

    private Board board;
    private readonly Dictionary<char, (UnityEngine.UIElements.Button button, Label label)> _keyElements = new Dictionary<char, (UnityEngine.UIElements.Button, Label)>();

    private void OnEnable()
    {
        board = FindFirstObjectByType<Board>();
        if (board == null)
        {
            Debug.LogWarning("KeyboardUIToolkitController: no Board found in scene - keyboard will not render.");
            return;
        }

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement rows = root.Q<VisualElement>("keyboard-rows");
        rows.Clear();
        _keyElements.Clear();

        rows.Add(BuildRow(Row1));
        rows.Add(BuildRow(Row2));
        rows.Add(BuildRow(Row3));

        board.HideLegacyKeyboardVisuals();
        board.LetterKeyColorChanged += OnLetterKeyColorChanged;
    }

    private void OnDisable()
    {
        if (board != null)
        {
            board.LetterKeyColorChanged -= OnLetterKeyColorChanged;
        }
        _keyElements.Clear();
    }

    private VisualElement BuildRow(string[] keys)
    {
        var rowElement = new VisualElement();
        rowElement.AddToClassList("keyboard-row");
        rowElement.pickingMode = PickingMode.Ignore; // only the individual keys should be clickable

        foreach (string key in keys)
        {
            rowElement.Add(BuildKey(key));
        }

        return rowElement;
    }

    private VisualElement BuildKey(string key)
    {
        var button = new UnityEngine.UIElements.Button();
        button.AddToClassList("keyboard-key");

        var label = new Label(key);
        label.AddToClassList("keyboard-key__label");
        label.pickingMode = PickingMode.Ignore;
        if (key.Length > 1)
        {
            label.AddToClassList("keyboard-key__label--small");
        }
        button.Add(label);

        switch (key)
        {
            case "ENTER":
                button.AddToClassList("keyboard-key--wide");
                button.clicked += () => board.PressEnterKey();
                ApplyColor(button, label, board.defaultColor);
                break;
            case "BACK":
                button.AddToClassList("keyboard-key--wide");
                button.clicked += () => board.PressBackspaceKey();
                ApplyColor(button, label, board.defaultColor);
                break;
            default:
                char letter = key[0];
                char lowerLetter = char.ToLowerInvariant(letter);
                button.clicked += () => board.PressLetterKey(lowerLetter);
                ApplyColor(button, label, board.GetKeyColor(lowerLetter));
                _keyElements[lowerLetter] = (button, label);
                break;
        }

        return button;
    }

    private void OnLetterKeyColorChanged(char letter, Color color)
    {
        if (_keyElements.TryGetValue(letter, out (UnityEngine.UIElements.Button button, Label label) entry))
        {
            ApplyColor(entry.button, entry.label, color);
        }
        else
        {
            Debug.LogWarning($"KeyboardUIToolkitController: received color change for '{letter}' but no matching key element was found.");
        }
    }

    private static void ApplyColor(VisualElement element, Label label, Color color)
    {
        element.style.backgroundColor = new StyleColor(color);

        // Pick readable text color based on the key's background brightness (perceived
        // luminance) instead of always using white, since default/unrevealed keys are often a
        // light gray where white text is hard to read.
        float luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        label.style.color = new StyleColor(luminance > 0.6f ? new Color(0.1f, 0.1f, 0.1f) : Color.white);
    }
}
