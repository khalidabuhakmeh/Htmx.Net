using System.Collections.Generic;
using JetBrains.Annotations;

namespace Htmx;

/// <summary>
/// Represents the HX-Location header as a complex object.
/// https://htmx.org/headers/hx-location/
/// </summary>
[PublicAPI]
public class HtmxLocation
{
    /// <summary>
    /// The path to navigate to. Required
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The source element of the request.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// An event that triggered the request.
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// A callback that will handle the response HTML.
    /// </summary>
    public string? Handler { get; set; }

    /// <summary>
    /// The target element to load the response into.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The swap strategy to use.
    /// </summary>
    public string? Swap { get; set; }

    /// <summary>
    /// The data to send with the request. Using an IDictionary to support JavaScript
    /// naming conventions that may include "-" i.e. "product-name".
    /// </summary>
    public IDictionary<string, object>? Values { get; set; }

    /// <summary>
    /// The headers to send with the request.
    /// </summary>
    public IDictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Allows you to select the content you want swapped from a response.
    /// </summary>
    public string? Select { get; set; }

    /// <summary>
    /// Set to 'false' or a path string to prevent or override the URL pushed to browser location history.
    /// </summary>
    public object? Push { get; set; }

    /// <summary>
    /// A path string to replace the URL in the browser location history.
    /// </summary>
    public string? Replace { get; set; }
}
