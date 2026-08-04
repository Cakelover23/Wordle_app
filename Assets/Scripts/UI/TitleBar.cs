using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleBar : MonoBehaviour
{
    // Colors used for the title screen boxes when the "Original" theme is selected (index 0),
    // since that theme has no accent/correct/wrongSpot values of its own - these match the
    // classic Wordle green/yellow/gray so the boxes still show 3 distinct colors + hex codes.
    private static readonly Color32 ClassicCorrect = new Color32(83, 141, 78, 255);
    private static readonly Color32 ClassicWrongSpot = new Color32(181, 159, 59, 255);
    private static readonly Color32 ClassicIncorrect = new Color32(58, 58, 60, 255);

    public GameObject titleBar1;
    public GameObject titleBar2;
    public GameObject titleBar3;

    [SerializeField] private string[] barTexts = new string[3];
    [SerializeField] private Color[] barColours = new Color[3];

    // Cached once in Awake so LateUpdate (see below) doesn't need to re-run GetComponentInChildren
    // every single frame. The boxes use TextMeshPro labels (TMP_Text), not legacy UI.Text.
    private readonly TMP_Text[] _barTextComponents = new TMP_Text[3];
    private readonly Image[] _barImageComponents = new Image[3];
    private bool _themeApplied;

    private void Awake()
    {
        CacheBar(0, titleBar1);
        CacheBar(1, titleBar2);
        CacheBar(2, titleBar3);
    }

    private void CacheBar(int index, GameObject bar)
    {
        if (bar == null)
        {
            return;
        }

        _barTextComponents[index] = bar.GetComponentInChildren<TMP_Text>(true);
        _barImageComponents[index] = bar.GetComponentInChildren<Image>(true);

        // The hex code labels are always white; on light theme colors (e.g. cream/tan boxes)
        // plain white text has poor contrast. A dark drop shadow behind the text keeps it
        // readable on any background color without needing per-theme text-color logic.
        TMP_Text textComponent = _barTextComponents[index];
        if (textComponent != null)
        {
            Shadow shadow = textComponent.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = textComponent.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;
        }
    }

    public void InitializeTitleBar()
    {
        ApplyBar(0);
        ApplyBar(1);
        ApplyBar(2);
    }

    /// <summary>
    /// Colors the 3 title screen boxes from the currently selected theme's accent/correct/
    /// wrongSpot colors (the same 3 "bright" colors CategorySelectUIController cycles category
    /// cards through), and labels each box with its own hex code instead of placeholder text.
    /// Falls back to the classic Wordle green/yellow/gray when the "Original" theme is selected.
    /// </summary>
    public void ApplyTheme()
    {
        Color box1, box2, box3;
        if (ThemeService.IsOriginal)
        {
            box1 = ClassicCorrect;
            box2 = ClassicWrongSpot;
            box3 = ClassicIncorrect;
        }
        else
        {
            GameTheme theme = ThemeService.Current;
            box1 = theme.accent;
            box2 = theme.correct;
            box3 = theme.wrongSpot;
        }

        SetBar(0, ToHex(box1), box1);
        SetBar(1, ToHex(box2), box2);
        SetBar(2, ToHex(box3), box3);
        _themeApplied = true;
    }

    private static string ToHex(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    public void SetBar(int index, string text, Color colour)
    {
        if (index < 0 || index >= barTexts.Length || index >= barColours.Length)
        {
            return;
        }

        barTexts[index] = text;
        barColours[index] = colour;
        ApplyBar(index);
    }

    private void ApplyBar(int index)
    {
        TMP_Text textComponent = _barTextComponents[index];
        if (textComponent != null)
        {
            textComponent.text = barTexts[index];
        }

        Image imageComponent = _barImageComponents[index];
        if (imageComponent != null)
        {
            imageComponent.color = barColours[index];
        }
    }

    /// <summary>
    /// The CloseMain/OpenMain Animator clips animate each box's own Image.color curve directly
    /// (part of their open/close visual effect), which would silently overwrite our theme color
    /// every single frame if we only applied it once. LateUpdate always runs after the Animator's
    /// own update, so re-applying here every frame guarantees our theme color - and hex code text -
    /// wins over the placeholder color/text baked into those animation clips.
    /// </summary>
    private void LateUpdate()
    {
        if (!_themeApplied)
        {
            return;
        }

        ApplyBar(0);
        ApplyBar(1);
        ApplyBar(2);
    }
}
