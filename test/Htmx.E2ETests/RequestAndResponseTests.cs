using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace Htmx.E2ETests;

public class RequestHeadersTests(SampleServerFixture s) : E2EBase(s)
{
    [Fact]
    public async Task Source_with_id_is_sent_and_echoed()
    {
        await GoAsync("/RequestHeaders");
        var req = await Page.RunAndWaitForRequestAsync(() => T("rh-btn-id").ClickAsync(), r => r.Url.Contains("/demo/request-headers?p=rh"));
        var h = await req.AllHeadersAsync();
        Assert.Equal("button#rh-btn-id", h["hx-source"]);
        Assert.Equal("partial", h["hx-request-type"]);
        Assert.Equal("div#rh-result", h["hx-target"]);

        await Expect(T("rh-is-htmx")).ToHaveTextAsync("True");
        await Expect(T("rh-source")).ToHaveTextAsync("button#rh-btn-id");
        await Expect(T("rh-source-tag")).ToHaveTextAsync("button");
        await Expect(T("rh-source-id")).ToHaveTextAsync("rh-btn-id");
        await Expect(T("rh-target-id")).ToHaveTextAsync("rh-result");
        await Expect(T("rh-request-type")).ToHaveTextAsync("Partial");
        await Expect(T("rh-is-partial")).ToHaveTextAsync("True");
        await Expect(T("rh-is-full")).ToHaveTextAsync("False");
    }

    [Fact]
    public async Task Source_without_id_uses_tag_only()
    {
        await GoAsync("/RequestHeaders");
        var req = await Page.RunAndWaitForRequestAsync(() => T("rh-btn-noid").ClickAsync(), r => r.Url.Contains("p=rh2"));
        Assert.Equal("button", (await req.AllHeadersAsync())["hx-source"]);
        await Expect(T("rh2-source")).ToHaveTextAsync("button");
        await Expect(T("rh2-source-id")).ToHaveTextAsync("");
        await Expect(T("rh2-target")).ToHaveTextAsync("div");
    }

    [Fact]
    public async Task Hx_select_makes_a_full_request()
    {
        await GoAsync("/RequestHeaders");
        var req = await Page.RunAndWaitForRequestAsync(() => T("rh-btn-full").ClickAsync(), r => r.Url.Contains("p=rh3"));
        Assert.Equal("full", (await req.AllHeadersAsync())["hx-request-type"]);
        await Expect(T("rh3-request-type")).ToHaveTextAsync("Full");
        await Expect(T("rh3-is-full")).ToHaveTextAsync("True");
    }

    [Fact]
    public async Task Header_names_are_rendered()
    {
        await GoAsync("/RequestHeaders");
        await Expect(T("rh-key-request")).ToHaveTextAsync("HX-Request");
        await Expect(T("rh-key-request-type")).ToHaveTextAsync("HX-Request-Type");
        await Expect(T("rh-page-is-htmx")).ToHaveTextAsync("False");
    }
}

public class ResponseHeadersTests(SampleServerFixture s) : E2EBase(s)
{
    [Fact]
    public async Task PushUrl_changes_location()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-push-url").ClickAsync();
        await Expect(Page).ToHaveURLAsync(BaseUrl + "/ResponseHeaders?pushed=1");
        await Expect(T("resp-result")).ToHaveTextAsync("push-url applied");
    }

    [Fact]
    public async Task ReplaceUrl_changes_location()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-replace-url").ClickAsync();
        await Expect(Page).ToHaveURLAsync(BaseUrl + "/ResponseHeaders?replaced=1");
    }

    [Fact]
    public async Task PreventPush_and_PreventReplace_veto_the_client_attribute()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-prevent-push").ClickAsync();
        await Expect(T("resp-result")).ToHaveTextAsync("prevent-push applied");
        await T("resp-btn-prevent-replace").ClickAsync();
        await Expect(T("resp-result")).ToHaveTextAsync("prevent-replace applied");
        Assert.Equal(BaseUrl + "/ResponseHeaders", Page.Url);
    }

    [Fact]
    public async Task Retarget_swaps_the_other_element()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-retarget").ClickAsync();
        await Expect(T("resp-retarget-target")).ToContainTextAsync("retargeted content");
        await Expect(T("resp-result-box")).ToHaveTextAsync("(no result yet)");
    }

    [Fact]
    public async Task Reswap_beforeend_appends()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-reswap").ClickAsync();
        await Expect(T("resp-result-box")).ToContainTextAsync("(no result yet)");
        await Expect(T("resp-result-box")).ToContainTextAsync("appended");
    }

    [Fact]
    public async Task Reselect_picks_only_the_selected_element()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-reselect").ClickAsync();
        await Expect(T("resp-picked")).ToBeVisibleAsync();
        await Expect(T("resp-skipped")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Redirect_navigates_fully()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-redirect").ClickAsync();
        await Expect(Page).ToHaveURLAsync(BaseUrl + "/Boosted");
    }

    [Fact]
    public async Task Refresh_reloads_the_page()
    {
        await GoAsync("/ResponseHeaders");
        await Page.EvaluateAsync("() => { window.__marker = 1; }");
        await T("resp-btn-refresh").ClickAsync();
        await Page.WaitForFunctionAsync("() => window.__marker === undefined");
    }

    [Fact]
    public async Task Location_string_navigates()
    {
        await GoAsync("/ResponseHeaders");
        await T("resp-btn-location-string").ClickAsync();
        await Expect(Page).ToHaveURLAsync(BaseUrl + "/Boosted");
    }

    [Theory]
    [InlineData("innerHTML")]
    [InlineData("beforeend")]
    [InlineData("afterbegin")]
    [InlineData("outerHTML")]
    [InlineData("delete")]
    [InlineData("none")]
    public async Task Swap_matrix(string style)
    {
        await GoAsync("/ResponseHeaders");
        await T($"swap-btn-{style}").ClickAsync();
        switch (style)
        {
            case "innerHTML":
                await Expect(T($"swap-target-{style}")).ToHaveTextAsync("innerHTML");
                break;
            case "beforeend":
                await Expect(T($"swap-target-{style}")).ToHaveTextAsync("originalbeforeend");
                break;
            case "afterbegin":
                await Expect(T($"swap-target-{style}")).ToHaveTextAsync("afterbeginoriginal");
                break;
            case "outerHTML":
                await Expect(T("swap-result-outerHTML")).ToBeVisibleAsync();
                break;
            case "delete":
                await Expect(T("swap-target-delete")).ToHaveCountAsync(0);
                break;
            case "none":
                await Expect(T("swap-result-none")).ToHaveCountAsync(0);
                await Expect(T("swap-target-none")).ToHaveTextAsync("original");
                break;
        }
    }
}

public class TriggersTests(SampleServerFixture s) : E2EBase(s)
{
    private ILocator Log(string name) => Page.Locator($"[data-testid=event-log-item][data-event='{name}']");

    [Fact]
    public async Task Single_event_fires()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-single").ClickAsync();
        await Expect(Log("demo-single")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Object_detail_is_copied_onto_event_detail()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-detail").ClickAsync();
        await Expect(Log("demo-detail")).ToHaveAttributeAsync("data-detail", "{\"id\":42,\"message\":\"hello\"}");
    }

    [Fact]
    public async Task Scalar_detail_is_in_value()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-scalar").ClickAsync();
        await Expect(Log("demo-scalar")).ToHaveAttributeAsync("data-detail", "{\"value\":\"plain-value\"}");
    }

    [Fact]
    public async Task Multiple_events_fire()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-multi").ClickAsync();
        await Expect(Log("demo-multi-a")).ToHaveCountAsync(1);
        await Expect(Log("demo-multi-b")).ToHaveAttributeAsync("data-detail", "{\"n\":2}");
    }

    [Fact]
    public async Task Merge_with_preset_name_list_keeps_all()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-merge-names").ClickAsync();
        await Expect(Log("demo-pre-a")).ToHaveCountAsync(1);
        await Expect(Log("demo-pre-b")).ToHaveCountAsync(1);
        await Expect(Log("demo-added")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Merge_with_preset_json_keeps_all()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-merge-json").ClickAsync();
        await Expect(Log("demo-pre-json")).ToHaveAttributeAsync("data-detail", "{\"id\":7}");
        await Expect(Log("demo-added")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Duplicate_name_first_wins()
    {
        await GoAsync("/Triggers");
        await T("trg-btn-first-wins").ClickAsync();
        await Expect(Log("demo-dup")).ToHaveAttributeAsync("data-detail", "{\"v\":\"first\"}");
        await Expect(Log("demo-dup")).ToHaveCountAsync(1);
    }
}
