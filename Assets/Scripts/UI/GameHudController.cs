using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Drives the UI Toolkit game HUD (GameHud.uxml/.uss): Pause, Stats, Leaderboard and Username
/// overlays. Created automatically by <see cref="GameHudBootstrapper"/> in the FiveLetterWordle
/// / SixLetterWordle scenes - it replaces the legacy uGUI <see cref="UIManager"/> panels, reusing
/// the already-assigned references on the existing UIManager instance (Keyboard GameObject,
/// Stats, LeaderBoardManager, CorrectWordText) so nothing needs to be re-wired in the Editor.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class GameHudController : MonoBehaviour
{
    private VisualElement _pausePanel;
    private VisualElement _statsPanel;
    private VisualElement _leaderboardPanel;
    private VisualElement _usernamePanel;
    private VisualElement _leaderboardList;

    private Label _statsGames;
    private Label _statsWins;
    private Label _statsLosses;
    private Label _statsStreak;
    private Label _statsGuesses;

    private TextField _usernameField;
    private Button _pauseButton;
    private Button _newWordButton;

    private UIManager _legacyUIManager;
    private Stats _stats;
    private LeaderBoardManager _leaderBoardManager;
    private Board _board;
    private GameObject _onScreenKeyboard;
    private GameObject _correctWordText;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        _pauseButton = root.Q<Button>("pause-button");
        Button statsButton = root.Q<Button>("stats-button");
        Button leaderboardButton = root.Q<Button>("leaderboard-button");
        _newWordButton = root.Q<Button>("new-word-button");

        _pausePanel = root.Q<VisualElement>("pause-panel");
        _statsPanel = root.Q<VisualElement>("stats-panel");
        _leaderboardPanel = root.Q<VisualElement>("leaderboard-panel");
        _usernamePanel = root.Q<VisualElement>("username-panel");
        _leaderboardList = root.Q<VisualElement>("leaderboard-list");

        _statsGames = root.Q<Label>("stats-games");
        _statsWins = root.Q<Label>("stats-wins");
        _statsLosses = root.Q<Label>("stats-losses");
        _statsStreak = root.Q<Label>("stats-streak");
        _statsGuesses = root.Q<Label>("stats-guesses");

        _usernameField = root.Q<TextField>("username-field");

        _pauseButton.clicked += PauseGame;
        root.Q<Button>("resume-button").clicked += ResumeGame;
        root.Q<Button>("menu-button").clicked += GoToMainMenu;
        statsButton.clicked += DisplayStats;
        root.Q<Button>("stats-close-button").clicked += () => Hide(_statsPanel);
        leaderboardButton.clicked += DisplayLeaderBoard;
        root.Q<Button>("leaderboard-close-button").clicked += () => Hide(_leaderboardPanel);
        root.Q<Button>("username-submit-button").clicked += SubmitUsername;
        _newWordButton.clicked += StartNewWord;

        ThemeService.ApplyAccent(root, "modal-button");
        ThemeService.ApplyAccent(root, "chrome-button");

        LinkLegacyReferences();
        GameManager.GameEvents.GameStart.onGameEvent += UIGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent += UIGameEnd;
        GameManager.GameEvents.GameLost.onGameEvent += UIGameLost;

        if (_stats != null && !_stats.HasUsername)
        {
            Show(_usernamePanel);
            SetKeyboardActive(false);
        }
    }

    private void OnDisable()
    {
        GameManager.GameEvents.GameStart.onGameEvent -= UIGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent -= UIGameEnd;
        GameManager.GameEvents.GameLost.onGameEvent -= UIGameLost;
    }

    /// <summary>
    /// Finds the scene's existing UIManager and repurposes its already-assigned references,
    /// then disables its own panel management so the two systems don't fight over the same
    /// GameObjects.
    /// </summary>
    private void LinkLegacyReferences()
    {
        _legacyUIManager = FindFirstObjectByType<UIManager>();
        if (_legacyUIManager == null)
        {
            Debug.LogWarning("GameHudController: no legacy UIManager found in scene - Stats/Leaderboard/Keyboard references are unavailable.");
            return;
        }

        _stats = _legacyUIManager.Stats;
        _leaderBoardManager = _legacyUIManager.LeaderBoardManager;
        _onScreenKeyboard = _legacyUIManager.Keyboard;
        _correctWordText = _legacyUIManager.CorrectWordText;
        _board = FindFirstObjectByType<Board>();
        _board?.HideLegacyNewWordButtonVisuals();

        // Hand control of these GameObjects over to this HUD; disable the legacy panels so
        // they don't render underneath / duplicate ours.
        SetActiveIfNotNull(_legacyUIManager.PauseScreen, false);
        SetActiveIfNotNull(_legacyUIManager.PauseButton, false);
        SetActiveIfNotNull(_legacyUIManager.StatScreen, false);
        SetActiveIfNotNull(_legacyUIManager.LeaderBoardScreen, false);
        if (_stats != null)
        {
            SetActiveIfNotNull(_stats.UsernameInput, false);
        }

        _legacyUIManager.enabled = false;
    }

    private static void SetActiveIfNotNull(GameObject go, bool active)
    {
        if (go != null)
        {
            go.SetActive(active);
        }
    }

    private void SetKeyboardActive(bool active) => SetActiveIfNotNull(_onScreenKeyboard, active);

    #region Pause

    private void PauseGame()
    {
        Show(_pausePanel);
        SetKeyboardActive(false);
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        Hide(_pausePanel);
        SetKeyboardActive(true);
        Time.timeScale = 1;
        _pauseButton.SetEnabled(true);
    }

    private void GoToMainMenu()
    {
        // Restore normal time flow in case the game was paused, then leave this scene entirely.
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

    #endregion

    #region Stats

    private void DisplayStats()
    {
        if (_stats != null)
        {
            _statsGames.text = _stats._totalGamesPlayed.ToString();
            _statsWins.text = _stats._totalWins.ToString();
            _statsLosses.text = _stats._totalLosses.ToString();
            _statsStreak.text = _stats._currentWinStreak.ToString();
            _statsGuesses.text = _stats._totalGuesses.ToString();
        }

        Show(_statsPanel);
    }

    #endregion

    #region Leaderboard

    private async void DisplayLeaderBoard()
    {
        Show(_leaderboardPanel);
        _leaderboardList.Clear();

        if (_leaderBoardManager == null)
        {
            return;
        }

        try
        {
            await _leaderBoardManager.GetScores();
            foreach (var score in _leaderBoardManager.GetScoresList())
            {
                _leaderboardList.Add(BuildLeaderboardRow(score));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameHudController: failed to fetch scores: {e.Message}");
        }
    }

    private static VisualElement BuildLeaderboardRow(LeaderBoardManager.ScoreEntry score)
    {
        var row = new VisualElement();
        row.AddToClassList("leaderboard-row");

        var rank = new Label(score.Rank.ToString());
        rank.AddToClassList("leaderboard-rank");
        row.Add(rank);

        var name = new Label(score.PlayerId);
        name.AddToClassList("leaderboard-name");
        row.Add(name);

        var value = new Label(score.Score.ToString());
        value.AddToClassList("leaderboard-score");
        row.Add(value);

        return row;
    }

    #endregion

    #region Username

    private void SubmitUsername()
    {
        if (_stats == null || string.IsNullOrWhiteSpace(_usernameField.value))
        {
            return;
        }

        _stats.SubmitUsernameText(_usernameField.value);
        Hide(_usernamePanel);
        SetKeyboardActive(true);
    }

    #endregion

    #region Game Start/End/Lost

    private void UIGameStart()
    {
        SetActiveIfNotNull(_correctWordText, false);
        Hide(_newWordButton);
        ResumeGame();
    }

    private void UIGameEnd()
    {
        _pauseButton.SetEnabled(false);
        Show(_newWordButton);
        _leaderBoardManager?.UploadCurrentWinStreak();
    }

    private void UIGameLost()
    {
        SetActiveIfNotNull(_correctWordText, true);
    }

    #endregion

    private void StartNewWord() => _board?.NewGame();

    private static void Show(VisualElement element) => element.RemoveFromClassList("hidden");
    private static void Hide(VisualElement element) => element.AddToClassList("hidden");
}
