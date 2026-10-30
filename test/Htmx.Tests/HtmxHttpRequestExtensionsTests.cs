using System.Linq;
using Htmx.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Htmx.Tests;

public class HtmxHttpRequestExtensionsTests
{
    public HtmxHttpRequestExtensionsTests()
    {
        Request = new FakeHttpRequest();
        Request.Headers.Append(HtmxRequestHeaders.Keys.Request, "true");
        Request.Headers.Append(HtmxRequestHeaders.Keys.HistoryRestoreRequest, "true");
        Request.Headers.Append(HtmxRequestHeaders.Keys.Source, "button#trigger");
        Request.Headers.Append(HtmxRequestHeaders.Keys.Prompt, "prompt");
        Request.Headers.Append(HtmxRequestHeaders.Keys.CurrentUrl, "/");
        Request.Headers.Append(HtmxRequestHeaders.Keys.Target, "123");
        Request.Headers.Append(HtmxRequestHeaders.Keys.Boosted, "true");
    }

    private HttpRequest Request { get; }

    [Fact]
    public void Is_htmx_request()
    {
        Assert.True(Request.IsHtmx());
    }

    [Fact]
    public void Can_get_Htmx__header_values()
    {
        var result = Request.IsHtmx(out var values);

        Assert.True(result);
        Assert.NotNull(values);
    }

    [Fact]
    public void Can_get_history_restore_request()
    {
        var result = Request.IsHtmxHistoryRestoreRequest();
        Assert.True(result);
    }

    [Fact]
    public void Can_get_prompt()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("prompt", values!.Prompt);
    }

    [Fact]
    public void Can_get_target()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("123", values!.Target);
    }

    [Fact]
    public void Can_get_current_url_of_browser()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("/", values!.CurrentUrl);
    }

    [Fact]
    public void Values_are_null_when_not_htmx_request()
    {
        var regularRequest = new FakeHttpRequest();
        regularRequest.IsHtmx(out var values);
        Assert.Null(values);
    }

    [Fact]
    public void Can_determine_request_is_boosted()
    {
        Assert.True(Request.IsHtmxBoosted());
    }
    
    [Fact]
    public void Can_determine_request_is_htmx_non_boosted()
    {
        var request = new FakeHttpRequest();
        request.Headers.Append(HtmxRequestHeaders.Keys.Request, "true");
        
        Assert.True(request.IsHtmxNonBoosted());
    }
    
    [Fact]
    public void Can_get_values_when_request_is_htmx_non_boosted()
    {
        var request = new FakeHttpRequest();
        request.Headers.Append(HtmxRequestHeaders.Keys.Request, "true");
        request.Headers.Append(HtmxRequestHeaders.Keys.HistoryRestoreRequest, "true");
        request.Headers.Append(HtmxRequestHeaders.Keys.Source, "button#trigger");
        request.Headers.Append(HtmxRequestHeaders.Keys.Prompt, "prompt");
        request.Headers.Append(HtmxRequestHeaders.Keys.CurrentUrl, "/");
        request.Headers.Append(HtmxRequestHeaders.Keys.Target, "123");
        request.Headers.Append(HtmxRequestHeaders.Keys.Boosted, "false");

        var isHtmxNonBoosted = request.IsHtmxNonBoosted(out var values);
        Assert.True(isHtmxNonBoosted);
        Assert.NotNull(values);
        Assert.True(values!.HistoryRestoreRequest);
        Assert.Equal("button#trigger", values.Source);
        Assert.Equal("prompt", values.Prompt);
        Assert.Equal("/", values.CurrentUrl);
        Assert.Equal("123", values.Target);
    }

    private static FakeHttpRequest Make(params (string, string)[] headers)
    {
        var r = new FakeHttpRequest();
        foreach (var (k, v) in headers) r.Headers.Append(k, v);
        return r;
    }

    [Theory]
    [InlineData("full", HtmxRequestType.Full)]
    [InlineData("partial", HtmxRequestType.Partial)]
    [InlineData("PARTIAL", HtmxRequestType.Partial)]
    [InlineData("garbage", HtmxRequestType.Unknown)]
    public void RequestType_parses(string value, HtmxRequestType expected)
    {
        var r = Make((HtmxRequestHeaders.Keys.Request, "true"), (HtmxRequestHeaders.Keys.RequestType, value));
        r.IsHtmx(out var v);
        Assert.Equal(expected, v!.RequestType);
    }

    [Fact]
    public void RequestType_missing_is_unknown()
    {
        var r = Make((HtmxRequestHeaders.Keys.Request, "true"));
        r.IsHtmx(out var v);
        Assert.Equal(HtmxRequestType.Unknown, v!.RequestType);
    }

    [Fact]
    public void Source_and_target_elements_parse()
    {
        var r = Make((HtmxRequestHeaders.Keys.Request, "true"),
            (HtmxRequestHeaders.Keys.Source, "button#submit"),
            (HtmxRequestHeaders.Keys.Target, "div#out"));
        r.IsHtmx(out var v);
        Assert.Equal("button#submit", v!.Source);
        Assert.Equal("button", v.SourceElement!.Value.TagName);
        Assert.Equal("submit", v.SourceElement!.Value.Id);
        Assert.Equal("div", v.TargetElement!.Value.TagName);
        Assert.Equal("out", v.TargetElement!.Value.Id);
    }

    [Fact]
    public void Source_and_target_elements_null_when_absent()
    {
        var r = Make((HtmxRequestHeaders.Keys.Request, "true"));
        r.IsHtmx(out var v);
        Assert.Null(v!.SourceElement);
        Assert.Null(v.TargetElement);
    }

    [Theory]
    [InlineData(null, false, false)]
    [InlineData("full", false, true)]
    [InlineData("partial", true, false)]
    [InlineData("garbage", false, false)]
    public void Partial_and_full_helpers(string? type, bool partial, bool full)
    {
        var r = type is null ? Make() : Make((HtmxRequestHeaders.Keys.RequestType, type));
        Assert.Equal(partial, r.IsHtmxPartial());
        Assert.Equal(full, r.IsHtmxFullRequest());
    }

    [Fact]
    public void Partial_and_full_helpers_null_request()
    {
        HttpRequest? r = null;
        Assert.False(r.IsHtmxPartial());
        Assert.False(r.IsHtmxFullRequest());
    }

    [Fact]
    public void Malformed_boolean_header_returns_false_and_does_not_throw()
    {
        var r = Make((HtmxRequestHeaders.Keys.Request, "true"), (HtmxRequestHeaders.Keys.Boosted, "yes"));
        var ex = Record.Exception(() => r.IsHtmxBoosted());
        Assert.Null(ex);
        Assert.False(r.IsHtmxBoosted());
        r.IsHtmx(out var v);
        Assert.False(v!.Boosted);
    }

    [Fact]
    public void Request_Keys_All_matches_htmx4_contract()
    {
        Assert.Equal(new[]
        {
            "HX-Request", "HX-Request-Type", "HX-Current-URL", "HX-Source", "HX-Target",
            "HX-Boosted", "HX-History-Restore-Request", "HX-Prompt"
        }.OrderBy(x => x), HtmxRequestHeaders.Keys.All.OrderBy(x => x));
        Assert.DoesNotContain(HtmxRequestHeaders.Keys.All, k => k is "HX-Trigger" or "HX-Trigger-Name");
    }
}
