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
    private readonly Dictionary<char, UnityEngine.UIElements.Button> _keyElements = new Dictionary<char, UnityEngine.UIElements.Button>();

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
                break;
            case "BACK":
                button.AddToClassList("keyboard-key--wide");
                button.clicked += () => board.PressBackspaceKey();
                break;
            default:
                char letter = key[0];
                button.clicked += () => board.PressLetterKey(letter);
                ApplyColor(button, board.GetKeyColor(letter));
                _keyElements[letter] = button;
                break;
        }

        return button;
    }

    private void OnLetterKeyColorChanged(char letter, Color color)
    {
        if (_keyElements.TryGetValue(letter, out UnityEngine.UIElements.Button button))
        {
            ApplyColor(button, color);
        }
    }

    private static void ApplyColor(VisualElement element, Color color)
    {
        element.style.backgroundColor = new StyleColor(color);
    }
}
