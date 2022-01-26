using Microsoft.AspNetCore.Http;

namespace Htmx
{
    /// <summary>
    /// Htmx Response Headers
    /// https://htmx.org/reference/#response_headers
    /// </summary>
    public class HtmxResponseHeaders
    {
        private readonly IHeaderDictionary _headers;

        public static class Keys
        {
            public const string Push = "HX-Push";
            public const string Redirect = "HX-Redirect";
            public const string Refresh = "HX-Refresh";
            public const string Trigger = "HX-Trigger";
            public const string TriggerAfterSettle = "HX-Trigger-After-Settle";
            public const string TriggerAfterSwap = "HX-Trigger-After-Swap";
            public const string Retarget = "HX-Retarget";
        }

        internal HtmxResponseHeaders(IHeaderDictionary headers)
        {
            _headers = headers;
        }

        /// <summary>
        ///	pushes a new url into the history stack
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmxResponseHeaders Push(string value)
        {
            _headers[Keys.Push] = value;
            return this;
        }

        /// <summary>
        /// Can be used to do a client-side redirect to a new location
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmxResponseHeaders Redirect(string value)
        {
            _headers[Keys.Redirect] = value;
            return this;
        }

        /// <summary>
        /// if called the client side will do a a full refresh of the page
        /// </summary>
        /// <returns></returns>
        public HtmxResponseHeaders Refresh()
        {
            _headers[Keys.Refresh] = "true";
            return this;
        }

        /// <summary>
        /// allows you to trigger client side events
        /// https://htmx.org/headers/hx-trigger
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmxResponseHeaders Trigger(string value)
        {
            _headers[Keys.Trigger] = value;
            return this;
        }

        /// <summary>
        /// allows you to trigger client side events after a settle
        /// https://htmx.org/headers/hx-trigger
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmxResponseHeaders TriggerAfterSettle(string value)
        {
            _headers[Keys.TriggerAfterSettle] = value;
            return this;
        }

        /// <summary>
        /// allows you to trigger client side events after a swap
        /// https://htmx.org/headers/hx-trigger
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HtmxResponseHeaders TriggerAfterSwap(string value)
        {
            _headers[Keys.TriggerAfterSwap] = value;
            return this;
        }

        /// <summary>
        /// A header that allows you to change the default target of returned content
        /// </summary>
        /// <param name="value">CSS Selector of target</param>
        /// <returns></returns>
        public HtmxResponseHeaders Retarget(string value)
        {
            _headers[Keys.Retarget] = value;
            return this;
        }
    }
}