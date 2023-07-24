using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Htmx.TagHelpers;

public static class HtmxAntiforgeryScriptEndpoints
{
    /// <summary>
    /// The path to the antiforgery script that is used from HTML
    /// </summary>
    public static string Path { get; private set; } = "_htmx/antiforgery.js";

    /// <summary>
    /// Register an endpoint that responds with the HTMX antiforgery script.<br/>
    /// IMPORTANT: Remember to add the following script tag to your _Layout.cshtml or Razor view:
    /// <![CDATA[
    /// <script src="@HtmxAntiforgeryScriptEndpoints.Path" defer></script>
    /// ]]>
    /// </summary>
    /// <param name="builder">Endpoint builder</param>
    /// <param name="path">The path to the antiforgery script</param>
    /// <param name="minified">Determines if the JavaScript is minified (defaults to true)</param>
    /// <returns>The registered endpoint (Use <seealso cref="Path"/> to reference endpoint)</returns>
    public static IEndpointConventionBuilder MapHtmxAntiforgeryScript(
        this IEndpointRouteBuilder builder,
        string? path = null,
        bool minified = true)
    {
        // set Path globally for access
        Path = path ?? Path;
        
        return builder.MapGet(Path, async ctx =>
        {
            var javaScript = HtmxSnippets.GetAntiforgeryJavaScript(minified);
            
            ctx.Response.ContentType = "text/javascript";
            await ctx.Response.WriteAsync(javaScript);
        });
    }
}