using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    private int _totalWins;
    private int _currentWinStreak;
    private int _totalGamesPlayed;
    private int _totalLosses;

    private const string TotalWinsKey = "TotalWins";
    private const string CurrentWinStreakKey = "CurrentWinStreak";
    private const string TotalGamesPlayedKey = "TotalGamesPlayed";
    private const string TotalLossesKey = "TotalLosses";

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
    }

    public void AddToLosses()
    {
        _totalLosses++;
        _currentWinStreak = 0;
        _totalGamesPlayed++;
        SaveStats();
    }

    private void SaveStats()
    {
        PlayerPrefs.SetInt(TotalWinsKey, _totalWins);
        PlayerPrefs.SetInt(CurrentWinStreakKey, _currentWinStreak);
        PlayerPrefs.SetInt(TotalGamesPlayedKey, _totalGamesPlayed);
        PlayerPrefs.SetInt(TotalLossesKey, _totalLosses);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        _totalWins = PlayerPrefs.GetInt(TotalWinsKey, 0);
        _currentWinStreak = PlayerPrefs.GetInt(CurrentWinStreakKey, 0);
        _totalGamesPlayed = PlayerPrefs.GetInt(TotalGamesPlayedKey, 0);
        _totalLosses = PlayerPrefs.GetInt(TotalLossesKey, 0);
    }

    private void OnEnable()
    {
        GameManager.GameEvents.GameWon.onGameEvent += AddToWin;
        GameManager.GameEvents.GameLost.onGameEvent += AddToLosses;
    }

    private void OnDisable()
    {
        GameManager.GameEvents.GameWon.onGameEvent -= AddToWin;
        GameManager.GameEvents.GameLost.onGameEvent -= AddToLosses;
    }
}