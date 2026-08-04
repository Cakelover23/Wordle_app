using UnityEngine;

/// <summary>
/// Single source of truth for the player's persisted username. Both the Menu scene's first-launch
/// prompt (CategorySelectUIController) and the in-game Stats/LeaderBoardManager flow read/write
/// through here, so there's exactly one PlayerPrefs key and one place that decides whether a
/// username has already been chosen.
/// </summary>
public static class UsernameService
{
    private const string UsernameKey = "Username";

    /// <summary>Whether the player has ever chosen a username (persists across app restarts).</summary>
    public static bool HasUsername => !string.IsNullOrEmpty(PlayerPrefs.GetString(UsernameKey));

    /// <summary>The currently saved username, or an empty string if none has been set yet.</summary>
    public static string Current => PlayerPrefs.GetString(UsernameKey);

    /// <summary>Trims and persists the given username. No-ops on blank input.</summary>
    public static void Set(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        PlayerPrefs.SetString(UsernameKey, username.Trim());
        PlayerPrefs.Save();
    }
}
