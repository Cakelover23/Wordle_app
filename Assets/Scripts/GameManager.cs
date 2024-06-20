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
            public static GameEvents GuessMade = new GameEvents();
            
            public event Action onGameEvent;

            public void TriggerEvent()
            {
                onGameEvent?.Invoke();
            }
        }
    
    public void LoadFiveLetterGame()
    {
        SceneManager.LoadScene("FiveLetterWordle");
    }

    public void LoadSixLetterGame()
    {
        SceneManager.LoadScene("SixLetterWordle");
    }

}
