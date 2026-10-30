using System.Net;
using System.Threading.Tasks;
using Htmx.TagHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Htmx.Tests.TagHelpers;

// Path is process-global static state; keep these tests serial within the class (xunit does) and restore the default.
[Collection("AntiforgeryEndpoints")]
public class HtmxAntiforgeryScriptEndpointsTests
{
    private static async Task<IHost> Start(string? path, bool minified)
    {
        var host = new HostBuilder().ConfigureWebHost(w => w
            .UseTestServer()
            .ConfigureServices(s => s.AddRouting())
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(e => e.MapHtmxAntiforgeryScript(path, minified));
            })).Build();
        await host.StartAsync();
        return host;
    }

    [Fact]
    public async Task Default_path_serves_minified_script_with_etag()
    {
        using var host = await Start(null, true);
        Assert.Equal("/_htmx/antiforgery.js", HtmxAntiforgeryScriptEndpoints.Path);
        var res = await host.GetTestClient().GetAsync("/_htmx/antiforgery.js");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/javascript", res.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HtmxSnippets.AntiforgeryJavaScriptMinified, await res.Content.ReadAsStringAsync());
        Assert.NotNull(res.Headers.ETag);
    }

    [Fact]
    public async Task Matching_if_none_match_returns_304()
    {
        using var host = await Start(null, true);
        var client = host.GetTestClient();
        var first = await client.GetAsync("/_htmx/antiforgery.js");
        var etag = first.Headers.ETag!.ToString();

        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/_htmx/antiforgery.js");
        req.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var second = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsStringAsync());

        var weak = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/_htmx/antiforgery.js");
        weak.Headers.TryAddWithoutValidation("If-None-Match", "\"other\", W/" + etag);
        Assert.Equal(HttpStatusCode.NotModified, (await client.SendAsync(weak)).StatusCode);

        var star = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/_htmx/antiforgery.js");
        star.Headers.TryAddWithoutValidation("If-None-Match", "*");
        Assert.Equal(HttpStatusCode.NotModified, (await client.SendAsync(star)).StatusCode);
    }

    [Fact]
    public async Task Non_matching_if_none_match_returns_200()
    {
        using var host = await Start(null, true);
        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/_htmx/antiforgery.js");
        req.Headers.TryAddWithoutValidation("If-None-Match", "\"nope\"");
        var res = await host.GetTestClient().SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Non_minified_option_serves_full_script_with_different_etag()
    {
        string minEtag;
        using (var min = await Start(null, true))
            minEtag = (await min.GetTestClient().GetAsync("/_htmx/antiforgery.js")).Headers.ETag!.ToString();

        using var host = await Start(null, false);
        var res = await host.GetTestClient().GetAsync("/_htmx/antiforgery.js");
        Assert.Equal(HtmxSnippets.AntiforgeryJavaScript, await res.Content.ReadAsStringAsync());
        Assert.NotEqual(minEtag, res.Headers.ETag!.ToString());
    }

    [Fact]
    public async Task Custom_path_works()
    {
        try
        {
            using var host = await Start("/custom/af.js", true);
            Assert.Equal("/custom/af.js", HtmxAntiforgeryScriptEndpoints.Path);
            var client = host.GetTestClient();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/custom/af.js")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/_htmx/antiforgery.js")).StatusCode);
        }
        finally
        {
            using var _ = await Start("/_htmx/antiforgery.js", true); // restore global default
        }
    }
}
