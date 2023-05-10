using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Htmx.TagHelpers;

[PublicAPI]
public static class HtmlExtensions
{
    /// <summary>
    /// Adds an event handler for "htmx:configRequest" to hydrate
    /// an antiforgery token on each HTMX mutating request (POST, PUT, DELETE).
    /// </summary>
    /// <remarks>
    /// Note: This includes wrapping script tags.To get the JavaScript string use <see cref="HtmxSnippets.AntiforgeryJavaScript">HtmxSnippets.AntiforgeryJavaScript</see>.
    /// </remarks>
    /// <param name="helper">An instance of the HTML Helper interface</param>
    /// <returns>HTML Content with JavaScript tag</returns>
    public static IHtmlContent HtmxAntiforgeryScript(this IHtmlHelper helper)
    {
        var script = helper.Raw($"<script>{HtmxSnippets.AntiforgeryJavaScript}</script>");
        return script;
    }
}