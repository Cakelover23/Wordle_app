using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Core;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;


public class LeaderBoardManager : MonoBehaviour
{ 
    public class ScoreEntry
{
    public string PlayerId { get; set; }
    public double Score { get; set; }
    public int Rank { get; set; }
}
    [Header("Stats Script")]
    public Stats stats;
  
    
    private string _userId;
    const string LeaderboardId = "CurrentStreak";
    const string SyncedUsernameKey = "SyncedLeaderboardUsername";

    string VersionId { get; set; }
    int Offset { get; set; }
    int Limit { get; set; }
    int RangeLimit { get; set; }
    List<string> FriendIds { get; set; }

     private List<ScoreEntry> scoresList = new List<ScoreEntry>(); // Step 1: Define the list

    async void Awake()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("Unity Services Initialized Successfully.");

        await SignInAnonymously();

        if (stats == null)
        {
            Debug.LogWarning("LeaderBoardManager: 'stats' reference is not assigned in the Inspector - skipping player name sync. Assign the Stats component on this GameObject (or scene) to fix.");
            return;
        }

        await SyncPlayerNameIfNeeded();
    }

    /// <summary>
    /// Only calls UpdatePlayerNameAsync when the local username has actually changed since the
    /// last successful sync (tracked via PlayerPrefs). Without this guard, every game scene load
    /// re-sends the same name to Unity Authentication, which is unnecessary and was the source of
    /// intermittent "name already set"/conflict errors the player saw every so often.
    /// </summary>
    private async Task SyncPlayerNameIfNeeded()
    {
        _userId = stats._username;
        if (string.IsNullOrWhiteSpace(_userId))
        {
            return;
        }

        if (PlayerPrefs.GetString(SyncedUsernameKey) == _userId)
        {
            return; // Already synced this exact name - nothing to do.
        }

        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(_userId);
            PlayerPrefs.SetString(SyncedUsernameKey, _userId);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"LeaderBoardManager: failed to sync player name '{_userId}': {e.Message}");
        }
    }


    async Task SignInAnonymously()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            // Already signed in from a previous scene load this session - calling
            // SignInAnonymouslyAsync() again is unnecessary and can throw.
            return;
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
        };

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"LeaderBoardManager: sign-in failed: {e.Message}");
        }
    }


   public async Task GetScores()
    {
        scoresList.Clear(); 
        var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId);
        Debug.Log(JsonConvert.SerializeObject(scoresResponse));
        foreach (var leaderboardEntry in scoresResponse.Results)
        {
            scoresList.Add(new ScoreEntry
            {
                PlayerId = leaderboardEntry.PlayerName,
                Score = leaderboardEntry.Score,
                Rank = leaderboardEntry.Rank +1
            });
            Debug.Log(leaderboardEntry.Rank.ToString());

            Debug.Log(leaderboardEntry.PlayerName);

            Debug.Log(leaderboardEntry.Score.ToString());
        }
    }

    // Step 3: Provide access to the scores list
    public List<ScoreEntry> GetScoresList()
    {
        return scoresList;
    }
    
   public async void UploadCurrentWinStreak()
    {
        if (stats != null)
        {
            // Assuming AddPlayerScoreAsync now also accepts a username parameter
            var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, stats._currentWinStreak);
            Debug.Log(JsonConvert.SerializeObject(scoreResponse));
        }
        else
        {
            Debug.LogError("Stats reference is not set in LeaderBoardManager.");
        }
    }

}