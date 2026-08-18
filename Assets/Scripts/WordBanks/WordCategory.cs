using System;

/// <summary>
/// Plain data model describing a selectable word bank / theme.
/// Loaded from Resources/WordBanks/categories.json via <see cref="WordCategoryDatabase"/>.
/// Deliberately not a ScriptableObject so new categories can be added by editing a text file
/// instead of hand-authoring serialized assets.
/// </summary>
[Serializable]
public class WordCategory
{
    public string id;
    public string displayName;
    public string description;
    public int wordLength;
    public string solutionsPath;
    public string validWordsPath;
}
