using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    
    private const string TotalWinsKey = "TotalWins";
    private const string CurrentWinStreakKey = "CurrentWinStreak";
    private const string TotalGamesPlayedKey = "TotalGamesPlayed";
    private const string TotalLossesKey = "TotalLosses";
    private const string TotalGuessKey = "TotalGuesses";

    private void Start()
    {
        LoadStats();
    }

    public void AddToWin()
    {
        _totalWins++;
        _currentWinStreak++;
        _totalGamesPlayed++;
        SaveStats();
        Debug.Log("Adding to stats");
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