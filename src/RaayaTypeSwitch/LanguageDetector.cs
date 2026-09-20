using System.Text.RegularExpressions;

namespace RaayaTypeSwitch;

internal sealed class LanguageDetector
{
    private static readonly HashSet<string> CommonPersian = new(StringComparer.Ordinal)
    {
        "سلام","خوب","خوبی","من","تو","ما","شما","این","اون","آن","یک","برای","با","از","به",
        "در","که","چی","چرا","بله","نه","ممنون","مرسی","لطفا","لطفاً","امروز","فردا","کار",
        "برنامه","پروژه","سایت","کتاب","متن","فارسی","انگلیسی","درست","مشکل","شروع","ادامه",
        "مثلا","مثلاً","میشه","می‌شه","میتونه","می‌تونه","میخوام","می‌خوام","میخواستم","می‌خواستم",
        "اضافه","اضافه‌کردن","اضافه کردن","حذف","تغییر","ویرایش","جدید","قبلی","بعدی"
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
        "ها","تر","کن","ور","ید","ای","یا","وا","رو","نا","هم","مث","لا","شه","خو","تو","نه"
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

        currentText = currentText.Trim();
        alternateText = alternateText.Trim();

        var currentScore = Score(currentText, currentLanguage);
        var alternateLanguage = currentLanguage == LanguageKind.Persian
            ? LanguageKind.English
            : LanguageKind.Persian;
        var alternateScore = Score(alternateText, alternateLanguage);

        var currentKnown = IsKnownWord(currentText, currentLanguage);
        var alternateKnown = IsKnownWord(alternateText, alternateLanguage);

        // Never "correct" a word that we already know to be valid in the
        // currently active language. This is a strong false-positive guard.
        if (currentKnown)
        {
            return new DetectionResult(
                false,
                currentLanguage,
                currentText,
                alternateText,
                currentScore,
                alternateScore);
        }

        var margin = alternateScore - currentScore;
        var shouldCorrect = ShouldCorrect(
            currentText,
            alternateText,
            currentLanguage,
            alternateLanguage,
            currentScore,
            alternateScore,
            margin,
            alternateKnown);

        return shouldCorrect
            ? new DetectionResult(true, alternateLanguage, currentText, alternateText, currentScore, alternateScore)
            : new DetectionResult(false, currentLanguage, currentText, alternateText, currentScore, alternateScore);
    }

    private static bool ShouldCorrect(
        string currentText,
        string alternateText,
        LanguageKind currentLanguage,
        LanguageKind alternateLanguage,
        double currentScore,
        double alternateScore,
        double margin,
        bool alternateKnown)
    {
        if (alternateText.Length < 2)
            return false;

        // A known target word is strong evidence. It still needs a margin so
        // common words in the current language are not rewritten accidentally.
        if (alternateKnown)
            return alternateScore >= 7.0 && margin >= 2.5;

        if (currentLanguage == LanguageKind.Persian &&
            alternateLanguage == LanguageKind.English)
        {
            // Unknown short English-looking strings are a major source of
            // false positives: e.g. "مثلا" mapped to "legh".
            if (currentText.Length <= 4)
                return false;

            // For an unknown English target, the current Persian token must
            // itself look suspicious, not merely have a lower score.
            if (!LooksSuspiciousPersian(currentText))
                return false;

            // Unknown English candidates need structural evidence too.
            // This prevents Persian words such as "اضافه" from being changed
            // to weak pseudo-English sequences such as "hghti".
            if (CountEnglishChunkHits(alternateText.ToLowerInvariant()) < 2)
                return false;

            return alternateScore >= 6.5 && margin >= 4.0;
        }

        if (currentLanguage == LanguageKind.English &&
            alternateLanguage == LanguageKind.Persian)
        {
            // If the current English token is structurally plausible, do not
            // convert it to an unknown Persian word. Known Persian words are
            // handled by the stronger branch above.
            if (LooksLikePlausibleEnglishWord(currentText.ToLowerInvariant()))
                return false;

            return alternateScore >= 6.5 && margin >= 4.0;
        }

        return alternateScore >= 7.0 && margin >= 4.0;
    }

    private static bool IsKnownWord(string text, LanguageKind language) =>
        language switch
        {
            LanguageKind.Persian => CommonPersian.Contains(text),
            LanguageKind.English => CommonEnglish.Contains(text),
            _ => false
        };

    private static double Score(string text, LanguageKind language)
    {
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

        var chunkHits = CountPersianChunkHits(text);
        score += chunkHits * 0.7;

        if (Regex.IsMatch(text, "[A-Za-z]"))
            score -= 3;

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
        var chunkHits = CountEnglishChunkHits(lower);
        score += chunkHits * 0.45;

        if (Regex.IsMatch(text, @"[\u0600-\u06FF]"))
            score -= 3;

        if (text.Length >= 4 && !lower.Any(IsEnglishVowel))
            score -= 3;

        if (LooksLikePlausibleEnglishWord(lower))
            score += 2.5;

        if (chunkHits >= 2)
            score += Math.Min(1.5, chunkHits * 0.25);

        return score;
    }

    private static bool LooksSuspiciousPersian(string text)
    {
        if (text.Length < 5)
            return false;

        if (!text.All(IsPersianChar))
            return false;

        // A Persian word with familiar Persian chunks is treated as plausible
        // and therefore protected from speculative conversion.
        if (CountPersianChunkHits(text) > 0)
            return false;

        // Repeated unusual letters are useful evidence for a wrong layout
        // (e.g. قششغش -> raaya), but not required.
        var repeatedAdjacent = false;
        for (var i = 1; i < text.Length; i++)
        {
            if (text[i] == text[i - 1])
            {
                repeatedAdjacent = true;
                break;
            }
        }

        return repeatedAdjacent || text.Length >= 5;
    }

    private static int CountPersianChunkHits(string text)
    {
        var hits = 0;

        foreach (var part in PersianCommonChunks)
        {
            if (text.Contains(part, StringComparison.Ordinal))
                hits++;
        }

        return hits;
    }

    private static int CountEnglishChunkHits(string text)
    {
        var hits = 0;

        foreach (var part in EnglishCommonChunks)
        {
            if (text.Contains(part, StringComparison.Ordinal))
                hits++;
        }

        return hits;
    }

    private static bool LooksLikePlausibleEnglishWord(string text)
    {
        if (text.Length < 3 || text.Length > 32)
            return false;

        if (!text.All(IsLatinLetter))
            return false;

        if (!text.Any(IsEnglishVowel))
            return false;

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
