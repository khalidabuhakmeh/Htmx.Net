using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Htmx
{
    public static class HtmxHttpExtensions
    {
        /// <summary>
        /// Determines if the current HTTP Request was invoked by Htmx on the client
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsHtmx(this HttpRequest request)
        {
            return request?.Headers.ContainsKey(HtmxRequestHeaders.Keys.Request) is true;
        }

        /// <summary>
        /// Determines if the current HTTP Request was invoked by Htmx on the client
        /// </summary>
        /// <param name="request">The HTTP Request</param>
        /// <param name="values">All the potential Htmx Header Values</param>
        /// <returns></returns>
        public static bool IsHtmx(this HttpRequest request, out HtmxRequestHeaders? values)
        {
            var isHtmx = request.IsHtmx();
            values = isHtmx ? new HtmxRequestHeaders(request) : null;
            return isHtmx;
        }

        /// <summary>
        /// true if the request is for history restoration after a miss in the local history cache
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsHtmxHistoryRestoreRequest(this HttpRequest request)
        {
            return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.HistoryRestoreRequest, false) is true;
        }
        
        /// <summary>
        /// true if the request is an HTMX Boosted Request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsHtmxBoosted(this HttpRequest request)
        {
            return request?.Headers.GetValueOrDefault(HtmxRequestHeaders.Keys.Boosted, false) is true;
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

        internal static T GetValueOrDefault<T>(this IHeaderDictionary headers, string key, T @default)
        {
            return headers.TryGetValue(key, out var values)
                ? (T)Convert.ChangeType(values.First(), typeof(T))
                : @default;
        }
    }
}