using System.Linq;
using UnityEngine;

/// <summary>
/// Tracks which <see cref="WordCategory"/> the player has chosen for each word length and
/// remembers the choice between sessions via PlayerPrefs. Board/SixBoard consult this when
/// loading their solution/valid-word lists.
/// </summary>
public static class CategorySelectionService
{
    private const string PrefKeyPrefix = "SelectedCategoryId_";

    public static void SelectCategory(string categoryId)
    {
        WordCategory category = WordCategoryDatabase.GetById(categoryId);
        if (category == null)
        {
            Debug.LogWarning($"CategorySelectionService: unknown category id '{categoryId}'.");
            return;
        }

        PlayerPrefs.SetString(PrefKeyPrefix + category.wordLength, categoryId);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the selected category for the given word length, or the first available
    /// category for that length if none has been chosen yet (or the saved choice is gone).
    /// Returns null if no categories of that length are registered at all.
    /// </summary>
    public static WordCategory GetSelectedCategory(int wordLength)
    {
        string savedId = PlayerPrefs.GetString(PrefKeyPrefix + wordLength, null);
        if (!string.IsNullOrEmpty(savedId))
        {
            WordCategory saved = WordCategoryDatabase.GetById(savedId);
            if (saved != null && saved.wordLength == wordLength)
            {
                return saved;
            }
        }

        return WordCategoryDatabase.GetByWordLength(wordLength).FirstOrDefault();
    }
}
