using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Shared text helpers for handling languages with tone marks (Igbo, Yoruba, etc.).
/// A "grapheme cluster" is a base letter plus any combining tone marks, treated as
/// a single visible unit (e.g. "á", "ọ́"). This is the unit the game spawns and matches.
/// </summary>
public static class TextUtils
{
    /// <summary>
    /// Splits text into grapheme clusters so a base letter and its combining tone
    /// marks stay together as one unit. Whitespace is preserved as its own element.
    /// </summary>
    public static List<string> SplitGraphemes(string text)
    {
        List<string> result = new List<string>();
        if (string.IsNullOrEmpty(text)) return result;

        // Compose first so things like "a" + U+0301 become "á" where possible.
        string composed = text.Normalize(System.Text.NormalizationForm.FormC);

        TextElementEnumerator e = StringInfo.GetTextElementEnumerator(composed);
        while (e.MoveNext())
        {
            result.Add((string)e.Current);
        }
        return result;
    }

    /// <summary>
    /// Splits text into grapheme clusters, skipping any whitespace clusters.
    /// </summary>
    public static List<string> SplitGraphemesNoWhitespace(string text)
    {
        List<string> all = SplitGraphemes(text);
        List<string> result = new List<string>(all.Count);
        foreach (string g in all)
        {
            if (!string.IsNullOrWhiteSpace(g))
                result.Add(g);
        }
        return result;
    }

    /// <summary>
    /// Counts visible letters (grapheme clusters) excluding whitespace.
    /// </summary>
    public static int CountGraphemes(string text)
    {
        return SplitGraphemesNoWhitespace(text).Count;
    }
}
