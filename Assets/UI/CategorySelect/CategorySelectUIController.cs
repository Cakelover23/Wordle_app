using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Drives the UI Toolkit Category Select screen (CategorySelect.uxml/.uss). Attach to a
/// GameObject that also has a UIDocument component pointing at that UXML, with a PanelSettings
/// asset assigned (Create > UI Toolkit > Panel Settings Asset in the Unity 6 Editor).
/// Lists every WordCategory for the selected word length and starts the matching game scene
/// via GameManager.SelectCategoryAndPlay when the player taps one.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class CategorySelectUIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private string menuSceneName = "Menu";

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
        if (gameManager == null)
        {
            Debug.LogError("CategorySelectUIController: GameManager reference is not assigned.");
            return;
        }

        gameManager.SelectCategoryAndPlay(category.id);
    }

    private void OnBackClicked()
    {
        if (gameManager != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
        }
    }
}
