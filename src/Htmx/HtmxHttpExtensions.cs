using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Htmx;

/// <summary>
/// Provides extension methods for handling HttpRequest and HttpResponse objects
/// in the context of Htmx interactions. These extensions are designed to
/// simplify the detection and manipulation of Htmx-related headers in HTTP
/// requests and responses.
/// </summary>
[PublicAPI]
public static class HtmxHttpExtensions
{
    /// <summary>
    /// Determines if the current HTTP Request was invoked by Htmx on the client
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmx(this HttpRequest? request)
    {
        return request?.Headers.ContainsKey(HtmxRequestHeaders.Keys.Request) is true;
    }

    /// <summary>
    /// Determines if the current HTTP Request was invoked by Htmx on the client
    /// </summary>
    /// <param name="request">The HTTP Request</param>
    /// <param name="values">All the potential Htmx Header Values</param>
    /// <returns></returns>
    public static bool IsHtmx(this HttpRequest? request, out HtmxRequestHeaders? values)
    {
        var isHtmx = request.IsHtmx();
        values = request is not null && isHtmx ? new HtmxRequestHeaders(request) : null;
        return isHtmx;
    }

    /// <summary>
    /// true if the request is for history restoration after a miss in the local history cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmxHistoryRestoreRequest(this HttpRequest? request)
    {
        return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.HistoryRestoreRequest, false) is true;
    }
        
    /// <summary>
    /// true if the htmx request is a partial request (<c>HX-Request-Type: partial</c>),
    /// i.e. the response is swapped into part of the page.
    /// Only meaningful on htmx requests (when <see cref="IsHtmx(HttpRequest?)"/> is true).
    /// Boosted vs. non-boosted (<c>HX-Boosted</c>) is a separate axis from full vs. partial.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmxPartial(this HttpRequest? request)
    {
        return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.RequestType, string.Empty) is { } v
               && HtmxRequestTypeParser.Parse(v) == HtmxRequestType.Partial;
    }

    /// <summary>
    /// true if the htmx request is a full request (<c>HX-Request-Type: full</c>),
    /// i.e. the response replaces the whole page body.
    /// Only meaningful on htmx requests (when <see cref="IsHtmx(HttpRequest?)"/> is true).
    /// Boosted vs. non-boosted (<c>HX-Boosted</c>) is a separate axis from full vs. partial.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmxFullRequest(this HttpRequest? request)
    {
        return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.RequestType, string.Empty) is { } v
               && HtmxRequestTypeParser.Parse(v) == HtmxRequestType.Full;
    }

    /// <summary>
    /// true if the request is an HTMX Boosted Request (<c>HX-Boosted: true</c>).
    /// Malformed values are treated as false. This is independent of full vs. partial request type.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmxBoosted(this HttpRequest? request)
    {
        return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.Boosted, false) is true;
    }
    
    /// <summary>
    /// true if the request is an HTMX Request that is not HTMX Boosted
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool IsHtmxNonBoosted(this HttpRequest? request)
    {
        return request?.IsHtmx() is true && !request.IsHtmxBoosted();
    }
    
    /// <summary>
    /// true if the request is an HTMX Request that is not HTMX Boosted
    /// </summary>
    /// <param name="request">The HTTP Request</param>
    /// <param name="values">All the potential Htmx Header Values</param>
    /// <returns></returns>
    public static bool IsHtmxNonBoosted(this HttpRequest? request, out HtmxRequestHeaders? values)
    {
        var isHtmx = request.IsHtmxNonBoosted();
        values = request is not null && isHtmx ? new HtmxRequestHeaders(request) : null;
        return isHtmx;
    }

    /// <summary>
    /// Set the Htmx Response Headers
    /// </summary>
    /// <param name="response"></param>
    /// <param name="action">Action used to action the response headers</param>
    public static void Htmx(this HttpResponse response, Action<HtmxResponseHeaders> action)
    {
        var headerContainer = new HtmxResponseHeaders(response.Headers);
        action(headerContainer);
        headerContainer.Process();
    }

    /// <summary>
    /// Sets the response status code to 286, which tells htmx to stop polling.
    /// </summary>
    /// <param name="response"></param>
    [Obsolete("htmx 4 no longer stops polling on HTTP 286. Return a response that does not contain the polling element, or use the hx-status/noSwap pattern. See docs.", error: true)]
    public static void StopPolling(this HttpResponse response)
    {
        response.StatusCode = 286;
    }

    internal static T GetValueOrDefault<T>(this IHeaderDictionary headers, string key, T @default)
    {
        if (!headers.TryGetValue(key, out var values))
            return @default;

        var raw = values.FirstOrDefault();
        if (raw is null)
            return @default;

        if (typeof(T) == typeof(string))
            return (T)(object)raw;

        if (typeof(T) == typeof(bool))
            return bool.TryParse(raw.Trim(), out var b) ? (T)(object)b : @default;

        try
        {
            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch (Exception e) when (e is FormatException or InvalidCastException or OverflowException)
        {
            return @default;
        }
    }
}