namespace RaayaTypeSwitch.Tests;

public sealed class LanguageDetectorRegressionTests
{
    private readonly LanguageDetector _detector = new();

    [Theory]
    [InlineData("مثلا", "legh")]
    [InlineData("مثلاً", "legh")]
    [InlineData("سلام", "sghl")]
    [InlineData("میخوام", "ldoh,l")]
    [InlineData("ممنون", "lkl,k")]
    public void ValidPersianWords_AreNotConvertedToEnglish(string current, string alternate)
    {
        var result = _detector.Detect(current, alternate, LanguageKind.Persian);

        Assert.False(result.ShouldCorrect);
    }

    [Theory]
    [InlineData("deploy", "یثحمخغ")]
    [InlineData("api", "شحه")]
    [InlineData("json", "تسخد")]
    [InlineData("raaya", "قششغش")]
    [InlineData("windows", "صهدیخصس")]
    public void PlausibleEnglishWords_AreNotConvertedToPersian(string current, string alternate)
    {
        var result = _detector.Detect(current, alternate, LanguageKind.English);

        Assert.False(result.ShouldCorrect);
    }

    [Fact]
    public void WrongPersianLayout_Raaya_IsCorrectedToEnglish()
    {
        var result = _detector.Detect("قششغش", "raaya", LanguageKind.Persian);

        Assert.True(result.ShouldCorrect);
        Assert.Equal(LanguageKind.English, result.TargetLanguage);
        Assert.Equal("raaya", result.AlternateText);
    }

    [Fact]
    public void WrongPersianLayout_Deploy_IsCorrectedToEnglish()
    {
        var result = _detector.Detect("یثحمخغ", "deploy", LanguageKind.Persian);

        Assert.True(result.ShouldCorrect);
        Assert.Equal(LanguageKind.English, result.TargetLanguage);
        Assert.Equal("deploy", result.AlternateText);
    }

    [Fact]
    public void WrongEnglishLayout_Salam_IsCorrectedToPersian()
    {
        var result = _detector.Detect("sghl", "سلام", LanguageKind.English);

        Assert.True(result.ShouldCorrect);
        Assert.Equal(LanguageKind.Persian, result.TargetLanguage);
        Assert.Equal("سلام", result.AlternateText);
    }

    [Theory]
    [InlineData("a", "ش", LanguageKind.English)]
    [InlineData("ب", "f", LanguageKind.Persian)]
    public void SingleCharacterInput_IsNeverAutoCorrected(
        string current,
        string alternate,
        LanguageKind language)
    {
        var result = _detector.Detect(current, alternate, language);

        Assert.False(result.ShouldCorrect);
    }
}
