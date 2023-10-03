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
    public static class Keys
    {
        public const string CurrentUrl = "HX-Current-URL";
        public const string HistoryRestoreRequest = "HX-History-Restore-Request";
        public const string Prompt = "HX-Prompt";
        public const string Request = "HX-Request";
        public const string Target = "HX-Target";
        public const string TriggerName = "HX-Trigger-Name";
        public const string Trigger = "HX-Trigger";
        public const string Boosted = "HX-Boosted";

        public static string[] All { get;  } = new[]
        {
            CurrentUrl, HistoryRestoreRequest, Prompt, Request, Target, TriggerName, Trigger, Boosted
        };
    }

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