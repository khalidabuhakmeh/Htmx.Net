using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Htmx;

/// <summary>
/// Htmx Request Headers
/// https://htmx.org/reference/#request_headers
/// </summary>
[PublicAPI]
public class HtmxRequestHeaders
{
    /// <summary>
    /// Represents a collection of predefined header keys used in HTMX requests.
    /// </summary>
    public static class Keys
    {
        /// <summary>
        /// Represents the key for the `HX-Current-URL` header, which provides the URL of the current page
        /// at the time the request was made in an HTMX-enabled application.
        /// </summary>
        public const string CurrentUrl = "HX-Current-URL";

        /// <summary>
        /// Represents the key for the `HX-History-Restore-Request` header, which indicates that the request
        /// is triggered as part of a client-side history restoration process in an HTMX-enabled application.
        /// </summary>
        public const string HistoryRestoreRequest = "HX-History-Restore-Request";

        /// <summary>
        /// Represents the key for the `HX-Prompt` header, which contains the value entered by the user
        /// in response to a prompt initiated during an HTMX request.
        /// </summary>
        public const string Prompt = "HX-Prompt";

        /// <summary>
        /// Represents the key for the `HX-Request` header, which is used to identify if the HTTP request
        /// originates from an HTMX-enabled action.
        /// </summary>
        public const string Request = "HX-Request";

        /// <summary>
        /// Represents the key for the `HX-Target` header, which specifies the ID of the target element
        /// receiving the content during an HTMX request.
        /// </summary>
        public const string Target = "HX-Target";

        /// <summary>
        /// Represents the key for the `HX-Trigger-Name` header, which specifies the name of the triggered element
        /// that initiated the request in an HTMX-enabled application.
        /// </summary>
        public const string TriggerName = "HX-Trigger-Name";

        /// <summary>
        /// Represents the key for the `HX-Trigger` header, which specifies the value of the `id` attribute
        /// of the HTML element that triggered the request in an HTMX-enabled application.
        /// </summary>
        public const string Trigger = "HX-Trigger";

        /// <summary>
        /// Represents the key for the `HX-Boosted` header, which indicates whether the request was triggered
        /// by a boosted link or form in an HTMX-enabled application.
        /// </summary>
        public const string Boosted = "HX-Boosted";

        /// <summary>
        /// Represents a collection of all predefined HTMX request header keys
        /// that are commonly used for identifying specific behaviors or states
        /// in an HTMX-enabled application.
        /// </summary>
        public static string[] All { get; } =
        [
            CurrentUrl, HistoryRestoreRequest, Prompt, Request, Target, TriggerName, Trigger, Boosted
        ];
    }

    /// <summary>
    /// Represents the headers used by HTMX in an HTTP request.
    /// </summary>
    public HtmxRequestHeaders(HttpRequest request)
    {
        var headers = request.Headers;
        CurrentUrl = headers.GetValueOrDefault(Keys.CurrentUrl, string.Empty);
        HistoryRestoreRequest = headers.GetValueOrDefault(Keys.HistoryRestoreRequest, false);
        Prompt = headers.GetValueOrDefault(Keys.Prompt, string.Empty);
        Target = headers.GetValueOrDefault(Keys.Target, string.Empty);
        TriggerName = headers.GetValueOrDefault(Keys.TriggerName, string.Empty);
        Trigger = headers.GetValueOrDefault(Keys.Trigger, string.Empty);
        Boosted = headers.GetValueOrDefault(Keys.Boosted, false);
    }

    /// <summary>
    /// the current URL of the browser
    /// </summary>
    public string CurrentUrl { get; }
    /// <summary>
    /// true if the request is for history restoration after a miss in the local history cache
    /// </summary>
    public bool HistoryRestoreRequest { get; }
    /// <summary>
    /// the user response to an hx-prompt on the client
    /// </summary>
    public string Prompt { get; }
    /// <summary>
    /// the id of the target element if it exists
    /// </summary>
    public string Target { get; }
    /// <summary>
    /// the name of the triggered element if it exists
    /// </summary>
    public string TriggerName { get; }
    /// <summary>
    /// the id of the triggered element if it exists
    /// </summary>
    public string Trigger { get; }
        
    /// <summary>
    /// Determines if the request was Boosted looking at "Hx-Boosted" header
    /// See https://htmx.org/docs/#boosting
    /// </summary>
    public bool Boosted { get; }
}