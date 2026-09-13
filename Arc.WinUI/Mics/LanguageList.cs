// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Arc.WinUI;

/// <summary>
/// Registers language identifiers and loads their embedded localized strings.
/// </summary>
public static class LanguageList
{
    /// <summary>
    /// Gets or sets the format string of the embedded language resource name ({0}: language).
    /// </summary>
    public static string LanguageFileFormat { get; set; } = "Resources.Strings.String-{0}.tinyhand";

    /// <summary>
    /// Gets an immutable snapshot mapping language codes to string identifiers.
    /// </summary>
    public static FrozenDictionary<string, string> LanguageToIdentifier
    {
        get
        {
            lock (SyncObject)
            {
                return languageToIdentifier ??= languageToIdentifierDictionary.ToFrozenDictionary();
            }
        }
    }

    /// <summary>
    /// Gets an immutable snapshot mapping string identifiers to language codes.
    /// </summary>
    public static FrozenDictionary<string, string> IdentifierToLanguage
    {
        get
        {
            lock (SyncObject)
            {
                return identifierToLanguage ??= identifierToLanguageDictionary.ToFrozenDictionary();
            }
        }
    }

    private static readonly object SyncObject = new();

    private static FrozenDictionary<string, string>? languageToIdentifier;
    private static FrozenDictionary<string, string>? identifierToLanguage;
    private static Dictionary<string, string> languageToIdentifierDictionary = new();
    private static Dictionary<string, string> identifierToLanguageDictionary = new();

    /// <summary>
    /// Adds a language and its identifier to the language list.
    /// </summary>
    /// <param name="language">The language to add 'en'.</param>
    /// <param name="identifier">The identifier for the language 'Language.En'.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    /// <exception cref="ArgumentException">The language or identifier is already registered.</exception>
    public static void Add(string language, string identifier)
    {
        // language: en, identifier: Language.En, text: English
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(identifier);

        lock (SyncObject)
        {
            if (languageToIdentifierDictionary.ContainsKey(language))
            {
                throw new ArgumentException("The language is already registered.", nameof(language));
            }

            if (identifierToLanguageDictionary.ContainsKey(identifier))
            {
                throw new ArgumentException("The identifier is already registered.", nameof(identifier));
            }

            languageToIdentifierDictionary.Add(language, identifier);
            identifierToLanguageDictionary.Add(identifier, language);
            languageToIdentifier = null;
            identifierToLanguage = null;
        }
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
