using System.Text.RegularExpressions;

namespace RaayaTypeSwitch;

internal sealed class LanguageDetector
{
    private static readonly HashSet<string> CommonPersian = new(StringComparer.Ordinal)
    {
        "سلام","خوب","خوبی","من","تو","ما","شما","این","اون","آن","یک","برای","با","از","به",
        "در","که","چی","چرا","بله","نه","ممنون","مرسی","لطفا","لطفاً","امروز","فردا","کار",
        "برنامه","پروژه","سایت","کتاب","متن","فارسی","انگلیسی","درست","مشکل","شروع","ادامه"
    };

    private static readonly HashSet<string> CommonEnglish = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello","hi","thanks","thank","you","yes","no","the","this","that","is","are","i","we",
        "they","for","from","to","with","and","or","project","app","site","book","text","english",
        "persian","start","continue","work","code","build","test","login","admin","api","windows"
    };

    private static readonly string[] PersianCommonChunks =
    {
        "سلام","است","بود","های","می","ان","ار","را","به","در","از","بر","ام","ات","ند","ری"
    };

    private static readonly string[] EnglishCommonChunks =
    {
        "th","he","in","er","an","re","on","at","en","nd","ing","ion","to","of"
    };

    public DetectionResult Detect(string currentText, string alternateText, LanguageKind currentLanguage)
    {
        if (string.IsNullOrWhiteSpace(currentText) || string.IsNullOrWhiteSpace(alternateText))
            return DetectionResult.None;

        var currentScore = Score(currentText, currentLanguage);
        var alternateLanguage = currentLanguage == LanguageKind.Persian
            ? LanguageKind.English
            : LanguageKind.Persian;
        var alternateScore = Score(alternateText, alternateLanguage);

        var margin = alternateScore - currentScore;
        var shouldCorrect =
            alternateScore >= 6.0 &&
            margin >= 4.0 &&
            alternateText.Length >= 2;

        return shouldCorrect
            ? new DetectionResult(true, alternateLanguage, currentText, alternateText, currentScore, alternateScore)
            : new DetectionResult(false, currentLanguage, currentText, alternateText, currentScore, alternateScore);
    }

    private static double Score(string text, LanguageKind language)
    {
        text = text.Trim();
        if (text.Length == 0)
            return 0;

        double score = 0;

        if (language == LanguageKind.Persian)
        {
            var persianChars = text.Count(IsPersianChar);
            score += 4.0 * persianChars / Math.Max(1, text.Length);

            if (CommonPersian.Contains(text))
                score += 8;

            foreach (var part in PersianCommonChunks)
                if (text.Contains(part, StringComparison.Ordinal))
                    score += 0.7;

            if (Regex.IsMatch(text, "[A-Za-z]"))
                score -= 3;
        }
        else if (language == LanguageKind.English)
        {
            var latinChars = text.Count(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
            score += 4.0 * latinChars / Math.Max(1, text.Length);

            if (CommonEnglish.Contains(text))
                score += 8;

            var lower = text.ToLowerInvariant();
            foreach (var part in EnglishCommonChunks)
                if (lower.Contains(part, StringComparison.Ordinal))
                    score += 0.6;

            if (Regex.IsMatch(text, @"[\u0600-\u06FF]"))
                score -= 3;

            if (text.Length >= 4 && !lower.Any(c => "aeiouy".Contains(c)))
                score -= 2.5;
        }

        return score;
    }

    private static bool IsPersianChar(char c) => c is >= '\u0600' and <= '\u06FF';
}

internal readonly record struct DetectionResult(
    bool ShouldCorrect,
    LanguageKind TargetLanguage,
    string CurrentText,
    string AlternateText,
    double CurrentScore,
    double AlternateScore)
{
    public static DetectionResult None =>
        new(false, LanguageKind.Other, string.Empty, string.Empty, 0, 0);
}
