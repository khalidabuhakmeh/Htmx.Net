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

    private readonly Dictionary<HtmxTriggerTiming, Dictionary<string, object?>> triggers = new();

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
        /// Represents the HTMX response header key used to specify client-side triggers
        /// that should be executed after the settle phase of an HTMX request.
        /// The "HX-Trigger-After-Settle" header allows the server to queue specific
        /// events to trigger once the response has been fully settled by the client.
        /// </summary>
        public const string TriggerAfterSettle = "HX-Trigger-After-Settle";

        /// <summary>
        /// Represents the HTMX response header key used to specify client-side triggers
        /// that should be executed after the "swap" operation completes.
        /// When included in the server's response, it signals the client to invoke
        /// the designated trigger(s) at this specific timing in the update lifecycle.
        /// </summary>
        public const string TriggerAfterSwap = "HX-Trigger-After-Swap";

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
            TriggerAfterSettle,
            TriggerAfterSwap,
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
    [Obsolete("HX-Push was replaced with HX-Push-Url, use PushUrl(value) instead")]
    public HtmxResponseHeaders Push(string value)
    {
        return PushUrl(value);
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
    /// allows you to trigger client side events
    /// https://htmx.org/headers/hx-trigger
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("WithTrigger(name, val?, timing?) provides more options for configuring triggers.")]
    public HtmxResponseHeaders Trigger(string value)
    {
        headers[Keys.Trigger] = value;
        return this;
    }

    /// <summary>
    /// allows you to trigger client side events after a settle
    /// https://htmx.org/headers/hx-trigger
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("WithTrigger(name, val?, timing?) provides more options for configuring triggers.")]
    public HtmxResponseHeaders TriggerAfterSettle(string value)
    {
        headers[Keys.TriggerAfterSettle] = value;
        return this;
    }

    /// <summary>
    /// allows you to trigger client side events after a swap
    /// https://htmx.org/headers/hx-trigger
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [Obsolete("WithTrigger(name, val?, timing?) provides more options for configuring triggers.")]
    public HtmxResponseHeaders TriggerAfterSwap(string value)
    {
        headers[Keys.TriggerAfterSwap] = value;
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
    public HtmxResponseHeaders WithVary()
    {
        headers.Append("Vary", "HX-Request");
        return this;
    }

    /// <summary>
    /// Trigger a client side event named <paramref name="eventName"/> at different stages of the HTMX lifecycle.
    /// </summary>
    /// <param name="eventName">Event name</param>
    /// <param name="detail">Detail to be sent with the event, string or other JSON mappable object</param>
    /// <param name="timing">The HTMX request lifecycle step to run the trigger at</param>
    /// <returns></returns>
    public HtmxResponseHeaders WithTrigger(string eventName, object? detail = null,
        HtmxTriggerTiming timing = HtmxTriggerTiming.Default)
    {
        if (triggers.TryGetValue(timing, out var trigger))
        {
            trigger.TryAdd(eventName, detail ?? string.Empty);
        }
        else
        {
            triggers.Add(timing, new Dictionary<string, object?>
            {
                { eventName, detail ?? string.Empty }
            });
        }

        return this;
    }

    /// <summary>
    /// Process the Triggers
    /// </summary>
    internal HtmxResponseHeaders Process()
    {
        if (triggers.ContainsKey(HtmxTriggerTiming.Default))
        {
            ParsePossibleExistingTriggers(Keys.Trigger, HtmxTriggerTiming.Default);

            headers[Keys.Trigger] = BuildTriggerHeader(HtmxTriggerTiming.Default);
        }

        if (triggers.ContainsKey(HtmxTriggerTiming.AfterSettle))
        {
            ParsePossibleExistingTriggers(Keys.TriggerAfterSettle, HtmxTriggerTiming.AfterSettle);

            headers[Keys.TriggerAfterSettle] = BuildTriggerHeader(HtmxTriggerTiming.AfterSettle);
        }

        // ReSharper disable once InvertIf
        if (triggers.ContainsKey(HtmxTriggerTiming.AfterSwap))
        {
            ParsePossibleExistingTriggers(Keys.TriggerAfterSwap, HtmxTriggerTiming.AfterSwap);

            headers[Keys.TriggerAfterSwap] = BuildTriggerHeader(HtmxTriggerTiming.AfterSwap);
        }

        return this;
    }

    /// <summary>
    /// Checks to see if the response has an existing header defined by headerKey.  If it does the
    /// header loads all of the triggers locally so they aren't overwritten by Htmx.
    /// </summary>
    /// <param name="headerKey"></param>
    /// <param name="timing"></param>
    private void ParsePossibleExistingTriggers(string headerKey, HtmxTriggerTiming timing)
    {
        if (!headers.TryGetValue(headerKey, out var header))
            return;

        // Attempt to parse existing header as Json, if fails it is a simplified event key
        // assume if the string starts with '{' and ends with '}', that it is JSON
        if (header.Any(h => h is ['{', .., '}']))
        {
            // this might still throw :(
            var jsonObject = JsonNode.Parse(header)!.AsObject();
            // Load any existing triggers
            foreach (var (key, value) in jsonObject)
                WithTrigger(key, value, timing);
        }
        else
        {
            foreach (var headerValue in header)
            {
                if (headerValue is null) continue;

                var eventNames = headerValue.Split(',');

                foreach (var eventName in eventNames)
                    WithTrigger(eventName, null, timing);
            }
        }
    }

    private string BuildTriggerHeader(HtmxTriggerTiming timing)
    {
        var trigger = triggers[timing];
        // Reduce the payload if the user has only specified 1 trigger with no value
        if (trigger.Count == 1 &&
            ReferenceEquals(trigger.First().Value, string.Empty))
        {
            return trigger.First().Key;
        }

        var jsonHeader = JsonSerializer.Serialize(trigger);
        return jsonHeader;
    }
}