// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Arc.WinUI;

public static class LanguageList
{// language: en, identifier: Language.En, text: English
    static LanguageList()
    {
    }

    /// <summary>
    /// Gets or sets the format string of the embedded language resource name ({0}: language).
    /// </summary>
    public static string LanguageFileFormat { get; set; } = "Resources.Strings.String-{0}.tinyhand";

    public static FrozenDictionary<string, string> LanguageToIdentifier => languageToIdentifier ??= languageToIdentifierDictionary.ToFrozenDictionary();

    public static FrozenDictionary<string, string> IdentifierToLanguage => identifierToLanguage ??= identifierToLanguageDictionary.ToFrozenDictionary();

    private static FrozenDictionary<string, string>? languageToIdentifier;
    private static FrozenDictionary<string, string>? identifierToLanguage;
    private static Dictionary<string, string> languageToIdentifierDictionary = new();
    private static Dictionary<string, string> identifierToLanguageDictionary = new();

    /// <summary>
    /// Adds a language and its identifier to the language list.
    /// </summary>
    /// <param name="language">The language to add 'en'.</param>
    /// <param name="identifier">The identifier for the language 'Language.En'.</param>
    public static void Add(string language, string identifier)
    {
        languageToIdentifierDictionary.Add(language, identifier);
        identifierToLanguageDictionary.Add(identifier, language);
    }

    public static bool TryGetIdentifier(string language, [MaybeNullWhen(false)] out string identifier)
        => LanguageToIdentifier.TryGetValue(language, out identifier);

    public static bool TryGetLanguage(string identifier, [MaybeNullWhen(false)] out string language)
        => IdentifierToLanguage.TryGetValue(identifier, out language);

    /// <summary>
    /// Loads the hashed strings of all registered languages from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly that contains the embedded language resources.</param>
    public static void LoadHashedStrings(Assembly assembly)
    {
        foreach (var x in LanguageToIdentifier.Keys)
        {
            HashedString.LoadAssembly(x, assembly, string.Format(LanguageFileFormat, x));
        }
    }
}
