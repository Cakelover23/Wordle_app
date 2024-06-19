using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SixBoard : Board
{
    protected override void LoadData()
    {
        TextAsset textFile = Resources.Load("real_six_letter_words") as TextAsset;
        solutions = textFile.text.Split('\n');

        textFile = Resources.Load("real_six_letter_words_all") as TextAsset;
        validWords = new HashSet<string>(textFile.text.Split('\n'));
    }
}
