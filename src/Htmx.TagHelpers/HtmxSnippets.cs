using System;
using System.IO;
using JetBrains.Annotations;

namespace Htmx.TagHelpers;

/// <summary>
/// Provides predefined JavaScript snippets that can be used to integrate HTMX functionality
/// with antiforgery tokens in ASP.NET applications.
/// </summary>
[PublicAPI]
public static class HtmxSnippets
{
    /// <summary>
    /// Provides the antiforgery JavaScript code snippet used to integrate antiforgery token
    /// handling within HTMX operations.
    /// The script is loaded as a string from the assembly's embedded resources.
    /// </summary>
    public static string AntiforgeryJavaScript { get; } = GetString(nameof(AntiforgeryJavaScript));

    /// <summary>
    /// Represents the minified version of the antiforgery JavaScript code snippet.
    /// This property retrieves a pre-minified JavaScript resource designed to integrate antiforgery protection.
    /// The resource is loaded as an embedded string from the assembly's manifest resources.
    /// </summary>
    public static string AntiforgeryJavaScriptMinified { get; } = GetString(nameof(AntiforgeryJavaScriptMinified));

    /// <summary>
    /// Retrieves the JavaScript string containing the antiforgery script.
    /// </summary>
    /// <param name="minified">Determines whether the returned JavaScript is minified.</param>
    /// <returns>A string containing the antiforgery JavaScript, either in minified or non-minified form.</returns>
    public static string GetAntiforgeryJavaScript(bool minified) =>
        minified ? AntiforgeryJavaScriptMinified : AntiforgeryJavaScript;

    private static string GetString(string name)
    {
        var assembly = typeof(HtmxSnippets).Assembly;
        using var resource = assembly.GetManifestResourceStream(name);

        if (resource == null)
            throw new ArgumentException($"Resource \"{name}\" was not found.", nameof(name));
            
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }
}