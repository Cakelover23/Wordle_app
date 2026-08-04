using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Stats : MonoBehaviour
{
    //[HideInInspector]
    public int _totalWins;
    //[HideInInspector]
    public int _currentWinStreak;
    //[HideInInspector]
    public int _totalGamesPlayed;
    //[HideInInspector]
    public int _totalLosses;
    //[HideInInspector]
    public int _totalGuesses;
    public string _username;
    public GameObject UsernameInput;
    public UIManager UIManager;
    
    
    private const string UsernameKey = "Username";
    private const string TotalWinsKey = "TotalWins";
    private const string CurrentWinStreakKey = "CurrentWinStreak";
    private const string TotalGamesPlayedKey = "TotalGamesPlayed";
    private const string TotalLossesKey = "TotalLosses";
    private const string TotalGuessKey = "TotalGuesses";

    private void Awake()
    {
        // Load the persisted username as early as possible (Awake runs before any other
        // component's Start/OnEnable reads Stats.HasUsername), so nothing can race ahead of
        // PlayerPrefs and mistakenly re-show the username prompt.
        _username = PlayerPrefs.GetString(UsernameKey);
    }

    private void Start()
    {
        LoadStats();
        SetupUsername();
    }

    public void AddToWin()
    {
        _totalWins++;
        _currentWinStreak++;
        _totalGamesPlayed++;
        SaveStats();
        Debug.Log("Adding to stats");
    }
    private void SetupUsername()
    {
       if (string.IsNullOrEmpty(_username))
       {
            UsernameInput.SetActive(true);
            UIManager.UsernameBeingInput();

       }
        
        Debug.Log("Username is: " + _username);
    }
    public void SubmitUsername()
    {
        _username = UsernameInput.GetComponentInChildren<InputField>().text;
        PlayerPrefs.SetString(UsernameKey, _username);
        PlayerPrefs.Save();
        UsernameInput.SetActive(false);
        UIManager.UsernameSubmitted();
    }

    /// <summary>Whether a username has already been chosen (loaded from PlayerPrefs or submitted this run).</summary>
    public bool HasUsername => !string.IsNullOrEmpty(_username);

    /// <summary>
    /// UI-framework-agnostic username submission, for callers (e.g. a UI Toolkit HUD) that
    /// already have the entered text and don't use the legacy uGUI InputField/UsernameInput panel.
    /// </summary>
    public void SubmitUsernameText(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        _username = username.Trim();
        PlayerPrefs.SetString(UsernameKey, _username);
        PlayerPrefs.Save();
    }

    public void AddToLosses()
    {
        _totalLosses++;
        _currentWinStreak = 0;
        _totalGamesPlayed++;
        SaveStats();
    }

    public void AddtoGuesses()
    {
        _totalGuesses++;
        SaveStats();
    }

  
    private void SaveStats()
    {
        PlayerPrefs.SetInt(TotalWinsKey, _totalWins);
        PlayerPrefs.SetInt(CurrentWinStreakKey, _currentWinStreak);
        PlayerPrefs.SetInt(TotalGamesPlayedKey, _totalGamesPlayed);
        PlayerPrefs.SetInt(TotalLossesKey, _totalLosses);
        PlayerPrefs.SetInt(TotalGuessKey, _totalGuesses);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        _totalWins = PlayerPrefs.GetInt(TotalWinsKey, 0);
        _currentWinStreak = PlayerPrefs.GetInt(CurrentWinStreakKey, 0);
        _totalGamesPlayed = PlayerPrefs.GetInt(TotalGamesPlayedKey, 0);
        _totalLosses = PlayerPrefs.GetInt(TotalLossesKey, 0);
        _totalGuesses = PlayerPrefs.GetInt(TotalGuessKey, 0);
    }

    private void OnEnable()
    {
        GameManager.GameEvents.GameWon.onGameEvent += AddToWin;
        GameManager.GameEvents.GameLost.onGameEvent += AddToLosses;
        GameManager.GameEvents.GuessMade.onGameEvent += AddtoGuesses;
    }

    private void OnDisable()
    {
        GameManager.GameEvents.GameWon.onGameEvent -= AddToWin;
        GameManager.GameEvents.GameLost.onGameEvent -= AddToLosses;
        GameManager.GameEvents.GuessMade.onGameEvent -= AddtoGuesses;
    }
}