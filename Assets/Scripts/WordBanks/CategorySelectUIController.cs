using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Drives the UI Toolkit Category Select screen (CategorySelect.uxml/.uss). Normally you don't
/// add this manually - CategorySelectBootstrapper creates it automatically when the "Menu" scene
/// loads. Lists every WordCategory for the selected word length and, when the player taps one,
/// records the choice and loads the matching game scene directly (no GameManager reference
/// needed, so this works whether or not the current scene happens to have one).
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class CategorySelectUIController : MonoBehaviour
{
    private VisualElement _categoryList;
    private VisualElement _themeSwatches;
    private Button _tabFive;
    private Button _tabSix;
    private int _selectedWordLength = 5;

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;

        _categoryList = root.Q<VisualElement>("category-list");
        _themeSwatches = root.Q<VisualElement>("theme-swatches");
        _tabFive = root.Q<Button>("tab-five-letter");
        _tabSix = root.Q<Button>("tab-six-letter");
        Button backButton = root.Q<Button>("back-button");

        _tabFive.clicked += () => ShowWordLength(5);
        _tabSix.clicked += () => ShowWordLength(6);
        backButton.clicked += OnBackClicked;

        PopulateThemeSwatches();
        ShowWordLength(_selectedWordLength);
    }

    private void ShowWordLength(int wordLength)
    {
        _selectedWordLength = wordLength;
        _tabFive.EnableInClassList("tab-button--active", wordLength == 5);
        _tabSix.EnableInClassList("tab-button--active", wordLength == 6);
        PopulateCategoryList(wordLength);
        ApplyAccentToTabs();
    }

    private void ApplyAccentToTabs()
    {
        // Clear any previous inline override first so the button that just lost the active
        // class (or every button, when the "Original" theme is selected) falls back to its
        // normal USS color instead of keeping a stale accent color.
        _tabFive.style.backgroundColor = StyleKeyword.Null;
        _tabSix.style.backgroundColor = StyleKeyword.Null;

        if (ThemeService.IsOriginal)
        {
            return;
        }

        Button activeTab = _selectedWordLength == 6 ? _tabSix : _tabFive;
        activeTab.style.backgroundColor = ThemeService.Current.accent;
    }

    private void PopulateThemeSwatches()
    {
        _themeSwatches.Clear();

        for (int i = 0; i < ThemeService.Themes.Length; i++)
        {
            int index = i;
            GameTheme theme = ThemeService.Themes[i];
            // "Original" has no real accent color of its own (it means "don't override
            // anything"), so show the game's existing classic green for that swatch.
            Color swatchColor = index == 0 ? new Color32(83, 141, 78, 255) : theme.accent;

            var swatch = new VisualElement();
            swatch.AddToClassList("theme-swatch");
            swatch.style.backgroundColor = swatchColor;
            swatch.tooltip = theme.name;
            swatch.EnableInClassList("theme-swatch--selected", index == ThemeService.SelectedIndex);
            swatch.RegisterCallback<ClickEvent>(_ => OnThemeChosen(index));
            _themeSwatches.Add(swatch);
        }
    }

    private void OnThemeChosen(int index)
    {
        if (index == ThemeService.SelectedIndex)
        {
            return;
        }

        ThemeService.SelectedIndex = index;
        PopulateThemeSwatches();
        ApplyAccentToTabs();
    }

    private void PopulateCategoryList(int wordLength)
    {
        _categoryList.Clear();

        foreach (WordCategory category in WordCategoryDatabase.GetByWordLength(wordLength))
        {
            _categoryList.Add(BuildCategoryCard(category));
        }
    }

    private VisualElement BuildCategoryCard(WordCategory category)
    {
        var card = new VisualElement();
        card.AddToClassList("category-card");

        var name = new Label(category.displayName);
        name.AddToClassList("category-card__name");
        card.Add(name);

        var description = new Label(category.description);
        description.AddToClassList("category-card__description");
        card.Add(description);

        card.RegisterCallback<ClickEvent>(_ => OnCategoryChosen(category));
        return card;
    }

    private void OnCategoryChosen(WordCategory category)
    {
        CategorySelectionService.SelectCategory(category.id);
        SceneManager.LoadScene(category.wordLength == 6 ? "SixLetterWordle" : "FiveLetterWordle");
    }

    private void OnBackClicked()
    {
        // The Menu scene's own content (e.g. leaderboard) sits underneath this UI; hiding it
        // just reveals whatever was already there instead of reloading the scene.
        gameObject.SetActive(false);
    }
}
