using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Htmx.IntegrationTests;

public class SampleFactory : WebApplicationFactory<Program>;

public abstract class TestBase(SampleFactory factory) : IClassFixture<SampleFactory>
{
    protected HttpClient NewClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    protected static async Task<IDocument> Parse(HttpResponseMessage r)
    {
        var html = await r.Content.ReadAsStringAsync();
        return await BrowsingContext.New(Configuration.Default).OpenAsync(req => req.Content(html));
    }

    protected static string? Tid(IDocument d, string id) =>
        d.QuerySelector($"[data-testid='{id}']")?.TextContent;

    protected static HttpRequestMessage Req(string url, HttpMethod? m = null, params (string, string)[] headers)
    {
        var r = new HttpRequestMessage(m ?? HttpMethod.Get, url);
        foreach (var (k, v) in headers) r.Headers.TryAddWithoutValidation(k, v);
        return r;
    }

    protected static string? Hdr(HttpResponseMessage r, string name) =>
        r.Headers.TryGetValues(name, out var v) ? string.Join(",", v) : null;

    protected async Task<IDocument> Page(string url)
    {
        var r = await NewClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return await Parse(r);
    }
}

public class RequestHeaderTests(SampleFactory f) : TestBase(f)
{
    private async Task<IDocument> Echo(params (string, string)[] h)
    {
        var r = await NewClient().SendAsync(Req("/demo/request-headers", null, h));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return await Parse(r);
    }

    [Fact]
    public async Task Page_Renders_FullLoad_NotHtmx()
    {
        var d = await Page("/RequestHeaders");
        Assert.Equal("False", Tid(d, "rh-page-is-htmx"));
        Assert.NotNull(d.QuerySelector("[data-testid=rh-btn-id]"));
    }

    [Fact]
    public async Task SourceWithId_And_Partial()
    {
        var d = await Echo(("HX-Request", "true"), ("HX-Source", "button#rh-btn-id"), ("HX-Target", "div#rh-result"),
            ("HX-Request-Type", "partial"), ("HX-Current-URL", "http://localhost/RequestHeaders"));
        Assert.Equal("True", Tid(d, "rh-is-htmx"));
        Assert.Equal("button#rh-btn-id", Tid(d, "rh-source"));
        Assert.Equal("button", Tid(d, "rh-source-tag"));
        Assert.Equal("rh-btn-id", Tid(d, "rh-source-id"));
        Assert.Equal("div", Tid(d, "rh-target-tag"));
        Assert.Equal("rh-result", Tid(d, "rh-target-id"));
        Assert.Equal("Partial", Tid(d, "rh-request-type"));
        Assert.Equal("True", Tid(d, "rh-is-partial"));
        Assert.Equal("False", Tid(d, "rh-is-full"));
        Assert.Equal("http://localhost/RequestHeaders", Tid(d, "rh-current-url"));
    }

    [Fact]
    public async Task BareTagSource_HasNoId()
    {
        var d = await Echo(("HX-Request", "true"), ("HX-Source", "button"));
        Assert.Equal("button", Tid(d, "rh-source-tag"));
        Assert.Equal("", Tid(d, "rh-source-id"));
    }

    [Fact]
    public async Task FullRequestType()
    {
        var d = await Echo(("HX-Request", "true"), ("HX-Request-Type", "full"));
        Assert.Equal("Full", Tid(d, "rh-request-type"));
        Assert.Equal("True", Tid(d, "rh-is-full"));
        Assert.Equal("False", Tid(d, "rh-is-partial"));
    }

    [Fact]
    public async Task BoostedAndHistoryRestore()
    {
        var d = await Echo(("HX-Request", "true"), ("HX-Boosted", "true"), ("HX-History-Restore-Request", "true"));
        Assert.Equal("True", Tid(d, "rh-is-boosted"));
        Assert.Equal("False", Tid(d, "rh-is-nonboosted"));
        Assert.Equal("True", Tid(d, "rh-is-history-restore"));
    }

    [Fact]
    public async Task NonHtmx_IsNotHtmx()
    {
        var d = await Echo();
        Assert.Equal("False", Tid(d, "rh-is-htmx"));
        Assert.Equal("", Tid(d, "rh-source-tag"));
    }
}

public class ResponseHeaderTests(SampleFactory f) : TestBase(f)
{
    private async Task<HttpResponseMessage> Get(string name)
    {
        var r = await NewClient().SendAsync(Req($"/demo/resp/{name}", null, ("HX-Request", "true")));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        foreach (var bad in new[] { "HX-Trigger-After-Swap", "HX-Trigger-After-Settle" })
            Assert.False(r.Headers.Contains(bad));
        return r;
    }

    [Fact]
    public async Task ResponseHeadersPage_Renders() =>
        Assert.NotNull((await Page("/ResponseHeaders")).QuerySelector("[data-testid]"));

    [Fact]
    public async Task PushUrl()
    {
        var r = await Get("push-url");
        Assert.Equal("/ResponseHeaders?pushed=1", Hdr(r, "HX-Push-Url"));
        Assert.Contains("Vary", r.Headers.Select(h => h.Key));
    }

    [Fact]
    public async Task PreventPush()
    {
        var r = await Get("prevent-push");
        Assert.Equal("false", Hdr(r, "HX-Push-Url"));
        var vary = Hdr(r, "Vary") ?? "";
        Assert.Contains("HX-Source", vary);
        Assert.Contains("HX-Target", vary);
    }

    [Fact] public async Task ReplaceUrl() => Assert.Equal("/ResponseHeaders?replaced=1", Hdr(await Get("replace-url"), "HX-Replace-Url"));
    [Fact] public async Task PreventReplace() => Assert.Equal("false", Hdr(await Get("prevent-replace"), "HX-Replace-Url"));
    [Fact] public async Task Redirect() => Assert.Equal("/Boosted", Hdr(await Get("redirect"), "HX-Redirect"));
    [Fact] public async Task Refresh() => Assert.Equal("true", Hdr(await Get("refresh"), "HX-Refresh"));
    [Fact] public async Task Retarget() => Assert.Equal("#resp-retarget-target", Hdr(await Get("retarget"), "HX-Retarget"));
    [Fact] public async Task Reswap() => Assert.Equal("beforeend", Hdr(await Get("reswap"), "HX-Reswap"));
    [Fact] public async Task Reselect() => Assert.Equal("#resp-pick", Hdr(await Get("reselect"), "HX-Reselect"));
    [Fact] public async Task LocationString() => Assert.Equal("/Boosted", Hdr(await Get("location-string"), "HX-Location"));

    [Fact]
    public async Task LocationObject_IsJson()
    {
        var v = Hdr(await Get("location-object"), "HX-Location")!;
        var j = JsonDocument.Parse(v).RootElement;
        Assert.Equal("/Location?handler=Landing", j.GetProperty("path").GetString());
        Assert.Equal("#resp-result", j.GetProperty("target").GetString());
        Assert.Equal("innerHTML", j.GetProperty("swap").GetString());
        Assert.Equal("#loc-incoming", j.GetProperty("select").GetString());
    }

    [Theory]
    [InlineData("innerHTML")] [InlineData("outerHTML")] [InlineData("delete")] [InlineData("none")]
    [InlineData("outerSync")] [InlineData("innerMorph")] [InlineData("outerMorph")] [InlineData("textContent")]
    [InlineData("before")] [InlineData("prepend")] [InlineData("append")] [InlineData("after")]
    public async Task SwapStyles_SendReswap(string style)
    {
        var r = await NewClient().GetAsync($"/demo/swap/{style}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(style, Hdr(r, "HX-Reswap"));
    }

    [Fact]
    public async Task UnknownHandler_404() =>
        Assert.Equal(HttpStatusCode.NotFound, (await NewClient().GetAsync("/demo/resp/nope")).StatusCode);
}

public class TriggerTests(SampleFactory f) : TestBase(f)
{
    private async Task<string> Trig(string kind)
    {
        var r = await NewClient().GetAsync($"/demo/trigger/{kind}");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.False(r.Headers.Contains("HX-Trigger-After-Swap"));
        Assert.False(r.Headers.Contains("HX-Trigger-After-Settle"));
        return Hdr(r, "HX-Trigger")!;
    }

    [Fact] public async Task PageRenders() => Assert.NotNull((await Page("/Triggers")).QuerySelector("[data-testid]"));
    [Fact] public async Task Single_IsCompactName() => Assert.Equal("demo-single", await Trig("single"));

    [Fact]
    public async Task Detail_IsJson()
    {
        var j = JsonDocument.Parse(await Trig("detail")).RootElement.GetProperty("demo-detail");
        Assert.Equal(42, j.GetProperty("id").GetInt32());
        Assert.Equal("hello", j.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Scalar_IsJson() =>
        Assert.Equal("plain-value", JsonDocument.Parse(await Trig("scalar")).RootElement.GetProperty("demo-scalar").GetString());

    [Fact]
    public async Task Multiple()
    {
        var j = JsonDocument.Parse(await Trig("multi")).RootElement;
        Assert.True(j.TryGetProperty("demo-multi-a", out _));
        Assert.Equal(2, j.GetProperty("demo-multi-b").GetProperty("n").GetInt32());
    }

    [Fact]
    public async Task MergeWithExistingNames()
    {
        var j = JsonDocument.Parse(await Trig("merge-names")).RootElement;
        Assert.True(j.TryGetProperty("demo-pre-a", out _));
        Assert.True(j.TryGetProperty("demo-pre-b", out _));
        Assert.Equal(1, j.GetProperty("demo-added").GetProperty("n").GetInt32());
    }

    [Fact]
    public async Task MergeWithExistingJson()
    {
        var j = JsonDocument.Parse(await Trig("merge-json")).RootElement;
        Assert.Equal(7, j.GetProperty("demo-pre-json").GetProperty("id").GetInt32());
        Assert.Equal(1, j.GetProperty("demo-added").GetProperty("n").GetInt32());
    }

    [Fact]
    public async Task FirstWriteWins()
    {
        var j = JsonDocument.Parse(await Trig("first-wins")).RootElement;
        Assert.Equal("first", j.GetProperty("demo-dup").GetProperty("v").GetString());
    }
}

public class ConfigTests(SampleFactory f) : TestBase(f)
{
    private static readonly string[] Removed =
    [
        "historyEnabled", "historyCacheSize", "refreshOnHistoryMiss", "defaultSwapStyle", "defaultSettleDelay_", "timeout",
        "wsReconnectDelay", "wsBinaryType", "disableSelector", "withCredentials", "scrollBehavior", "scrollIntoViewOnBoost",
        "getCacheBusterParam", "globalViewTransitions", "methodsThatUseUrlParams", "selfRequestsOnly", "ignoreTitle",
        "disableInheritance", "allowScriptTags", "allowEval", "useTemplateFragments", "attributesToSettle", "inlineStyleNonce",
        "addedClass", "settlingClass", "swappingClass", "defaultSwapDelay", "triggerSpecsCache", "responseHandling", "allowNestedOobSwaps"
    ];

    private async Task<JsonElement> Meta(string url)
    {
        var d = await Page(url);
        var metas = d.QuerySelectorAll("meta[name=htmx-config]");
        Assert.Single(metas);
        return JsonDocument.Parse(metas[0].GetAttribute("content")!).RootElement.Clone();
    }

    [Fact]
    public async Task Global_Meta_HasAntiForgery_AndOnlyExpectedKeys()
    {
        var j = await Meta("/Config".Replace("Config", "Boosted"));
        var keys = j.EnumerateObject().Select(p => p.Name).ToHashSet();
        Assert.Contains("antiForgery", keys);
        Assert.Contains("indicatorClass", keys);
        Assert.Equal(new HashSet<string> { "antiForgery", "indicatorClass" }, keys);
        var af = j.GetProperty("antiForgery");
        Assert.False(string.IsNullOrEmpty(af.GetProperty("requestToken").GetString()));
        Assert.False(string.IsNullOrEmpty(af.GetProperty("headerName").GetString()));
    }

    [Fact]
    public async Task ConfigPage_Meta_IsHtmx4Only()
    {
        var j = await Meta("/Config");
        var keys = j.EnumerateObject().Select(p => p.Name).ToList();
        foreach (var r in Removed) Assert.DoesNotContain(r, keys);
        Assert.Contains("antiForgery", keys);
        Assert.Equal("beforeend", j.GetProperty("defaultSwap").GetString());
        Assert.Equal("same-origin", j.GetProperty("mode").GetString());
        Assert.Equal(7, j.GetProperty("morphScanLimit").GetInt32());
        Assert.Equal(45000, j.GetProperty("defaultTimeout").GetInt32());
        Assert.False(j.GetProperty("logAll").GetBoolean());
        Assert.Equal(3, j.GetProperty("noSwap").GetArrayLength());
        // Unset-by-page properties must not be serialised.
        Assert.DoesNotContain("htmx", keys);
    }

    [Fact]
    public async Task OtherPages_DoNotLeakConfigKeys()
    {
        var j = await Meta("/Inherited");
        Assert.DoesNotContain("defaultSwap", j.EnumerateObject().Select(p => p.Name));
    }
}

public class UrlHelperTests(SampleFactory f) : TestBase(f)
{
    private static string? Attr(IDocument d, string tid, string name) =>
        d.QuerySelector($"[data-testid='{tid}']")?.GetAttribute(name);

    [Fact]
    public async Task GeneratedUrls()
    {
        var d = await Page("/UrlHelper");
        Assert.Equal("/UrlHelper?handler=Fragment", Attr(d, "url-btn-page", "hx-get"));
        Assert.Equal("/RequestHeaders", Attr(d, "url-btn-page-only", "hx-get"));
        Assert.Equal("/demo/echo/7?p=ctrl", Attr(d, "url-btn-controller", "hx-get"));
        Assert.Equal("/demo/echo", Attr(d, "url-btn-route", "hx-get"));
        Assert.Equal("/demo/echo/42?p=rv", Attr(d, "url-btn-route-values", "hx-get"));
        Assert.Equal("/admin/ping", Attr(d, "url-btn-area", "hx-get"));
        Assert.Equal("/demo/echo/9#frag", Attr(d, "url-btn-fragment", "hx-get"));
        Assert.Equal("/demo/echo/data", Attr(d, "url-btn-data", "data-hx-get"));
        Assert.Equal("/UrlHelper?handler=Post", Attr(d, "url-btn-post", "hx-post"));
        Assert.Equal("/demo/echo/explicit", Attr(d, "url-btn-existing", "hx-get"));
        Assert.Equal("https://example.test/demo/echo", Attr(d, "url-btn-host", "hx-get"));
    }

    [Fact]
    public async Task InheritedUrls()
    {
        var d = await Page("/UrlHelper");
        Assert.Equal("/demo/echo/inh", Attr(d, "url-inherited-parent", "hx-get:inherited"));
        Assert.Equal("/demo/echo/inh2", Attr(d, "url-inherited-parent2", "hx-get:inherited"));
        Assert.Null(Attr(d, "url-inherited-parent", "hx-get"));
    }

    [Fact]
    public async Task GeneratedUrls_Resolve()
    {
        var c = NewClient();
        foreach (var u in new[] { "/UrlHelper?handler=Fragment", "/demo/echo/7?p=ctrl", "/admin/ping" })
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync(u)).StatusCode);
        var d = await Parse(await c.GetAsync("/demo/echo/7?p=ctrl"));
        Assert.Contains("id=7 p=ctrl", Tid(d, "url-result"));
    }
}

public class InheritedTests(SampleFactory f) : TestBase(f)
{
    [Fact]
    public async Task InheritedAttributes_RenderWithSuffix()
    {
        var d = await Page("/Inherited");
        var with = d.QuerySelector("[data-testid=inh-with]")!;
        Assert.Equal("#inh-out", with.GetAttribute("hx-target:inherited"));
        Assert.Null(with.GetAttribute("hx-target"));
        var headers = with.Attributes.FirstOrDefault(a => a.Name.StartsWith("hx-headers"));
        Assert.NotNull(headers);
        Assert.Contains(":inherited", with.Attributes.Select(a => a.Name).First(n => n.StartsWith("hx-headers")) + with.OuterHtml);
        var without = d.QuerySelector("[data-testid=inh-without]")!;
        Assert.Equal("#inh-out2", without.GetAttribute("hx-target"));
        Assert.Null(without.GetAttribute("hx-target:inherited"));
    }

    [Fact]
    public async Task Echo_ReflectsHeaderAndTarget()
    {
        var r = await NewClient().SendAsync(Req("/demo/inherited/echo?p=inh", null,
            ("HX-Request", "true"), ("HX-Target", "div#inh-out"), ("X-Demo", "from-parent")));
        var d = await Parse(r);
        Assert.Equal("from-parent", Tid(d, "inh-header"));
        Assert.Equal("div#inh-out", Tid(d, "inh-target"));
        var d2 = await Parse(await NewClient().SendAsync(Req("/demo/inherited/echo?p=x", null, ("HX-Request", "true"))));
        Assert.Equal("none", Tid(d2, "x-header"));
    }
}

public class AntiforgeryTests(SampleFactory f) : TestBase(f)
{
    private async Task<(HttpClient c, string token, string header, string field)> Setup()
    {
        var c = NewClient();
        var d = await Parse(await c.GetAsync("/Antiforgery"));
        var json = JsonDocument.Parse(d.QuerySelector("meta[name=htmx-config]")!.GetAttribute("content")!).RootElement;
        var af = json.GetProperty("antiForgery");
        var token = WebUtility.HtmlDecode(af.GetProperty("requestToken").GetString()!);
        return (c, token, af.GetProperty("headerName").GetString()!, d.QuerySelector("[data-testid=af-field-name]")!.TextContent);
    }

    [Fact]
    public async Task Page_Shows_Names()
    {
        var (_, token, header, field) = await Setup();
        Assert.NotEmpty(token);
        Assert.Equal("RequestVerificationToken", header);
        Assert.Equal("__RequestVerificationToken", field);
    }

    [Theory]
    [InlineData("POST")] [InlineData("PUT")] [InlineData("PATCH")] [InlineData("DELETE")]
    public async Task Strict_HeaderToken_Succeeds(string method)
    {
        var (c, token, header, _) = await Setup();
        var r = await c.SendAsync(Req("/demo/af/strict", new HttpMethod(method), ("HX-Request", "true"), (header, token)));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains(method, Tid(await Parse(r), "af-strict-result"));
    }

    [Fact]
    public async Task Strict_FormToken_Succeeds()
    {
        var (c, token, _, field) = await Setup();
        var req = Req("/demo/af/strict", HttpMethod.Post, ("HX-Request", "true"));
        req.Content = new FormUrlEncodedContent([new(field, token)]);
        Assert.Equal(HttpStatusCode.OK, (await c.SendAsync(req)).StatusCode);
    }

    [Fact]
    public async Task Strict_NoToken_Is400()
    {
        var (c, _, _, _) = await Setup();
        Assert.Equal(HttpStatusCode.BadRequest, (await c.SendAsync(Req("/demo/af/strict", HttpMethod.Post, ("HX-Request", "true")))).StatusCode);
    }

    [Fact]
    public async Task Strict_TokenWithoutCookie_Is400()
    {
        var (_, token, header, _) = await Setup();
        var fresh = NewClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await fresh.SendAsync(Req("/demo/af/strict", HttpMethod.Post, (header, token)))).StatusCode);
    }

    [Fact]
    public async Task ValidateAttr_Header_And_NoToken()
    {
        var (c, token, header, _) = await Setup();
        Assert.Equal(HttpStatusCode.OK, (await c.SendAsync(Req("/demo/af/validate-attr", HttpMethod.Post, (header, token)))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.SendAsync(Req("/demo/af/validate-attr", HttpMethod.Post))).StatusCode);
    }

    [Fact]
    public async Task Check_ReportsSource()
    {
        var (c, token, header, field) = await Setup();
        var d = await Parse(await c.SendAsync(Req("/demo/af/check", HttpMethod.Post, (header, token))));
        Assert.Equal("header", Tid(d, "af-token-source"));
        Assert.Equal("True", Tid(d, "af-valid"));

        var req = Req("/demo/af/check", HttpMethod.Post);
        req.Content = new FormUrlEncodedContent([new(field, token), new("note", "hi")]);
        var d2 = await Parse(await c.SendAsync(req));
        Assert.Equal("form", Tid(d2, "af-token-source"));
        Assert.Equal("hi", Tid(d2, "af-note"));

        var bad = await c.SendAsync(Req("/demo/af/check", HttpMethod.Put));
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.Equal("False", Tid(await Parse(bad), "af-valid"));
    }

    [Fact]
    public async Task Get_NeedsNoToken() =>
        Assert.Equal("n/a", Tid(await Parse(await NewClient().GetAsync("/demo/af/check")), "af-valid"));

    [Fact]
    public async Task ExistingTokenField_RenderedOnce()
    {
        var d = await Page("/Antiforgery");
        Assert.Single(d.QuerySelectorAll("[data-testid=af-form-existing] input[type=hidden]"));
    }

    [Fact]
    public async Task ScriptEndpoint_Served()
    {
        var r = await NewClient().GetAsync(Htmx.TagHelpers.HtmxAntiforgeryScriptEndpoints.Path);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("javascript", r.Content.Headers.ContentType!.MediaType);
    }
}

public class PageTests(SampleFactory f) : TestBase(f)
{
    [Fact]
    public async Task Index_ListsAllDemos()
    {
        var d = await Page("/");
        foreach (var n in Htmx4.Sample.Demos.All)
            Assert.NotNull(d.QuerySelector($"[data-testid='nav-{n}']"));
    }

    [Theory]
    [InlineData("Boosted")] [InlineData("BoostedOther")] [InlineData("Partials")] [InlineData("Morph")]
    [InlineData("ErrorSwaps")] [InlineData("Location")] [InlineData("Polling")] [InlineData("Config")]
    public async Task EveryPage_Renders_WithHtmxScript(string page)
    {
        var d = await Page("/" + page);
        Assert.NotNull(d.QuerySelector("script[src*='htmx']"));
        Assert.NotNull(d.QuerySelector("[data-testid=main]"));
    }

    [Fact]
    public async Task Boosted_Page_ReportsRequestType()
    {
        var c = NewClient();
        var d = await Parse(await c.GetAsync("/Boosted"));
        Assert.Equal("False", Tid(d, "boost-is-htmx"));
        Assert.Equal("n/a", Tid(d, "boost-request-type"));
        Assert.Equal("boosted", d.QuerySelector("[data-testid=boost-container]")?.GetAttribute("hx-boost:inherited") == "true" ? "boosted" : "x");

        var d2 = await Parse(await c.SendAsync(Req("/Boosted", null, ("HX-Request", "true"), ("HX-Boosted", "true"), ("HX-Request-Type", "full"))));
        Assert.Equal("True", Tid(d2, "boost-is-boosted"));
        Assert.Equal("Full", Tid(d2, "boost-request-type"));
    }

    [Fact]
    public async Task Boosted_Post_RequiresAntiforgery()
    {
        var c = NewClient();
        var d = await Parse(await c.GetAsync("/Boosted"));
        var cfg = JsonDocument.Parse(d.QuerySelector("meta[name=htmx-config]")!.GetAttribute("content")!).RootElement.GetProperty("antiForgery");
        var token = WebUtility.HtmlDecode(cfg.GetProperty("requestToken").GetString()!);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.SendAsync(Req("/Boosted?handler=Post", HttpMethod.Post, ("HX-Request", "true"), ("HX-Boosted", "true")))).StatusCode);
        var ok = await c.SendAsync(Req("/Boosted?handler=Post", HttpMethod.Post, ("HX-Request", "true"), ("HX-Boosted", "true"), (cfg.GetProperty("headerName").GetString()!, token)));
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Contains("boosted=True", Tid(await Parse(ok), "boost-post-result"));
    }

    [Fact]
    public async Task Partials_Fragment_HasPartialsAndOob()
    {
        var r = await NewClient().SendAsync(Req("/demo/partials", null, ("HX-Request", "true")));
        var d = await Parse(r);
        var partials = d.QuerySelectorAll("hx-partial");
        Assert.Equal(3, partials.Length);
        Assert.Equal("#part-a", partials[0].GetAttribute("hx-target"));
        Assert.Equal("beforeend", partials[1].GetAttribute("hx-swap"));
        Assert.Equal("true", d.QuerySelector("#part-oob")!.GetAttribute("hx-swap-oob"));
        Assert.NotNull(d.QuerySelector("[data-testid=part-main-content]"));
    }

    [Fact]
    public async Task Partials_OnlyHandler()
    {
        var d = await Parse(await NewClient().SendAsync(Req("/Partials?handler=Only", null, ("HX-Request", "true"))));
        Assert.Equal("#part-c", d.QuerySelector("hx-partial")!.GetAttribute("hx-target"));
        Assert.Equal("C (partial only)", Tid(d, "part-c"));
    }

    [Theory]
    [InlineData("plain", null)] [InlineData("outer", "outerMorph")] [InlineData("inner", "innerMorph")]
    public async Task Morph_Fragments(string kind, string? swap)
    {
        var r = await NewClient().SendAsync(Req($"/demo/morph/{kind}", null, ("HX-Request", "true")));
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(swap, Hdr(r, "HX-Reswap"));
        Assert.NotNull((await Parse(r)).QuerySelector($"[data-testid=morph-{kind}-input]"));
    }

    [Fact]
    public async Task ErrorSwaps_Statuses()
    {
        var c = NewClient();
        var r422 = await c.GetAsync("/demo/status/422");
        Assert.Equal((HttpStatusCode)422, r422.StatusCode);
        Assert.Contains("Validation failed", Tid(await Parse(r422), "err-422-body"));
        var r500 = await c.GetAsync("/demo/status/500");
        Assert.Equal(HttpStatusCode.InternalServerError, r500.StatusCode);
        var r204 = await c.GetAsync("/demo/status/204");
        Assert.Equal(HttpStatusCode.NoContent, r204.StatusCode);
        Assert.Equal("", await r204.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotModified, (await c.GetAsync("/demo/status/304")).StatusCode);
    }

    [Fact]
    public async Task ErrorSwaps_PageHandlers()
    {
        var c = NewClient();
        var bad = await c.SendAsync(Req("/ErrorSwaps?handler=Validate", HttpMethod.Post, ("HX-Request", "true")));
        Assert.Equal((HttpStatusCode)422, bad.StatusCode);
        var req = Req("/ErrorSwaps?handler=Validate", HttpMethod.Post, ("HX-Request", "true"));
        req.Content = new FormUrlEncodedContent([new("name", "Ada")]);
        var ok = await c.SendAsync(req);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Contains("Ada", Tid(await Parse(ok), "err-form-ok"));
        var del = await c.SendAsync(Req("/ErrorSwaps?handler=Delete&item=7", HttpMethod.Delete, ("HX-Request", "true")));
        Assert.Contains("item=[7]", Tid(await Parse(del), "del-result"));
    }

    [Fact]
    public async Task Location_Handlers()
    {
        var c = NewClient();
        var go = await c.SendAsync(Req("/Location?handler=Go", null, ("HX-Request", "true")));
        var j = JsonDocument.Parse(Hdr(go, "HX-Location")!).RootElement;
        Assert.Equal("/Location?handler=Landing", j.GetProperty("path").GetString());
        Assert.Equal("#loc-dest", j.GetProperty("target").GetString());
        Assert.Equal("#loc-oob-incoming:#loc-oob", j.GetProperty("selectOOB").GetString());
        Assert.Equal("hx-location", j.GetProperty("values").GetProperty("from").GetString());
        Assert.Equal("loc", j.GetProperty("headers").GetProperty("X-Demo").GetString());

        var s = await c.SendAsync(Req("/Location?handler=GoString", null, ("HX-Request", "true")));
        Assert.Equal("/Location?handler=Landing", Hdr(s, "HX-Location"));

        var ho = await c.SendAsync(Req("/Location?handler=HeaderOnly", null, ("HX-Request", "true")));
        Assert.False(ho.Headers.Contains("HX-Location"));
        var hj = JsonDocument.Parse(WebUtility.HtmlDecode(await ho.Content.ReadAsStringAsync())).RootElement;
        Assert.Equal("#loc-start", hj.GetProperty("source").GetString());

        var land = await Parse(await c.SendAsync(Req("/Location?handler=Landing&from=a&n=3", null, ("HX-Request", "true"), ("X-Demo", "loc"))));
        Assert.Contains("from=[a] n=[3] x-demo=[loc]", Tid(land, "loc-incoming"));
    }

    [Fact]
    public async Task Polling_StopsAfterN()
    {
        var c = NewClient();
        var d0 = await Parse(await c.GetAsync("/Polling"));
        Assert.NotNull(d0.QuerySelector("#poll-load[hx-get]"));
        var d1 = await Parse(await c.GetAsync("/demo/poll?n=0&stop=3&mode=load"));
        Assert.NotNull(d1.QuerySelector("[hx-trigger]"));
        Assert.Equal("1", Tid(d1, "poll-load-count"));
        var d2 = await Parse(await c.GetAsync("/demo/poll?n=1&stop=3&mode=every"));
        Assert.Equal("every 1s", d2.QuerySelector("[hx-trigger]")!.GetAttribute("hx-trigger"));
        var d3 = await Parse(await c.GetAsync("/demo/poll?n=2&stop=3&mode=load"));
        Assert.Null(d3.QuerySelector("[hx-get]"));
        Assert.Null(d3.QuerySelector("[hx-trigger]"));
        Assert.NotNull(Tid(d3, "poll-load-done"));
        Assert.Equal("3", Tid(d3, "poll-load-count"));
    }
}
