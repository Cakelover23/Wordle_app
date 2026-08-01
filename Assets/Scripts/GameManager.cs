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

    /// <summary>Records the chosen word bank, then loads the matching game scene for it.</summary>
    public void SelectCategoryAndPlay(string categoryId)
    {
        WordCategory category = WordCategoryDatabase.GetById(categoryId);
        if (category == null)
        {
            Debug.LogWarning($"GameManager: unknown category id '{categoryId}'.");
            return;
        }

        CategorySelectionService.SelectCategory(categoryId);

        if (category.wordLength == 6)
        {
            LoadSixLetterGame();
        }
        else
        {
            LoadFiveLetterGame();
        }
    }

}
