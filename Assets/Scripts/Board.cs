using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[] {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z,
    };

    private Row[] rows;
    private int rowIndex;
    private int columnIndex;

    protected string[] solutions;
    protected HashSet<string> validWords;
    private string word;

    [Header("Tiles")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    [Header("UI")]
    public GameObject tryAgainButton;
    public GameObject newWordButton;
    public GameObject invalidWordText;
    public GameObject correctWordText;

    // Add references to the UI Buttons
    public Button[] letterButtons;
    public Button backspaceButton;
    public Button enterButton;

    // Dictionary to map letters to their corresponding buttons
    private Dictionary<char, Button> letterButtonMap;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();

        // Initialize the dictionary
        letterButtonMap = new Dictionary<char, Button>();

        // Add listeners for letter buttons
        foreach (Button button in letterButtons)
        {
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                char letter = buttonText.text[0];
                letterButtonMap[letter] = button; // Map the letter to the button
                button.onClick.AddListener(() => OnLetterButtonClick(letter));
            }
            else
            {
                Debug.LogError("Button is missing a TMP_Text component: " + button.name);
            }
        }

        // Add listeners for special buttons
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(OnBackspaceButtonClick);
        }
        else
        {
            Debug.LogError("Backspace button is not assigned.");
        }

        if (enterButton != null)
        {
            enterButton.onClick.AddListener(OnEnterButtonClick);
        }
        else
        {
            Debug.LogError("Enter button is not assigned.");
        }
    }

    private void Start()
    {
        LoadData();
        NewGame();
    }

    protected virtual void LoadData()
    {
        TextAsset textFile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = new HashSet<string>(textFile.text.Split('\n'));
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();
        GameManager.GameEvents.GameStart.TriggerEvent();
        // Enable all letter buttons at the start of a new game
        foreach (Button button in letterButtons)
        {
            button.interactable = true;
        }

        enabled = true;
    }

    public void TryAgain()
    {
        ClearBoard();

        // Enable all letter buttons when trying again
        foreach (Button button in letterButtons)
        {
            button.interactable = true;
        }

        enabled = true;
    }

    private void SetRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)].ToLower().Trim();
        Debug.Log("New word set: " + word);
    }

    private void Update()
    {
        // Handle input from the physical keyboard
        if (enabled)
        {
            Row currentRow = rows[rowIndex];

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                OnBackspaceButtonClick();
            }
            else if (columnIndex >= currentRow.tiles.Length)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    OnEnterButtonClick();
                }
            }
            else
            {
                for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
                {
                    if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                    {
                        OnLetterButtonClick((char)SUPPORTED_KEYS[i]);
                        break;
                    }
                }
            }
        }
    }

    private void OnLetterButtonClick(char letter)
    {
        Row currentRow = rows[rowIndex];
        if (columnIndex < currentRow.tiles.Length)
        {
            currentRow.tiles[columnIndex].SetLetter(letter);
            currentRow.tiles[columnIndex].SetState(occupiedState);
            columnIndex++;
        }
    }

    private void OnBackspaceButtonClick()
    {
        if (columnIndex > 0)
        {
            Row currentRow = rows[rowIndex];
            columnIndex = Mathf.Max(columnIndex - 1, 0);
            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);
            invalidWordText.SetActive(false);
        }
    }

    private void OnEnterButtonClick()
    {
        Row currentRow = rows[rowIndex];
        if (columnIndex >= currentRow.tiles.Length)
        {
            SubmitRow(currentRow);
        }
        else
        {
            Debug.Log("Not enough letters to submit.");
        }
    }

    private void SubmitRow(Row row)
    {
        if (!IsValidWord(row.Word))
        {
            invalidWordText.SetActive(true);
            return;
        }

        GameManager.GameEvents.GuessMade.TriggerEvent();
        string remaining = word;

        // First pass: Check for correct letters
        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];
            if (tile.letter == word[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1).Insert(i, " ");
            }
        }

        // Second pass: Check for wrong spot letters and incorrect letters
        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];
            if (tile.state != correctState)
            {
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);
                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1).Insert(index, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                    DisableLetterButton(tile.letter);
                }
            }
        }

        if (HasWon(row))
        {
            GameManager.GameEvents.GameEnd.TriggerEvent();
            correctWordText.SetActive(true);
            enabled = false;
            GameManager.GameEvents.GameWon.TriggerEvent();
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length)
        {
            correctWordText.SetActive(true);
            enabled = false;
        }
    }

    private bool IsValidWord(string word)
    {
        return validWords.Contains(word.ToLower().Trim());
    }

    private bool HasWon(Row row)
    {
        foreach (Tile tile in row.tiles)
        {
            if (tile.state != correctState)
            {
                return false;
            }
        }
        return true;
    }

    private void ClearBoard()
    {
        foreach (Row row in rows)
        {
            foreach (Tile tile in row.tiles)
            {
                tile.SetLetter('\0');
                tile.SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
    }

    private void OnEnable()
    {
        tryAgainButton.SetActive(false);
        newWordButton.SetActive(false);
        correctWordText.SetActive(false);
    }

    private void OnDisable()
    {
        tryAgainButton.SetActive(true);
        newWordButton.SetActive(true);
    }

    private void DisableLetterButton(char letter)
    {
        letter = char.ToUpper(letter); // Ensure we match the letter buttons which are likely uppercase
        if (letterButtonMap.ContainsKey(letter))
        {
            letterButtonMap[letter].interactable = false;
        }
    }
}