using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Htmx.TagHelpers;

public static class HtmxAntiforgeryScriptEndpoints
{
    private static readonly string MinifiedETag = ComputeETag(HtmxSnippets.AntiforgeryJavaScriptMinified);
    private static readonly string FullETag = ComputeETag(HtmxSnippets.AntiforgeryJavaScript);

    private static string ComputeETag(string content) =>
        "\"" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..32] + "\"";

    /// <summary>
    /// The path to the antiforgery script that is used from HTML.
    /// Note: Be sure it is rooted path ("starts with '/') or else it can break as paths get more nested.
    /// </summary>
    public static string Path { get; private set; } = "/_htmx/antiforgery.js";

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
            var etag = minified ? MinifiedETag : FullETag;

            ctx.Response.Headers.ETag = etag;
            ctx.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";

            var ifNoneMatch = ctx.Request.Headers.IfNoneMatch;
            foreach (var value in ifNoneMatch)
            {
                foreach (var candidate in (value ?? "").Split(','))
                {
                    var tag = candidate.Trim();
                    if (tag == "*" || tag == etag || tag == "W/" + etag)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status304NotModified;
                        return;
                    }
                }
            }

            ctx.Response.ContentType = "text/javascript";
            await ctx.Response.WriteAsync(javaScript);
        });
    }
}