using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SixBoard : Board
{
    protected override int WordLength => 6;
    protected override string DefaultSolutionsResource => "real_six_letter_words";
    protected override string DefaultValidWordsResource => "real_six_letter_words_all";
}
