using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Htmx;

/// <summary>
/// A class that can be used to set the HX-Location header
/// </summary>
public class HtmxLocation
{
    /// <summary>
    /// the path to the location
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = default!;

    /// <summary>
    /// the target element to swap into
    /// </summary>
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    /// <summary>
    /// the source element to swap from
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// the event handler to trigger
    /// </summary>
    [JsonPropertyName("handler")]
    public string? Handler { get; set; }

    /// <summary>
    /// values to be sent with the request
    /// </summary>
    [JsonPropertyName("values")]
    public object? Values { get; set; }

    /// <summary>
    /// headers to be sent with the request
    /// </summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// a CSS selector that allows you to choose which part of the response is used to be swapped in.
    /// </summary>
    [JsonPropertyName("select")]
    public string? Select { get; set; }

    /// <summary>
    /// the swap style to use
    /// </summary>
    [JsonPropertyName("swap")]
    public string? Swap { get; set; }
}
