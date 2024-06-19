using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public class GameEvents
        {
            public static GameEvents GameStart = new GameEvents();
            public static GameEvents GameEnd = new GameEvents();
            public static GameEvents GameLost = new GameEvents();
            public static GameEvents GameWon = new GameEvents();
            
            public event Action onGameEvent;

            public void TriggerEvent()
            {
                onGameEvent?.Invoke();
            }
        }
    public GameObject PauseScreen;
    public GameObject PauseButton;
    public GameObject Keyboard;

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
    
    public void LoadFiveLetterGame()
    {
        SceneManager.LoadScene("FiveLetterWordle");
    }

    public void LoadSixLetterGame()
    {
        SceneManager.LoadScene("SixLetterWordle");
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
        GameEvents.GameStart.onGameEvent += OnGameStart;
        GameEvents.GameEnd.onGameEvent += OnGameEnd;
    }
    private void OnDisable()
    {  
        GameEvents.GameStart.onGameEvent -= OnGameStart;
        GameEvents.GameEnd.onGameEvent -= OnGameEnd;
    }
}
