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
    /// 
    /// You may also want to consider using the <see cref="HtmxAntiforgeryScriptEndpoints"/> instead of this approach.
    /// </remarks>
    /// <param name="helper">An instance of the HTML Helper interface</param>
    /// <param name="minified">Determines if the JavaScript is minified (defaults to true)</param>
    /// <returns>HTML Content with JavaScript tag</returns>
    public static IHtmlContent HtmxAntiforgeryScript(this IHtmlHelper helper, bool minified = true)
    {
        var javaScript = HtmxSnippets.GetAntiforgeryJavaScript(minified);
        var script = helper.Raw($"<script>{javaScript}</script>");
        return script;
    }
}