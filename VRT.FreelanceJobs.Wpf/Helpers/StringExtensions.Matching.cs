namespace VRT.FreelanceJobs.Wpf.Helpers;
public static partial class StringExtensions
{
    public static bool MatchesAll(this string? text, params string[] patternParts)
    {
        if (text == null || text.Length == 0)
        {
            return patternParts == null || patternParts.Length == 0;
        }
        return patternParts.All(pattern => text.Contains(pattern, StringComparison.InvariantCultureIgnoreCase));
    }
    public static bool MatchesAny(this string text, params string[] patternParts)
    {
        if (text == null || string.IsNullOrWhiteSpace(text))
        {
            return patternParts == null || patternParts.Length == 0 || patternParts.All(string.IsNullOrWhiteSpace);
        }
        return patternParts.Any(pattern => text.Contains(pattern, StringComparison.InvariantCultureIgnoreCase));
    }
    public static bool MatchesAll(this string?[] texts, string[] patterns)
    {
        if (texts == null || texts.Length == 0)
        {
            return patterns == null || patterns.Length == 0;
        }
        return string.Join(" ", texts).MatchesAll(patterns);
    }

    public static bool EqualsIgnoreCase(this string text, string text2)
    {
        if (text == null || text.Length == 0)
        {
            return text2 == null || text2.Length == 0;
        }
        return text.Equals(text2, StringComparison.InvariantCultureIgnoreCase);
    }
}