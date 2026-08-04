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

    public void InitializeTitleBar()
    {
        ApplyBar(titleBar1, barTexts[0], barColours[0]);
        ApplyBar(titleBar2, barTexts[1], barColours[1]);
        ApplyBar(titleBar3, barTexts[2], barColours[2]);
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

        switch (index)
        {
            case 0:
                ApplyBar(titleBar1, barTexts[0], barColours[0]);
                break;
            case 1:
                ApplyBar(titleBar2, barTexts[1], barColours[1]);
                break;
            case 2:
                ApplyBar(titleBar3, barTexts[2], barColours[2]);
                break;
        }
    }

    private void ApplyBar(GameObject bar, string text, Color colour)
    {
        if (bar == null)
        {
            return;
        }

        var textComponent = bar.GetComponentInChildren<Text>(true);
        if (textComponent != null)
        {
            textComponent.text = text;
        }

        var imageComponent = bar.GetComponentInChildren<Image>(true);
        if (imageComponent != null)
        {
            imageComponent.color = colour;
        }
    }
}
