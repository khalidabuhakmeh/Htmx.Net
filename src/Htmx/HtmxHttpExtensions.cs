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
    /// true if the request is an HTMX Boosted Request
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
    public static void StopPolling(this HttpResponse response)
    {
        response.StatusCode = 286;
    }

    internal static T GetValueOrDefault<T>(this IHeaderDictionary headers, string key, T @default)
    {
        return headers.TryGetValue(key, out var values)
            ? (T)Convert.ChangeType(values.First(), typeof(T))
            : @default;
    }
}