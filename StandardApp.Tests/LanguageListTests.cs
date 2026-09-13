using Arc.WinUI;

namespace StandardApp.Tests;

/// <summary>
/// Tests language registration and immutable lookup snapshots.
/// </summary>
public class LanguageListTests
{
    [Fact]
    public void AddingAfterReadingSnapshotsUpdatesBothLookups()
    {
        var languages = LanguageList.LanguageToIdentifier;
        var identifiers = LanguageList.IdentifierToLanguage;
        var language = Guid.NewGuid().ToString();
        var identifier = "Language." + language;

        LanguageList.Add(language, identifier);

        Assert.True(LanguageList.TryGetIdentifier(language, out var actualIdentifier));
        Assert.Equal(identifier, actualIdentifier);
        Assert.True(LanguageList.TryGetLanguage(identifier, out var actualLanguage));
        Assert.Equal(language, actualLanguage);
        Assert.False(languages.ContainsKey(language));
        Assert.False(identifiers.ContainsKey(identifier));
    }

    [Fact]
    public void DuplicateIdentifierDoesNotPartiallyRegisterLanguage()
    {
        var first = Guid.NewGuid().ToString();
        var second = Guid.NewGuid().ToString();
        LanguageList.Add(first, first);
        Assert.Throws<ArgumentException>(() => LanguageList.Add(second, first));
        Assert.False(LanguageList.TryGetIdentifier(second, out _));
        LanguageList.Add(second, second);
        Assert.Equal(second, LanguageList.LanguageToIdentifier[second]);
    }

    [Fact]
    public void DuplicateLanguageDoesNotRegisterIdentifier()
    {
        var language = Guid.NewGuid().ToString();
        var identifier = Guid.NewGuid().ToString();
        LanguageList.Add(language, language);
        Assert.Throws<ArgumentException>(() => LanguageList.Add(language, identifier));
        Assert.False(LanguageList.TryGetLanguage(identifier, out _));
    }

    [Theory]
    [InlineData(null, "identifier")]
    [InlineData("language", null)]
    public void NullArgumentsLeaveRegistryUnchanged(string? language, string? identifier)
    {
        var before = LanguageList.LanguageToIdentifier;
        Assert.Throws<ArgumentNullException>(() => LanguageList.Add(language!, identifier!));
        Assert.Same(before, LanguageList.LanguageToIdentifier);
    }

    [Fact]
    public void ConcurrentRegistrationAndLookupsRemainConsistent()
    {
        var prefix = Guid.NewGuid().ToString();
        Parallel.For(0, 100, i =>
        {
            var key = prefix + i;
            LanguageList.Add(key, key);
            Assert.Equal(key, LanguageList.LanguageToIdentifier[key]);
            Assert.Equal(key, LanguageList.IdentifierToLanguage[key]);
        });
    }
}
