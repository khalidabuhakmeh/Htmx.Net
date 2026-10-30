using System;
using JetBrains.Annotations;

namespace Htmx;

/// <summary>
/// An element reference in htmx 4 <c>HX-Source</c> / <c>HX-Target</c> format: <c>tag#id</c>, or <c>tag</c> when there is no id.
/// </summary>
/// <param name="TagName">The tag name as received (client sends lowercase). Empty if the value had no tag.</param>
/// <param name="Id">The id as received (URI-encoded by the client), or null.</param>
/// <param name="Raw">The original header value.</param>
[PublicAPI]
public readonly record struct HtmxElementReference(string TagName, string? Id, string Raw)
{
    /// <summary>The id with URI-decoding applied, or null. Falls back to <see cref="Id"/> if decoding fails.</summary>
    public string? DecodedId
    {
        get
        {
            if (Id is null) return null;
            try { return Uri.UnescapeDataString(Id); }
            catch (UriFormatException) { return Id; }
        }
    }

    /// <summary>
    /// Parses a <c>tag#id</c> value, splitting on the first <c>#</c> only.
    /// </summary>
    public static bool TryParse(string? value, out HtmxElementReference result)
    {
        if (string.IsNullOrEmpty(value))
        {
            result = default;
            return false;
        }

        var index = value.IndexOf('#');
        result = index < 0
            ? new HtmxElementReference(value, null, value)
            : new HtmxElementReference(value[..index], value[(index + 1)..], value);
        return true;
    }
}
