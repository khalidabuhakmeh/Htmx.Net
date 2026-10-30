using JetBrains.Annotations;

namespace Htmx;

/// <summary>
/// Value of the htmx 4 <c>HX-Request-Type</c> request header.
/// </summary>
[PublicAPI]
public enum HtmxRequestType
{
    /// <summary>Header missing or unrecognized.</summary>
    Unknown = 0,

    /// <summary>The target is <c>body</c> or <c>hx-select</c> is used.</summary>
    Full,

    /// <summary>The request targets something other than <c>body</c>.</summary>
    Partial
}

internal static class HtmxRequestTypeParser
{
    internal static HtmxRequestType Parse(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "full" => HtmxRequestType.Full,
        "partial" => HtmxRequestType.Partial,
        _ => HtmxRequestType.Unknown
    };
}
