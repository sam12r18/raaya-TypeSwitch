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
        "persian","start","continue","work","code","build","test","login","admin","api","windows",
        "deploy","deployment","server","client","branch","commit","push","pull","main","develop"
    };

    private static readonly string[] PersianCommonChunks =
    {
        "سلام","است","بود","های","می","ان","ار","را","به","در","از","بر","ام","ات","ند","ری",
        "ها","تر","کن","ور","ید","ای","یا","وا","رو","نا","هم"
    };

    private static readonly string[] EnglishCommonChunks =
    {
        "th","he","in","er","an","re","on","at","en","nd","ing","ion","to","of",
        "de","ra","ay","pl","lo","oy","st","ou","ea","it","is","or","te","se","ar"
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

        // Unknown but structurally plausible words (brands, technical terms, names)
        // must still be correctable without requiring an exhaustive dictionary.
        var shouldCorrect =
            alternateScore >= 6.0 &&
            margin >= 3.5 &&
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

        return language switch
        {
            LanguageKind.Persian => ScorePersian(text),
            LanguageKind.English => ScoreEnglish(text),
            _ => 0
        };
    }

    private static double ScorePersian(string text)
    {
        double score = 0;

        var persianChars = text.Count(IsPersianChar);
        score += 4.0 * persianChars / Math.Max(1, text.Length);

        if (CommonPersian.Contains(text))
            score += 8;

        var chunkHits = 0;
        foreach (var part in PersianCommonChunks)
        {
            if (text.Contains(part, StringComparison.Ordinal))
            {
                score += 0.7;
                chunkHits++;
            }
        }

        if (Regex.IsMatch(text, "[A-Za-z]"))
            score -= 3;

        // Long Persian-looking strings with no familiar Persian structure are
        // often the result of typing an English word while Persian is active.
        if (text.Length >= 4 &&
            persianChars == text.Length &&
            !CommonPersian.Contains(text) &&
            chunkHits == 0)
        {
            score -= 1.5;
        }

        return score;
    }

    private static double ScoreEnglish(string text)
    {
        double score = 0;

        var latinChars = text.Count(IsLatinLetter);
        score += 4.0 * latinChars / Math.Max(1, text.Length);

        if (CommonEnglish.Contains(text))
            score += 8;

        var lower = text.ToLowerInvariant();
        var chunkHits = 0;

        foreach (var part in EnglishCommonChunks)
        {
            if (lower.Contains(part, StringComparison.Ordinal))
            {
                score += 0.45;
                chunkHits++;
            }
        }

        if (Regex.IsMatch(text, @"[\u0600-\u06FF]"))
            score -= 3;

        if (text.Length >= 4 && !lower.Any(IsEnglishVowel))
            score -= 3;

        if (LooksLikePlausibleEnglishWord(lower))
            score += 2.5;

        // A few matching English chunks provide additional confidence for
        // unknown words without turning the dictionary into a hard dependency.
        if (chunkHits >= 2)
            score += Math.Min(1.5, chunkHits * 0.25);

        return score;
    }

    private static bool LooksLikePlausibleEnglishWord(string text)
    {
        if (text.Length < 3 || text.Length > 32)
            return false;

        if (!text.All(IsLatinLetter))
            return false;

        if (!text.Any(IsEnglishVowel))
            return false;

        // Reject highly implausible consonant runs. Y is treated as a vowel
        // for this lightweight heuristic because it is common in names/brands.
        var longestConsonantRun = 0;
        var currentRun = 0;

        foreach (var c in text)
        {
            if (IsEnglishVowel(c))
            {
                currentRun = 0;
            }
            else
            {
                currentRun++;
                longestConsonantRun = Math.Max(longestConsonantRun, currentRun);
            }
        }

        return longestConsonantRun <= 4;
    }

    private static bool IsLatinLetter(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsEnglishVowel(char c) =>
        c is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';

    private static bool IsPersianChar(char c) =>
        c is >= '\u0600' and <= '\u06FF';
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
