using System.Text.Json;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Playwright.Assertions;

namespace Htmx.E2ETests;

/// <summary>
/// D5: antiforgery security scenarios in a real browser. Two origins are simulated with 127.0.0.1:port (victim)
/// and localhost:port (attacker) against the same sample server. The attacker endpoint
/// (/demo/af-test/capture) answers CORS preflights permissively and records what it receives.
/// </summary>
public class AntiforgeryTests(SampleServerFixture s, ITestOutputHelper output) : E2EBase(s)
{
    private const string HeaderName = "RequestVerificationToken";

    private string Victim => BaseUrl;                                   // http://127.0.0.1:port
    private string Attacker => BaseUrl.Replace("127.0.0.1", "localhost"); // http://localhost:port

    private readonly List<IRequest> _requests = [];

    private async Task<bool> HasHeaderAsync(IRequest r) =>
        (await r.AllHeadersAsync()).Keys.Any(k => k.Equals(HeaderName, StringComparison.OrdinalIgnoreCase));

    private async Task<string> HeaderValueAsync(IRequest r) =>
        (await r.AllHeadersAsync()).FirstOrDefault(k => k.Key.Equals(HeaderName, StringComparison.OrdinalIgnoreCase)).Value ?? "";

    private async Task<IRequest> ClickAndCaptureAsync(string testId, string urlPart) =>
        await Page.RunAndWaitForRequestAsync(() => T(testId).ClickAsync(), r => r.Url.Contains(urlPart));

    // ---------- 1. header mode ----------
    [Theory]
    [InlineData("post", "POST")]
    [InlineData("put", "PUT")]
    [InlineData("patch", "PATCH")]
    [InlineData("delete", "DELETE")]
    public async Task Header_mode_attaches_token_and_validates(string verb, string method)
    {
        await GoAsync("/Antiforgery");
        var req = await ClickAndCaptureAsync($"af-btn-header-{verb}", "/demo/af/check");
        Assert.Equal(method, req.Method);
        Assert.True(await HasHeaderAsync(req), "antiforgery header missing");
        Assert.False(string.IsNullOrEmpty(await HeaderValueAsync(req)));
        await Expect(T("af-method")).ToHaveTextAsync(method);
        await Expect(T("af-token-source")).ToHaveTextAsync("header");
        await Expect(T("af-valid")).ToHaveTextAsync("True");
    }

    [Fact]
    public async Task Header_mode_works_against_attribute_validated_endpoints()
    {
        await GoAsync("/Antiforgery");
        await T("af-btn-strict-post").ClickAsync();
        await Expect(T("af-strict-result")).ToContainTextAsync("POST validated");
        await T("af-btn-strict-delete").ClickAsync();
        await Expect(T("af-strict-result")).ToContainTextAsync("DELETE validated");
        await T("af-btn-attr").ClickAsync();
        await Expect(T("af-attr-result")).ToContainTextAsync("validated by [ValidateAntiForgeryToken]");
    }

    // ---------- 2. form-field mode ----------
    // The meta carries both a header name and a form field name and the script prefers the header. To exercise
    // form-field mode the config is switched to {formFieldName, requestToken} only (headerName removed).
    private async Task UseFormModeAsync() =>
        await Page.EvaluateAsync("() => { const a = htmx.config.antiForgery; htmx.config.antiForgery = { formFieldName: a.formFieldName, requestToken: a.requestToken }; }");

    [Theory]
    [InlineData("post", "POST")]
    [InlineData("put", "PUT")]
    [InlineData("patch", "PATCH")]
    public async Task Form_mode_puts_token_in_body_and_validates(string verb, string method)
    {
        await GoAsync("/Antiforgery");
        await UseFormModeAsync();
        var req = await ClickAndCaptureAsync($"af-btn-form-{verb}", "/demo/af/check");
        Assert.Equal(method, req.Method);
        Assert.False(await HasHeaderAsync(req), "header must not be sent in form mode");
        Assert.Contains(await Page.GetByTestId("af-field-name").InnerTextAsync(), req.PostData ?? "");
        await Expect(T("af-method")).ToHaveTextAsync(method);
        await Expect(T("af-token-source")).ToHaveTextAsync("form");
        await Expect(T("af-valid")).ToHaveTextAsync("True");
    }

    // EXPECTED htmx 4 behavior (documented limitation): in form-field mode DELETE does NOT work. htmx 4 sends DELETE form data in the URL query string (like
    // GET), so the FormData the script assigns to request.body is serialized into the URL: the token is exposed
    // in the URL (logs/Referer-adjacent) and ASP.NET Core does not read it from the query => validation fails.
    // Use header mode (the default from the meta tag) for DELETE.
    [Fact]
    public async Task Form_mode_DELETE_puts_token_in_url_query_and_fails_validation_observed_limitation()
    {
        await GoAsync("/Antiforgery");
        await UseFormModeAsync();
        var req = await ClickAndCaptureAsync("af-btn-form-delete", "/demo/af/check");
        Assert.Equal("DELETE", req.Method);
        Assert.Contains("__RequestVerificationToken=", req.Url);
        Assert.False(await HasHeaderAsync(req));
        await Expect(T("af-token-source")).ToHaveTextAsync("none");
        await Expect(T("af-valid")).ToHaveTextAsync("False");
    }

    // ---------- 3. existing token not duplicated ----------
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Existing_token_field_is_not_duplicated(bool formMode)
    {
        await GoAsync("/Antiforgery");
        if (formMode) await UseFormModeAsync();
        var req = await ClickAndCaptureAsync("af-btn-form-existing", "/demo/af/check");
        var field = await Page.GetByTestId("af-field-name").InnerTextAsync();
        var body = req.PostData ?? "";
        var count = body.Split(field).Length - 1;
        // Form mode: the rendered field must be left alone (exactly 1). Header mode: field from the form
        // (exactly 1) plus the header; the script adds no extra body field.
        Assert.Equal(1, count);
        await Expect(T("af-valid")).ToHaveTextAsync("True");
    }

    // ---------- 4. safe method ----------
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_carries_no_token(bool formMode)
    {
        await GoAsync("/Antiforgery");
        if (formMode) await UseFormModeAsync();
        var req = await ClickAndCaptureAsync("af-btn-get", "/demo/af/check");
        Assert.Equal("GET", req.Method);
        Assert.False(await HasHeaderAsync(req));
        Assert.Null(req.PostData);
        Assert.DoesNotContain("RequestVerificationToken", req.Url);
        await Expect(T("af-token-source")).ToHaveTextAsync("none");
    }

    // ---------- 5. no-token control ----------
    [Fact]
    public async Task Request_without_token_is_rejected_with_400()
    {
        await GoAsync("/Antiforgery");
        await T("af-btn-notoken").ClickAsync();
        await Expect(T("af-notoken-status")).ToHaveTextAsync("400");
    }

    [Fact]
    public async Task Check_endpoint_reports_invalid_when_script_does_not_attach_token()
    {
        // Same page, but a raw fetch (no htmx => no script involvement) to the validating echo endpoint.
        await GoAsync("/Antiforgery");
        var status = await Page.EvaluateAsync<int>("async () => (await fetch('/demo/af/check', { method: 'POST' })).status");
        Assert.Equal(400, status);
    }

    // ---------- 6. boosted token rotation (documented limitation — no auto-refresh) ----------
    // htmx 4 boosted navigation swaps only <body> and does NOT merge <head>, so a rotated token's meta
    // never enters the DOM. The snippet deliberately does NOT auto-refresh the token from response text
    // (doing so safely is impossible — a security audit rejected every response-text approach because the
    // response body can't be trusted to signal a genuine full-page navigation). So after a token-rotating
    // boosted navigation, htmx.config.antiForgery stays stale and a POST fails until a full reload.
    // Recovery is proven by Full_reload_after_rotation_still_works below.
    [Fact]
    public async Task Boosted_navigation_after_rotation_does_not_auto_refresh_and_post_fails()
    {
        await GoAsync("/Boosted");
        var before = await Page.EvaluateAsync<string>("() => htmx.config.antiForgery.requestToken");

        // Force rotation: dropping the antiforgery cookie = new session; the next render issues a new cookie token.
        Assert.Contains(await Context.CookiesAsync(), c => c.Name.StartsWith(".AspNetCore.Antiforgery"));
        await Context.ClearCookiesAsync();

        await Page.EvaluateAsync("() => { window.__marker = 'same-document'; }");
        var nav = await Page.RunAndWaitForResponseAsync(() => T("boost-link-other").ClickAsync(), r => r.Url.EndsWith("/BoostedOther"));
        var html = await nav.TextAsync();
        await Expect(T("boost-page")).ToHaveTextAsync("other");
        Assert.Equal("same-document", await Page.EvaluateAsync<string>("() => window.__marker")); // truly boosted

        var served = System.Text.RegularExpressions.Regex.Match(html, @"CfDJ8[A-Za-z0-9_\-]+").Value;
        var after = await Page.EvaluateAsync<string>("() => htmx.config.antiForgery.requestToken");
        output.WriteLine($"rotation: server rotated={before != served}; htmx.config NOT auto-refreshed={before == after}");
        Assert.NotEqual(before, served);       // the server DID rotate
        Assert.Equal(before, after);            // htmx.config is NOT auto-refreshed (by design, no response-text parsing)

        // A POST after the token-rotating boosted navigation fails, because the live token no longer
        // matches the rotated cookie. This is the documented limitation; a full reload (next test) recovers.
        var resp = await Page.RunAndWaitForResponseAsync(
            () => Page.EvaluateAsync("() => htmx.ajax('POST', '/demo/af/check', { target: '#main', swap: 'beforeend' })"),
            r => r.Url.Contains("/demo/af/check"));
        output.WriteLine($"rotation: POST after boosted nav with rotated cookie => HTTP {resp.Status}");
        Assert.Equal(400, resp.Status);
    }

    [Fact]
    public async Task Full_reload_after_rotation_still_works()
    {
        await GoAsync("/Boosted");
        await Context.ClearCookiesAsync();
        // Recovery: a full page load picks up the fresh token.
        await GoAsync("/Boosted");
        var resp2 = await Page.RunAndWaitForResponseAsync(
            () => Page.EvaluateAsync("() => htmx.ajax('POST', '/demo/af/check', { target: '#main', swap: 'beforeend' })"),
            r => r.Url.Contains("/demo/af/check"));
        Assert.Equal(200, resp2.Status);
    }

    [Fact]
    public async Task Stale_token_after_rotation_is_rejected_proving_rotation_is_real()
    {
        await GoAsync("/Boosted");
        var stale = await Page.EvaluateAsync<string>("() => htmx.config.antiForgery.requestToken");
        await Context.ClearCookiesAsync();
        // Obtain a new cookie by loading another page in a second tab (new session).
        var other = await Context.NewPageAsync();
        await other.GotoAsync(BaseUrl + "/Antiforgery");
        await other.CloseAsync();
        var status = await Page.EvaluateAsync<int>(
            "async (t) => (await fetch('/demo/af/strict', { method: 'POST', headers: { RequestVerificationToken: t } })).status", stale);
        Assert.Equal(400, status);
    }

    // ---------- 7. warp finding 1: <base href> ----------
    [Theory]
    [InlineData("min")]
    [InlineData("full")]
    public async Task Base_href_to_other_origin_blocks_token_on_relative_action(string variant)
    {
        await GoXAsync($"?variant={variant}&base={Uri.EscapeDataString(Attacker + "/")}&attacker={Uri.EscapeDataString(Attacker)}");
        await ResetCapturesAsync();

        // Relative action "/demo/af-test/capture" resolves against <base> => attacker origin.
        var req = await ClickAndCaptureAsync("x-btn-rel", "/demo/af-test/capture");
        Assert.StartsWith(Attacker, req.Url);
        Assert.False(await HasHeaderAsync(req), "TOKEN LEAKED to base-href origin");
        await Expect(T("x-out")).ToContainTextAsync("captured by localhost");

        var caps = await GetCapturesAsync();
        Assert.All(caps, c => { Assert.Equal("", c.HeaderToken); Assert.Equal("", c.FormToken); });
        Assert.Contains(caps, c => c.Method == "POST");
    }

    [Theory]
    [InlineData("min")]
    [InlineData("full")]
    public async Task Base_href_to_other_origin_blocks_token_in_form_mode(string variant)
    {
        await GoXAsync($"?variant={variant}&base={Uri.EscapeDataString(Attacker + "/")}&attacker={Uri.EscapeDataString(Attacker)}");
        await UseFormModeAsync();
        await ResetCapturesAsync();
        await ClickAndCaptureAsync("x-btn-rel", "/demo/af-test/capture");
        await Expect(T("x-out")).ToContainTextAsync("captured by localhost");
        var caps = await GetCapturesAsync();
        Assert.NotEmpty(caps);
        Assert.All(caps, c => { Assert.Equal("", c.HeaderToken); Assert.Equal("", c.FormToken); });
    }

    [Theory]
    [InlineData("min")]
    [InlineData("full")]
    public async Task Cross_origin_absolute_action_never_carries_token(string variant)
    {
        await GoXAsync($"?variant={variant}&attacker={Uri.EscapeDataString(Attacker)}");
        await ResetCapturesAsync();
        var req = await ClickAndCaptureAsync("x-btn-cross-abs", "/demo/af-test/capture");
        Assert.StartsWith(Attacker, req.Url);
        Assert.False(await HasHeaderAsync(req));
        await Expect(T("x-out")).ToContainTextAsync("captured by localhost");
        Assert.All(await GetCapturesAsync(), c => { Assert.Equal("", c.HeaderToken); Assert.Equal("", c.FormToken); });
    }

    [Theory]
    [InlineData("min")]
    [InlineData("full")]
    public async Task Positive_control_same_origin_absolute_action_carries_token_even_with_base_href(string variant)
    {
        // Guard resolves against baseURI; an absolute same-origin URL is unaffected by <base>.
        await GoXAsync($"?variant={variant}&base={Uri.EscapeDataString(Attacker + "/")}&attacker={Uri.EscapeDataString(Attacker)}");
        await ResetCapturesAsync();
        var req = await ClickAndCaptureAsync("x-btn-same-abs", "/demo/af-test/capture");
        Assert.StartsWith(Victim, req.Url);
        Assert.True(await HasHeaderAsync(req));
        var caps = await GetCapturesAsync();
        Assert.Contains(caps, c => c.Host.StartsWith("127.0.0.1") && c.HeaderToken.Length > 0);
    }

    // ---------- 8. warp finding 2: same-origin -> cross-origin redirect ----------
    // MEASURES reality. The guard only sees the ORIGINAL action (same-origin) and fetch() follows the 307/308
    // transparently, re-sending method, body and every header (custom headers survive cross-origin redirects
    // when the redirect target passes CORS preflight). The assertions below record the observed behaviour.
    [Theory]
    [InlineData(307, "min", false)]
    [InlineData(308, "min", false)]
    [InlineData(307, "full", false)]
    [InlineData(307, "min", true)]
    [InlineData(308, "min", true)]
    [InlineData(307, "full", true)]
    public async Task Redirect_same_origin_to_cross_origin_observed_exposure(int code, string variant, bool formMode)
    {
        await GoXAsync($"?variant={variant}&attacker={Uri.EscapeDataString(Attacker)}");
        if (formMode) await UseFormModeAsync();
        await ResetCapturesAsync();

        var all = new List<IRequest>();
        Page.Request += (_, r) => all.Add(r);
        await T($"x-btn-redirect{code}").ClickAsync();
        await Expect(T("x-out")).ToContainTextAsync("captured by localhost");

        var caps = await GetCapturesAsync();
        var hit = Assert.Single(caps, c => c.Method == "POST");
        var leakedHeader = hit.HeaderToken.Length > 0;
        var leakedForm = hit.FormToken.Length > 0;
        output.WriteLine($"[{code} {variant} {(formMode ? "form" : "header")}] attacker host={hit.Host} origin={hit.Origin} headerLeaked={leakedHeader} formLeaked={leakedForm}");
        foreach (var r in all.Where(r => r.Url.Contains("redirect-to") || r.Url.Contains("af-test/capture")))
            output.WriteLine($"   {r.Method} {r.Url} redirectedFrom={(r.RedirectedFrom?.Url ?? "-")}");

        // Observed: the redirect IS followed with CORS and the token IS delivered to the foreign origin.
        Assert.Equal(Victim, hit.Origin);
        if (formMode) Assert.True(leakedForm, "observed: form-field token reaches attacker via 307/308 redirect");
        else Assert.True(leakedHeader, "observed: header token reaches attacker via 307/308 redirect");
    }

    // With htmx 4's DEFAULT config.mode ("same-origin") the browser's fetch refuses to follow the redirect (and any
    // direct cross-origin URL): nothing reaches the foreign origin, so the redirect leak requires an app to opt in
    // to mode "cors". Console error from the blocked fetch is expected, so this test bypasses E2EBase's error gate
    // by only asserting on server captures.
    [Fact]
    public async Task Redirect_to_cross_origin_is_blocked_by_default_same_origin_mode()
    {
        await GoXAsync($"?attacker={Uri.EscapeDataString(Attacker)}", cors: false);
        Assert.Equal("same-origin", await Page.EvaluateAsync<string>("() => htmx.config.mode"));
        await ResetCapturesAsync();
        await T("x-btn-redirect307").ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
        Assert.Empty(await GetCapturesAsync());
        Assert.Equal("(none)", await T("x-out").InnerTextAsync());
        ExpectedConsoleErrors = true;
    }

    // ---------- helpers ----------
    // htmx 4's default config.mode is "same-origin" (fetch itself refuses cross-origin URLs and redirects).
    // Cross-origin scenarios therefore opt in to mode "cors" (what an app that talks to a CORS API would do)
    // so the request actually reaches the foreign origin and the token guard is what's being measured.
    private async Task GoXAsync(string query, bool cors = true)
    {
        await Page.GotoAsync(BaseUrl + "/AntiforgeryXorigin" + query);
        await Page.WaitForFunctionAsync("() => typeof window.htmx !== 'undefined' && window.__htmxAntiforgery === true");
        if (cors) await Page.EvaluateAsync("() => { htmx.config.mode = 'cors'; }");
    }

    private record Capture(string Host, string Method, string Origin, string HeaderToken, string FormToken);

    private async Task ResetCapturesAsync()
    {
        using var http = new HttpClient();
        (await http.PostAsync(BaseUrl + "/demo/af-test/reset", null)).EnsureSuccessStatusCode();
    }

    private async Task<List<Capture>> GetCapturesAsync()
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync(BaseUrl + "/demo/af-test/captured");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(e => new Capture(
            e.GetProperty("host").GetString()!, e.GetProperty("method").GetString()!, e.GetProperty("origin").GetString()!,
            e.GetProperty("headerToken").GetString()!, e.GetProperty("formToken").GetString()!)).ToList();
    }
}
