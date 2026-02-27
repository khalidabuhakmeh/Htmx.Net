using System.Collections.Generic;
using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace Htmx;

/// <summary>
/// A class that can be used to set the HX-Location header
/// Represents the HX-Location header as a complex object.
/// https://htmx.org/headers/hx-location/
/// </summary>
[PublicAPI]
public class HtmxLocation
{
    /// <summary>
    /// The path to navigate to. Required
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The source element of the request.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// An event that triggered the request.
    /// </summary>
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    /// <summary>
    /// A callback that will handle the response HTML.
    /// </summary>
    [JsonPropertyName("handler")]
    public string? Handler { get; set; }

    /// <summary>
    /// The target element to load the response into.
    /// </summary>
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    /// <summary>
    /// The swap strategy to use.
    /// </summary>
    [JsonPropertyName("swap")]
    public string? Swap { get; set; }

    /// <summary>
    /// The data to send with the request. Using an IDictionary to support JavaScript
    /// naming conventions that may include "-" i.e. "product-name".
    /// </summary>
    [JsonPropertyName("values")]
    public IDictionary<string, object>? Values { get; set; }

    /// <summary>
    /// The headers to send with the request.
    /// </summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Allows you to select the content you want swapped from a response.
    /// </summary>
    [JsonPropertyName("select")]
    public string? Select { get; set; }

    /// <summary>
    /// Set to 'false' or a path string to prevent or override the URL pushed to browser location history.
    /// </summary>
    [JsonPropertyName("push")]
    public object? Push { get; set; }

    /// <summary>
    /// A path string to replace the URL in the browser location history.
    /// </summary>
    [JsonPropertyName("replace")]
    public string? Replace { get; set; }
}
