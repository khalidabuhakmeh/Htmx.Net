using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Htmx.TagHelpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using VerifyXunit;
using Xunit;

namespace Htmx.Tests.TagHelpers;

public class HtmxConfigMetaTagHelperTests
{
    // Keys from docs/src/content/docs/reference/htmx4-facts.md section 4 (+ antiForgery, an Htmx.Net extension)
    private static readonly string[] Htmx4Keys =
    {
        "logAll", "prefix", "metaCharacter", "transitions", "history", "mode", "defaultSwap",
        "allowEmptySwapAfterOOB", "defaultFocusScroll", "defaultSettleDelay", "indicatorClass", "requestClass",
        "includeIndicatorCSS", "defaultTimeout", "extensions", "morphIgnore", "morphSkip", "morphSkipChildren",
        "morphScanLimit", "noSwap", "implicitInheritance", "inlineScriptNonce"
    };

    private class FakeAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) =>
            new("req-token+/=", "cookie-token", "__RequestVerificationToken", "RequestVerificationToken");
        public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => throw new NotSupportedException();
        public Task<bool> IsRequestValidAsync(HttpContext httpContext) => throw new NotSupportedException();
        public Task ValidateRequestAsync(HttpContext httpContext) => throw new NotSupportedException();
        public void SetCookieTokenAndHeader(HttpContext httpContext) => throw new NotSupportedException();
    }

    private static string Run(HtmxConfigMetaTagHelper helper, bool withAntiforgery = false)
    {
        var services = new ServiceCollection();
        if (withAntiforgery) services.AddSingleton<IAntiforgery>(new FakeAntiforgery());
        var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        helper.ViewContext = new ViewContext { HttpContext = http };

        var ctx = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput("meta", new TagHelperAttributeList { { "name", "htmx-config" } },
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        helper.Process(ctx, output);

        var content = output.Attributes["content"];
        Assert.Equal(HtmlAttributeValueStyle.SingleQuotes, content.ValueStyle);
        return content.Value.ToString()!;
    }

    private static HtmxConfigMetaTagHelper Full() => new()
    {
        LogAll = true, Prefix = "data-x-", MetaCharacter = "-", Transitions = true, History = "reload",
        Mode = "cors", DefaultSwap = "outerHTML", AllowEmptySwapAfterOOB = true, DefaultFocusScroll = true,
        DefaultSettleDelay = 20, IndicatorClass = "ind", RequestClass = "req", IncludeIndicatorCSS = false,
        DefaultTimeout = 5000, Extensions = "sse, ws", MorphIgnore = new[] { "a", "b" }, MorphSkip = "[skip]",
        MorphSkipChildren = "[skipc]", MorphScanLimit = 5, NoSwap = new object[] { 204, "5xx" },
        ImplicitInheritance = true, InlineScriptNonce = "abc123"
    };

    [Fact]
    public Task Snapshot_empty_config() => Verifier.Verify(Run(new HtmxConfigMetaTagHelper()));

    [Fact]
    public Task Snapshot_every_property_set() => Verifier.Verify(Run(Full()));

    [Fact]
    public Task Snapshot_antiforgery_only() =>
        Verifier.Verify(Run(new HtmxConfigMetaTagHelper { IncludeAntiForgery = true }, withAntiforgery: true));

    [Fact]
    public void Empty_config_emits_no_keys_and_defaults_do_not_leak()
    {
        // latent bug fix: unset bools must not serialize as false
        Assert.Equal("{}", Run(new HtmxConfigMetaTagHelper()));
    }

    [Fact]
    public void Explicit_false_and_zero_are_serialized()
    {
        var json = Run(new HtmxConfigMetaTagHelper { Transitions = false, DefaultSettleDelay = 0 });
        Assert.Equal("{\"transitions\":false,\"defaultSettleDelay\":0}", json);
    }

    [Fact]
    public void Every_serialized_key_is_an_htmx4_key_or_antiForgery()
    {
        var json = Run(Full());
        var withAf = Run(new HtmxConfigMetaTagHelper { IncludeAntiForgery = true }, true);
        var allowed = Htmx4Keys.Append("antiForgery").ToHashSet();

        var keys = JsonDocument.Parse(json).RootElement.EnumerateObject().Select(p => p.Name).ToList();
        Assert.All(keys, k => Assert.Contains(k, allowed));
        Assert.Equal(Htmx4Keys.OrderBy(k => k), keys.OrderBy(k => k)); // nothing missing either
        Assert.All(JsonDocument.Parse(withAf).RootElement.EnumerateObject(), p => Assert.Contains(p.Name, allowed));
    }

    [Fact]
    public void No_removed_htmx2_keys_are_emitted()
    {
        var json = Run(Full());
        foreach (var old in new[] { "defaultSwapStyle", "globalViewTransitions", "historyEnabled", "timeout" })
            Assert.DoesNotContain($"\"{old}\"", json);
    }

    [Fact]
    public void Antiforgery_requested_without_service_emits_nothing()
    {
        Assert.Equal("{}", Run(new HtmxConfigMetaTagHelper { IncludeAntiForgery = true }, withAntiforgery: false));
    }

    [Fact]
    public void Existing_content_attribute_is_replaced()
    {
        var helper = new HtmxConfigMetaTagHelper { ViewContext = new ViewContext { HttpContext = new DefaultHttpContext() } };
        var output = new TagHelperOutput("meta", new TagHelperAttributeList { { "content", "old" } },
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        helper.Process(new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id"), output);
        Assert.Equal("{}", Assert.Single(output.Attributes).Value.ToString());
    }
}
