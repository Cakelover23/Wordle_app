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
    private Button _tabFive;
    private Button _tabSix;
    private int _selectedWordLength = 5;

    private void OnEnable()
    {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;

        _categoryList = root.Q<VisualElement>("category-list");
        _tabFive = root.Q<Button>("tab-five-letter");
        _tabSix = root.Q<Button>("tab-six-letter");
        Button backButton = root.Q<Button>("back-button");

        _tabFive.clicked += () => ShowWordLength(5);
        _tabSix.clicked += () => ShowWordLength(6);
        backButton.clicked += OnBackClicked;

        ShowWordLength(_selectedWordLength);
    }

    private void ShowWordLength(int wordLength)
    {
        _selectedWordLength = wordLength;
        _tabFive.EnableInClassList("tab-button--active", wordLength == 5);
        _tabSix.EnableInClassList("tab-button--active", wordLength == 6);
        PopulateCategoryList(wordLength);
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
