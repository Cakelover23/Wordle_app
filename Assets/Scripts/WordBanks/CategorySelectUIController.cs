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
    private VisualElement _root;
    private Button _tabFive;
    private Button _tabSix;
    private int _selectedWordLength = 5;

    private VisualElement _usernamePanel;
    private TextField _usernameField;

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;

        _root = root.Q<VisualElement>("root");
        _categoryList = root.Q<VisualElement>("category-list");
        _themeSwatches = root.Q<VisualElement>("theme-swatches");
        _tabFive = root.Q<Button>("tab-five-letter");
        _tabSix = root.Q<Button>("tab-six-letter");
        Button backButton = root.Q<Button>("back-button");

        _usernamePanel = root.Q<VisualElement>("username-panel");
        _usernameField = root.Q<TextField>("username-field");
        Button usernameSubmitButton = root.Q<Button>("username-submit-button");

        // Temporarily disabled while the 6-letter game scene is being fixed up - remove this
        // line to bring the "6 Letters" tab back once that scene is confirmed working again.
        _tabSix.style.display = DisplayStyle.None;

        _tabFive.clicked += () => ShowWordLength(5);
        _tabSix.clicked += () => ShowWordLength(6);
        backButton.clicked += OnBackClicked;
        usernameSubmitButton.clicked += OnUsernameSubmitted;

        PopulateThemeSwatches();
        ShowWordLength(_selectedWordLength);
        ThemeService.ApplyAccent(root, "modal-button");
        ApplyThemeBackground();

        // Very first launch only: the player has never chosen a username before, so ask for one
        // here on the Menu screen and never again - UsernameService persists it forever, and the
        // in-game Stats/LeaderBoardManager flow reads the same value.
        if (!UsernameService.HasUsername)
        {
            _usernamePanel.RemoveFromClassList("hidden");
        }

        // Stay hidden behind the title screen until its open animation finishes -
        // TitleScreenController raises MainMenuRevealGate.Revealed once that happens. If no
        // title screen exists in the scene at all, nothing will ever call Reveal() and this
        // screen simply won't appear - that's an intentional trade-off for keeping the two
        // scripts fully decoupled.
        _root.style.display = DisplayStyle.None;
        MainMenuRevealGate.Revealed += OnRevealed;
    }

    private void OnDisable()
    {
        MainMenuRevealGate.Revealed -= OnRevealed;
    }

    private void OnRevealed()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    private void OnUsernameSubmitted()
    {
        if (string.IsNullOrWhiteSpace(_usernameField.value))
        {
            return;
        }

        UsernameService.Set(_usernameField.value);
        _usernamePanel.AddToClassList("hidden");
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
        ApplyThemeBackground();
        PopulateCategoryList(_selectedWordLength);
    }

    /// <summary>
    /// Tints the whole Menu screen's root background with the current theme's darkest tile color
    /// (mirrors what BoardUIToolkitController does for the Game scene), so both screens share the
    /// same dark poster-style backdrop. No-ops on "Original" so nothing changes by default.
    /// </summary>
    private void ApplyThemeBackground()
    {
        _root.style.backgroundColor = ThemeService.IsOriginal
            ? new StyleColor(StyleKeyword.Null)
            : new StyleColor(ThemeService.Current.tileEmpty);
    }

    private void PopulateCategoryList(int wordLength)
    {
        _categoryList.Clear();

        int index = 0;
        foreach (WordCategory category in WordCategoryDatabase.GetByWordLength(wordLength))
        {
            _categoryList.Add(BuildCategoryCard(category, index));
            index++;
        }
    }

    private VisualElement BuildCategoryCard(WordCategory category, int index)
    {
        var card = new VisualElement();
        card.AddToClassList("category-card");

        var name = new Label(category.displayName);
        name.AddToClassList("category-card__name");
        card.Add(name);

        var description = new Label(category.description);
        description.AddToClassList("category-card__description");
        card.Add(description);

        ApplyCardTheme(card, name, description, index);

        card.RegisterCallback<ClickEvent>(_ => OnCategoryChosen(category));
        return card;
    }

    /// <summary>
    /// Cycles category card backgrounds through the theme's 3 "bright" colors (accent, correct,
    /// wrongSpot) by index, mimicking the multi-color-block look of the retro poster references
    /// instead of painting every card the same flat color. Text color is picked for readability
    /// using the same luminance formula KeyboardUIToolkitController uses for its keys. No-ops on
    /// "Original" so the default flat gray cards are left untouched.
    /// </summary>
    private static void ApplyCardTheme(VisualElement card, Label name, Label description, int index)
    {
        if (ThemeService.IsOriginal)
        {
            card.style.backgroundColor = StyleKeyword.Null;
            name.style.color = StyleKeyword.Null;
            description.style.color = StyleKeyword.Null;
            return;
        }

        GameTheme theme = ThemeService.Current;
        Color[] palette = { theme.accent, theme.correct, theme.wrongSpot };
        Color background = palette[index % palette.Length];
        card.style.backgroundColor = background;

        float luminance = background.r * 0.299f + background.g * 0.587f + background.b * 0.114f;
        Color textColor = luminance > 0.6f ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
        name.style.color = textColor;
        description.style.color = textColor;
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
