using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Renders the game board (rows/tiles) using UI Toolkit (Board.uxml/.uss), mirroring the state
/// of the existing legacy <see cref="Board"/>/<see cref="Row"/>/<see cref="Tile"/> components
/// instead of replacing them. Board keeps 100% of its game logic (word selection, guess
/// validation, win/loss, on-screen keyboard coloring) completely unchanged; this controller only
/// listens to each Tile's LetterChanged/StateChanged events and mirrors them onto new
/// VisualElements, then hides the legacy Image/Outline/Text rendering so only the new grid is
/// visible. Created automatically by <see cref="BoardUIToolkitBootstrapper"/>.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class BoardUIToolkitController : MonoBehaviour
{
    private readonly List<Tile> _boundTiles = new List<Tile>();

    private void OnEnable()
    {
        Board board = FindFirstObjectByType<Board>();
        if (board == null)
        {
            Debug.LogWarning("BoardUIToolkitController: no Board found in scene - board grid will not render.");
            return;
        }

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        VisualElement grid = root.Q<VisualElement>("board-grid");
        grid.Clear();
        _boundTiles.Clear();

        foreach (Row row in board.Rows)
        {
            var rowElement = new VisualElement();
            rowElement.AddToClassList("board-row");
            rowElement.pickingMode = PickingMode.Ignore; // purely visual, must not block clicks to legacy UI below
            grid.Add(rowElement);

            foreach (Tile tile in row.tiles)
            {
                BindTile(tile, rowElement);
            }
        }
    }

    private void OnDisable()
    {
        // Tile/board GameObjects are torn down together with this controller on scene unload,
        // so the per-tile lambda subscriptions above don't outlive their targets - nothing to
        // explicitly unsubscribe here.
        _boundTiles.Clear();
    }

    private void BindTile(Tile tile, VisualElement rowElement)
    {
        var tileElement = new VisualElement();
        tileElement.AddToClassList("board-tile");
        tileElement.pickingMode = PickingMode.Ignore; // purely visual, must not block clicks to legacy UI below

        var label = new Label();
        label.AddToClassList("board-tile__label");
        label.pickingMode = PickingMode.Ignore;
        tileElement.Add(label);
        rowElement.Add(tileElement);

        tile.HideLegacyVisuals();

        // Initialize from the tile's current state/letter (Board already ran NewGame/ClearBoard
        // by the time this bootstrapper attaches), then keep mirroring future changes.
        ApplyLetter(label, tile.letter);
        ApplyState(tileElement, tile.state);

        tile.LetterChanged += letter => ApplyLetter(label, letter);
        tile.StateChanged += state => ApplyState(tileElement, state);

        _boundTiles.Add(tile);
    }

    private static void ApplyLetter(Label label, char letter)
    {
        label.text = letter == '\0' ? string.Empty : letter.ToString();
    }

    private static void ApplyState(VisualElement element, Tile.State state)
    {
        if (state == null)
        {
            return;
        }

        element.style.backgroundColor = new StyleColor(state.fillColor);
        element.style.borderTopColor = new StyleColor(state.outlineColor);
        element.style.borderBottomColor = new StyleColor(state.outlineColor);
        element.style.borderLeftColor = new StyleColor(state.outlineColor);
        element.style.borderRightColor = new StyleColor(state.outlineColor);
    }
}
