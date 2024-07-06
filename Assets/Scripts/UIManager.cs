using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Stats Script")]
    public Stats Stats;
    public LeaderBoardManager LeaderBoardManager; 
    
    
    [Header("UI Panels")]
    public GameObject PauseScreen;
    public GameObject PauseButton;
    public GameObject Keyboard;
    public GameObject StatScreen;
    public GameObject CorrectWordText;
    public GameObject LeaderBoardScreen;


    [Header("Text Boxes")]
    public Text TotalWinTXT;
    public Text CurrentWinTXT;
    public Text TotalGamesTXT;
    public Text TotalLossTXT;
    public Text TotalGuessTXT;

    [Header("LeaderBoard Prefabs")]
    public BoardPost BoardPostPrefabs;
    public GameObject Content;
    public List<BoardPost> BoardPosts = new List<BoardPost>();

//-------------------------------------------------------------------------
    #region Pause Methods
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
    public void UsernameBeingInput()
    {
        PauseButton.SetActive(false);
        Keyboard.SetActive(false);
    }
    
    public void UsernameSubmitted()
    {
        PauseButton.SetActive(true);
        Keyboard.SetActive(true);
    }
    #endregion

    #region  SubScreen Methods

    //Starts are Loaded whenever the stats button is clicked
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

    //LeaderBoard is loaded whenever the LeaderBoard button is clicked
    public async void DisplayLeaderBoard()
    {
        LeaderBoardScreen.SetActive(true);
        try
        {
            await LeaderBoardManager.GetScores();

            var scoresList = LeaderBoardManager.GetScoresList();
            if (scoresList == null || scoresList.Count == 0)
            {
                Debug.Log("No scores available or failed to fetch scores.");
                return;
            }

            foreach (var score in scoresList)
            {
                var boardPost = Instantiate(BoardPostPrefabs, Content.transform);
                boardPost.PlayerName.text = score.PlayerId; 
                boardPost.PlayerScore.text = score.Score.ToString();
                boardPost.PlayerRank.text = score.Rank.ToString();
                BoardPosts.Add(boardPost);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch scores: {e.Message}");
        }
    }

    public void HideLeaderBoard()
    {
        // Create a temporary list to hold the items to remove
        var postsToRemove = new List<GameObject>();

        // Collect the GameObjects to remove
        foreach (var post in BoardPosts)
        {
            postsToRemove.Add(post.gameObject);
        }

        // Remove the collected GameObjects from the BoardPosts list and destroy them
        foreach (var postGameObject in postsToRemove)
        {
            BoardPosts.RemoveAll(post => post.gameObject == postGameObject);
            Destroy(postGameObject);
        }
        LeaderBoardScreen.SetActive(false);
    }

    
    #endregion
    
    #region Game Start and End Methods

     private void UIGameStart()
    {
        CorrectWordText.SetActive(false);
        ResumeGame();

    }

    private void UIGameEnd()
    {
        PauseButton.SetActive(false);
        LeaderBoardManager.UploadCurrentWinStreak();
    }

    private void UIGameLost()
    {
        CorrectWordText.SetActive(true);
    }
       private void OnEnable()
    {
        GameManager.GameEvents.GameStart.onGameEvent += UIGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent += UIGameEnd;
        GameManager.GameEvents.GameLost.onGameEvent += UIGameLost;
    }

    private void OnDisable()
    {  
        GameManager.GameEvents.GameStart.onGameEvent -= UIGameStart;
        GameManager.GameEvents.GameEnd.onGameEvent -= UIGameEnd;
        GameManager.GameEvents.GameLost.onGameEvent -= UIGameLost;
    }
    #endregion
}

