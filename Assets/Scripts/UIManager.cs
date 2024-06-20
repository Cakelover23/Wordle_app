using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Stats Script")]
    public Stats Stats;
    
    
    [Header("UI Panels")]
    public GameObject PauseScreen;
    public GameObject PauseButton;
    public GameObject Keyboard;
    public GameObject StatScreen;


    [Header("Text Boxes")]
    public Text TotalWinTXT;
    public Text CurrentWinTXT;
    public Text TotalGamesTXT;
    public Text TotalLossTXT;
    public Text TotalGuessTXT;
//-------------------------------------------------------------------------
    public void DisplayStats()
    {
        StatScreen.SetActive(true);
        TotalGamesTXT.text = Stats._totalGamesPlayed.ToString();
        TotalWinTXT.text = Stats._totalWins.ToString();
        TotalLossTXT.text = Stats._totalLosses.ToString();
        CurrentWinTXT.text = Stats._currentWinStreak.ToString();
        TotalGuessTXT.text = Stats._totalGuesses.ToString(); 
    }
    public void HideStats()
    {
        StatScreen.SetActive(false);
    }
    
    public void PauseGame()
    {
        PauseScreen.SetActive(true);
        PauseButton.SetActive(false);
        Keyboard.SetActive(false);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        PauseScreen.SetActive(false);
        PauseButton.SetActive(true);
        Keyboard.SetActive(true);
        Time.timeScale = 1;
    }

     private void OnGameStart()
    {
        PauseButton.SetActive(true);
    }

    private void OnGameEnd()
    {
        PauseButton.SetActive(false);
    }

       private void OnEnable()
    {
        GameManager.GameEvents.GameStart.onGameEvent += OnGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent += OnGameEnd;
    }

    private void OnDisable()
    {  
        GameManager.GameEvents.GameStart.onGameEvent -= OnGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent -= OnGameEnd;
    }
}

