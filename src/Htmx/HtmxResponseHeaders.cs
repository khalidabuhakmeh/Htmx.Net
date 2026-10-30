using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Htmx;

/// <summary>
/// Htmx Response Headers
/// https://htmx.org/reference/#response_headers
/// </summary>
[PublicAPI]
public class HtmxResponseHeaders
{
    private readonly IHeaderDictionary headers;

    // htmx 4 only supports HX-Trigger. Keyed by event name; first write wins.
    private readonly Dictionary<string, object?> triggers = new();

    /// <summary>
    /// Represents a collection of constants for HTMX response headers.
    /// Provides key names for various headers used to control HTMX behaviors,
    /// such as redirection, swapping content, triggering events, etc.
    /// Reference: https://htmx.org/reference/#response_headers
    /// </summary>
    public static class Keys
    {
        // Sorted by https://htmx.org/reference/#response_headers to make it easier to update

        /// <summary>
        /// Represents the HTMX response header key for specifying a client-side redirection.
        /// The "HX-Location" header is used to instruct the client to navigate to a new location.
        /// When included in the server's response, the client will perform a full-page redirect
        /// to the specified location.
        /// </summary>
        public const string Location = "HX-Location";

        /// <summary>
        /// Represents the HTMX response header key used to control URL history management on the client side.
        /// The "HX-Push-Url" header specifies a URL to replace or add to the browser's history.
        /// Setting this header allows the server to explicitly define the new URL for the browser history
        /// during an AJAX request initiated by HTMX.
        /// </summary>
        public const string PushUrl = "HX-Push-Url";

        /// <summary>
        /// Represents the HTMX response header key for triggering a client-side redirection
        /// to a new location without initiating a full-page reload.
        /// The "HX-Redirect" header is used to specify the URL to which the client should navigate.
        /// </summary>
        public const string Redirect = "HX-Redirect";

        /// <summary>
        /// Represents the HTMX response header key for instructing a client-side full-page refresh.
        /// The "HX-Refresh" header is used to signal that the client should re-fetch and reload the current page.
        /// This is typically utilized when server-side changes require the client to reset the page state completely.
        /// Reference: https://htmx.org/reference/#response_headers
        /// </summary>
        public const string Refresh = "HX-Refresh";

        /// <summary>
        /// Represents the HTMX response header key used to specify a URL to replace the current browser history entry.
        /// When included in the server's response, the client will update the browser's address bar without reloading the page.
        /// </summary>
        public const string ReplaceUrl = "HX-Replace-Url";

        /// <summary>
        /// Represents the HTMX response header key for specifying how content swapping should occur
        /// on the client side. The "HX-Reswap" header allows the server to control the swap behavior
        /// of the content being replaced, enabling finer-grained control over the update process.
        /// </summary>
        public const string Reswap = "HX-Reswap";

        /// <summary>
        /// Represents the HTMX response header key for specifying a client-side retargeting of an action.
        /// The "HX-Retarget" header is used to designate a specific element on the page as the target
        /// for the response content. This allows dynamic updates to target elements other than the
        /// default one.
        /// </summary>
        public const string Retarget = "HX-Retarget";

        /// <summary>
        /// Represents the HTMX response header key for specifying an element to be reselected
        /// after an HX operation is completed. The "HX-Reselect" header allows the server to
        /// instruct the client to focus or reselect a specific element in the DOM following
        /// the processing of an HTMX request.
        /// </summary>
        public const string Reselect = "HX-Reselect";

        /// <summary>
        /// Represents the HTMX response header key for triggering client-side events.
        /// The "HX-Trigger" header is used to specify one or more event names that
        /// the client should trigger upon receiving the server's response.
        /// This facilitates custom client-side behaviors in response to server actions.
        /// </summary>
        public const string Trigger = "HX-Trigger";

        /// <summary>
        /// Represents an array containing all the HTMX response header keys defined in the <see cref="HtmxResponseHeaders.Keys"/> class.
        /// This property provides a central collection of all supported HTMX header keys, including those used for redirection,
        /// refreshing, content swaping, targeting, and triggering client-side behaviors in response to server actions.
        /// </summary>
        public static string[] All { get; } = new[]
        {
            Location,
            PushUrl, 
            Redirect,
            Refresh,
            ReplaceUrl,
            Reswap, 
            Retarget,
            Reselect,
            Trigger,
        };
    }

    internal HtmxResponseHeaders(IHeaderDictionary headers)
    {
        this.headers = headers;
    }

    /// <summary>
    ///	pushes a new url into the history stack
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders PushUrl(string value)
    {
        headers[Keys.PushUrl] = value;
        return this;
    }

    /// <summary>
    /// Prevents the browser history from being updated
    /// </summary>
    /// <returns></returns>
    public HtmxResponseHeaders PreventPush()
    {
        headers[Keys.PushUrl] = "false";
        return this;
    }

    /// <summary>
    /// Can be used to do a client-side redirect to a new location
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders Redirect(string value)
    {
        headers[Keys.Redirect] = value;
        return this;
    }

    /// <summary>
    /// Allows you to specify how the response will be swapped
    /// See https://htmx.org/attributes/hx-swap/ for values.
    /// You can use <see cref="HtmxSwap"/> for constants.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders Reswap(string value)
    {
        headers[Keys.Reswap] = value;
        return this;
    }
    
    /// <summary>
    /// a CSS selector that allows you to choose which part of the response is used to be swapped in.
    /// Overrides an existing hx-select on the triggering element
    /// See https://htmx.org/attributes/hx-select/ for hx-select behavior.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders Reselect(string value)
    {
        headers[Keys.Reselect] = value;
        return this;
    }

    /// <summary>
    /// Allows you to do a client-side redirect that does not do a full page reload
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders Location(string value)
    {
        headers[Keys.Location] = value;
        return this;
    }

    /// <summary>
    /// Allows you to do a client-side redirect that does not do a full page reload, with a complex object
    /// https://htmx.org/headers/hx-location/
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public HtmxResponseHeaders Location(HtmxLocation location)
    {
        var json = JsonSerializer.Serialize(location, Shared.JsonSerializerOptions);
        headers[Keys.Location] = json;
        return this;
    }

    /// <summary>
    /// Replaces the current URL in the location bar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public HtmxResponseHeaders ReplaceUrl(string value)
    {
        headers[Keys.ReplaceUrl] = value;
        return this;
    }

    /// <summary>
    /// Prevents the browser URL from being updated
    /// </summary>
    /// <returns></returns>
    public HtmxResponseHeaders PreventReplace()
    {
        headers[Keys.ReplaceUrl] = "false";
        return this;
    }

    /// <summary>
    /// if called the client side will do a a full refresh of the page
    /// </summary>
    /// <returns></returns>
    public HtmxResponseHeaders Refresh()
    {
        headers[Keys.Refresh] = "true";
        return this;
    }

    /// <summary>
    /// A header that allows you to change the default target of returned content
    /// </summary>
    /// <param name="value">CSS Selector of target</param>
    /// <returns></returns>
    public HtmxResponseHeaders Retarget(string value)
    {
        headers[Keys.Retarget] = value;
        return this;
    }

    /// <summary>
    /// Adds "Vary: HX-Request" to the response headers.
    /// Essential when serving different content (full page vs partial) to htmx requests vs standard requests.
    /// </summary>
    /// <returns></returns>
    public HtmxResponseHeaders WithVary() => WithVary(HtmxRequestHeaders.Keys.Request);

    /// <summary>
    /// Appends one or more request header names to the "Vary" response header without duplicating
    /// values already present. Use this when the server renders different content based on headers
    /// such as <c>HX-Request</c> or <c>HX-Request-Type</c> so caches key on the composite correctly.
    /// </summary>
    /// <param name="headerNames">
    /// The request header names to vary on (for example <c>HX-Request</c>, <c>HX-Request-Type</c>).
    /// </param>
    /// <returns></returns>
    public HtmxResponseHeaders WithVary(params string[] headerNames)
    {
        if (headerNames is not { Length: > 0 })
            return this;

        var existing = headers.TryGetValue("Vary", out var current)
            ? new HashSet<string>(
                current.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in headerNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            if (existing.Add(name.Trim()))
                headers.Append("Vary", name.Trim());
        }

        return this;
    }

    /// <summary>
    /// Trigger a client side event named <paramref name="eventName"/> via the HX-Trigger response header.
    /// If the same event name is added more than once, the first value wins.
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="detail">Detail to be sent with the event, string or other JSON mappable object</param>
    /// <returns></returns>
    public HtmxResponseHeaders WithTrigger(string eventName, object? detail = null)
    {
        triggers.TryAdd(eventName, detail ?? string.Empty);
        return this;
    }

    /// <summary>
    /// Removed: htmx 4 only supports HX-Trigger.
    /// </summary>
    [Obsolete("htmx 4 only supports HX-Trigger; HX-Trigger-After-Swap/Settle were removed. Use WithTrigger(name, detail).", error: true)]
    public HtmxResponseHeaders WithTrigger(string eventName, object? detail, HtmxTriggerTiming timing)
    {
        throw new NotSupportedException("htmx 4 only supports HX-Trigger.");
    }

    /// <summary>
    /// Process the Triggers
    /// </summary>
    internal HtmxResponseHeaders Process()
    {
        if (triggers.Count == 0)
            return this;

        ParsePossibleExistingTriggers();
        headers[Keys.Trigger] = BuildTriggerHeader();
        return this;
    }

    /// <summary>
    /// Loads any HX-Trigger header already set on the response so it isn't overwritten.
    /// Existing entries are added after the ones set through Htmx(), preserving first-wins semantics.
    /// </summary>
    private void ParsePossibleExistingTriggers()
    {
        if (!headers.TryGetValue(Keys.Trigger, out var header))
            return;

        foreach (var headerValue in header)
        {
            if (string.IsNullOrWhiteSpace(headerValue)) continue;

            var trimmed = headerValue.Trim();
            if (trimmed is ['{', .., '}'])
            {
                try
                {
                    if (JsonNode.Parse(trimmed) is JsonObject jsonObject)
                    {
                        foreach (var (key, value) in jsonObject)
                            WithTrigger(key, value);
                        continue;
                    }
                }
                catch (JsonException)
                {
                    // fall through to comma-split
                }
            }

            foreach (var eventName in headerValue.Split(','))
            {
                var name = eventName.Trim();
                if (name.Length > 0)
                    WithTrigger(name);
            }
        }
    }

    private string BuildTriggerHeader()
    {
        // Reduce the payload if the user has only specified 1 trigger with no value
        if (triggers.Count == 1 &&
            ReferenceEquals(triggers.First().Value, string.Empty))
        {
            return triggers.First().Key;
        }

        return JsonSerializer.Serialize(triggers);
    }
}
