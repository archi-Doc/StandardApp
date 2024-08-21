// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Arc.WinUI;

public static class LanguageList
{// language: en, identifier: Language.En, text: English
    static LanguageList()
    {
    }

    public static string LanguageFile { get; set; } = "Resources.Strings.String-{0}.tinyhand";

    private static Arc.Crypto.Utf16Hashtable<string> languageToIdentifier = new();
    private static Arc.Crypto.Utf16Hashtable<string> identifierToLanguage = new();

    /// <summary>
    /// Tries to add a language and its identifier to the language list.
    /// </summary>
    /// <param name="language">The language to add 'en'.</param>
    /// <param name="identifier">The identifier for the language 'Language.En'.</param>
    /// <returns><c>true</c> if the language and identifier were added successfully; otherwise, <c>false</c>.</returns>
    public static bool TryAdd(string language, string identifier)
    {
        var result = languageToIdentifier.TryAdd(language, identifier);
        identifierToLanguage.TryAdd(identifier, language);
        return result;
    }

    public static bool TryGetIdentifier(string language, [MaybeNullWhen(false)] out string identifier)
        => languageToIdentifier.TryGetValue(language, out identifier);

    public static bool TryGetLanguage(string identifier, [MaybeNullWhen(false)] out string language)
        => identifierToLanguage.TryGetValue(identifier, out language);

    public static void LoadHashedString(Assembly assembly)
    {
        var languages = identifierToLanguage.ToArray();
        foreach (var x in languages)
        {
            HashedString.LoadAssembly(x, assembly, string.Format(LanguageFile, x));
        }
    }
}
