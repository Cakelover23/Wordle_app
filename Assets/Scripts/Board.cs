using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
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

    /// <summary>Read-only access to this board's rows/tiles, for external renderers
    /// (e.g. BoardUIToolkitController) that need to mirror tile state without owning game logic.</summary>
    public Row[] Rows => rows;

    /// <summary>Raised whenever a keyboard letter's color changes (correct/wrong-spot/incorrect/
    /// reset to default), so an external renderer (e.g. KeyboardUIToolkitController) can mirror
    /// on-screen keyboard state without duplicating Board's guess-checking logic.</summary>
    public event Action<char, Color> LetterKeyColorChanged;

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
    public TMP_Text correctWordText;
    public GameManager Keyboard;

    // Add references to the UI Buttons
    public Color defaultColor;
    public Color incorrectColor;
    public Color correctColor;
    public Color wrongSpotColor;

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

    // Word length for this board; SixBoard overrides this to 6.
    protected virtual int WordLength => 5;

    // Used only if no matching WordCategory can be found (keeps old scenes working untouched).
    protected virtual string DefaultSolutionsResource => "official_wordle_common";
    protected virtual string DefaultValidWordsResource => "official_wordle_all";

    protected virtual void LoadData()
    {
        WordCategory category = CategorySelectionService.GetSelectedCategory(WordLength);
        string solutionsPath = category != null ? category.solutionsPath : DefaultSolutionsResource;
        string validWordsPath = category != null ? category.validWordsPath : DefaultValidWordsResource;

        TextAsset textFile = Resources.Load<TextAsset>(solutionsPath);
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load<TextAsset>(validWordsPath);
        validWords = new HashSet<string>();
        foreach (string line in textFile.text.Split('\n'))
        {
            // Some word list files (e.g. official_wordle_all.txt) use Windows-style CRLF line
            // endings; splitting on '\n' alone leaves a trailing '\r' on every line except the
            // last, which would silently fail every guess-validity Contains() check below.
            string trimmed = line.ToLower().Trim();
            if (trimmed.Length > 0)
            {
                validWords.Add(trimmed);
            }
        }

        // Themed categories share the big classic dictionaries as their valid-guess list, so
        // make sure every one of this category's own solution words is guessable too, even if
        // a particular themed word (e.g. a genre or franchise-adjacent term) isn't in that
        // dictionary.
        foreach (string solution in solutions)
        {
            string trimmed = solution.ToLower().Trim();
            if (trimmed.Length > 0)
            {
                validWords.Add(trimmed);
            }
        }
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();
        GameManager.GameEvents.GameStart.TriggerEvent();
        ResetAllLetterButtons();
        enabled = true;
    }

    /// <summary>Letters this board has an on-screen keyboard key for, in the same casing used by
    /// the legacy keyboard buttons (matches the labels' text, e.g. uppercase 'Q').</summary>
    public IEnumerable<char> KeyboardLetters => letterButtonMap.Keys;

    /// <summary>Current display color for a keyboard letter key (default/correct/wrong-spot/
    /// incorrect), for external renderers to initialize from before subscribing to
    /// <see cref="LetterKeyColorChanged"/>.</summary>
    public Color GetKeyColor(char letter)
    {
        return letterButtonMap.TryGetValue(letter, out Button button) ? button.colors.normalColor : defaultColor;
    }

    /// <summary>Forwards a letter key press to the same logic the legacy on-screen keyboard
    /// button uses, for an external UI Toolkit keyboard renderer to call.</summary>
    public void PressLetterKey(char letter) => OnLetterButtonClick(letter);

    /// <summary>Forwards a backspace key press to the same logic the legacy backspace button
    /// uses, for an external UI Toolkit keyboard renderer to call.</summary>
    public void PressBackspaceKey() => OnBackspaceButtonClick();

    /// <summary>Forwards an enter key press to the same logic the legacy enter button uses, for
    /// an external UI Toolkit keyboard renderer to call.</summary>
    public void PressEnterKey() => OnEnterButtonClick();

    /// <summary>
    /// Hides the legacy on-screen keyboard's rendering (button backgrounds/labels) without
    /// disabling the GameObjects, so Board's game logic (letterButtonMap, click handlers) keeps
    /// working untouched while a UI Toolkit keyboard (KeyboardUIToolkitController) renders in
    /// its place.
    /// </summary>
    public void HideLegacyKeyboardVisuals()
    {
        foreach (Button button in letterButtons)
        {
            HideButtonVisual(button);
        }
        HideButtonVisual(backspaceButton);
        HideButtonVisual(enterButton);
    }

    private static void HideButtonVisual(Button button)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.enabled = false;
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.enabled = false;
        }
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
        word = solutions[UnityEngine.Random.Range(0, solutions.Length)].ToLower().Trim();
        correctWordText.GetComponent<TMP_Text>().SetText(word);
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
                CorrectLetterButton(tile.letter);
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
                    WrongSpotLetterButton(tile.letter);

                }
                else
                {
                    tile.SetState(incorrectState);
                    DisableLetterButton(tile.letter); // This line disables the letter on the keyboard UI
                }
            }
        }
        rowIndex++;
        columnIndex = 0;

        if (HasWon(row))
        {
            GameManager.GameEvents.GameEnd.TriggerEvent();
            //correctWordText.SetActive(true);
            enabled = false;
            GameManager.GameEvents.GameWon.TriggerEvent();
            return;
        }

    if (rowIndex >= rows.Length)
    {
        // Assuming GameManager.GameEvents has a GameLost event
        GameManager.GameEvents.GameEnd.TriggerEvent();
        GameManager.GameEvents.GameLost.TriggerEvent(); // Trigger the losing event
        // Update UI or state to reflect the game loss
        enabled = false; // Disable further input or game actions
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
        
        newWordButton.SetActive(false);
        //correctWordText.SetActive(false);
    }

    private void OnDisable()
    {
        
        newWordButton.SetActive(true);
    }

    private void DisableLetterButton(char letter)
    {
        Debug.Log($"Attempting to disable letter: {letter}");
    
        if (letterButtonMap.ContainsKey(letter) && letterButtonMap[letter].colors.normalColor != correctColor && letterButtonMap[letter].colors.normalColor != wrongSpotColor)
        {
            ColorBlock colors = letterButtonMap[letter].colors;
            colors.normalColor = incorrectColor;
            letterButtonMap[letter].colors = colors;
            LetterKeyColorChanged?.Invoke(letter, incorrectColor);
        }
    }

    private void CorrectLetterButton(char letter)
    {
        if (letterButtonMap.ContainsKey(letter))
        {
            ColorBlock colors = letterButtonMap[letter].colors;
            colors.normalColor = correctColor;
            letterButtonMap[letter].colors = colors;
            LetterKeyColorChanged?.Invoke(letter, correctColor);
        }
    }

    private void WrongSpotLetterButton(char letter)
    {
        if (letterButtonMap.ContainsKey(letter))
        {
            if(letterButtonMap[letter].colors.normalColor != correctColor)
            {
                ColorBlock colors = letterButtonMap[letter].colors;
                colors.normalColor = wrongSpotColor;
                letterButtonMap[letter].colors = colors;
                LetterKeyColorChanged?.Invoke(letter, wrongSpotColor);
            }
        }
    }

    private void ResetAllLetterButtons()
    {
        foreach (KeyValuePair<char, Button> entry in letterButtonMap)
        {
            ColorBlock colors = entry.Value.colors;
            colors.normalColor = defaultColor;
            entry.Value.colors = colors;
            LetterKeyColorChanged?.Invoke(entry.Key, defaultColor);
        }
    }
}