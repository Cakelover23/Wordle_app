using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Loads and caches the list of available <see cref="WordCategory"/> word banks from
/// Resources/WordBanks/categories.json. Add a new category by adding two .txt files
/// (solutions + valid words) under Assets/Resources/WordBanks/ and one entry to that JSON file -
/// no code or scene changes required.
/// </summary>
public static class WordCategoryDatabase
{
    private const string ManifestResourcePath = "WordBanks/categories";

    [Serializable]
    private class Manifest
    {
        public List<WordCategory> categories;
    }

    private static List<WordCategory> _categories;

    public static IReadOnlyList<WordCategory> All
    {
        get
        {
            EnsureLoaded();
            return _categories;
        }
    }

    public static IEnumerable<WordCategory> GetByWordLength(int wordLength)
    {
        EnsureLoaded();
        return _categories.Where(c => c.wordLength == wordLength);
    }

    public static WordCategory GetById(string id)
    {
        EnsureLoaded();
        return _categories.FirstOrDefault(c => c.id == id);
    }

    private static void EnsureLoaded()
    {
        if (_categories != null)
        {
            return;
        }

        TextAsset manifestAsset = Resources.Load<TextAsset>(ManifestResourcePath);
        if (manifestAsset == null)
        {
            Debug.LogError($"WordCategoryDatabase: could not find manifest at Resources/{ManifestResourcePath}.json");
            _categories = new List<WordCategory>();
            return;
        }

        try
        {
            Manifest manifest = JsonUtility.FromJson<Manifest>(manifestAsset.text);
            _categories = manifest?.categories ?? new List<WordCategory>();
        }
        catch (Exception e)
        {
            Debug.LogError($"WordCategoryDatabase: failed to parse categories.json - {e.Message}");
            _categories = new List<WordCategory>();
        }
    }
}
