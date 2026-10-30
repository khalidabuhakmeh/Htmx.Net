using System;
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
        /// Represents the key for the htmx 4 `HX-Source` header (<c>tag#id</c> of the element that triggered the request).
        /// </summary>
        public const string Source = "HX-Source";

        /// <summary>
        /// Represents the key for the htmx 4 `HX-Request-Type` header (<c>full</c> or <c>partial</c>).
        /// </summary>
        public const string RequestType = "HX-Request-Type";

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
            Request, RequestType, CurrentUrl, Source, Target, Boosted, HistoryRestoreRequest, Prompt
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
        TargetElement = HtmxElementReference.TryParse(Target, out var t) ? t : null;
        Source = headers.GetValueOrDefault(Keys.Source, string.Empty);
        SourceElement = HtmxElementReference.TryParse(Source, out var s) ? s : null;
        RequestType = HtmxRequestTypeParser.Parse(headers.GetValueOrDefault(Keys.RequestType, string.Empty));
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
    /// the user response to an hx-prompt on the client.
    /// Not sent by htmx 4 core; only sent when the hx-prompt extension is installed.
    /// </summary>
    public string Prompt { get; }
    /// <summary>
    /// the raw HX-Target value, in <c>tag#id</c> format (or bare <c>tag</c> when there is no id)
    /// </summary>
    public string Target { get; }
    /// <summary>
    /// the parsed HX-Target, or null when absent
    /// </summary>
    public HtmxElementReference? TargetElement { get; }
    /// <summary>
    /// the raw HX-Source value (<c>tag#id</c>, or bare <c>tag</c>); replaces HX-Trigger in htmx 4
    /// </summary>
    public string Source { get; }
    /// <summary>
    /// the parsed HX-Source, or null when absent
    /// </summary>
    public HtmxElementReference? SourceElement { get; }
    /// <summary>
    /// the HX-Request-Type (full or partial); Unknown when missing or unrecognized
    /// </summary>
    public HtmxRequestType RequestType { get; }

    /// <summary>Removed in htmx 4.</summary>
    [Obsolete("Use Source / SourceElement (the HX-Source header replaces HX-Trigger in htmx 4).", error: true)]
    public string Trigger => throw new NotSupportedException();

    /// <summary>Removed in htmx 4.</summary>
    [Obsolete("HX-Trigger-Name was removed in htmx 4. Use Source / SourceElement.", error: true)]
    public string TriggerName => throw new NotSupportedException();
        
    /// <summary>
    /// Determines if the request was Boosted looking at "Hx-Boosted" header
    /// See https://htmx.org/docs/#boosting
    /// </summary>
    public bool Boosted { get; }
}