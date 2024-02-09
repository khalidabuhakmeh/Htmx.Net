using Htmx.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Htmx.Tests;

public class HtmxHttpRequestExtensionsTests
{
    public HtmxHttpRequestExtensionsTests()
    {
        Request = new FakeHttpRequest();
        Request.Headers.Add(HtmxRequestHeaders.Keys.Request, "true");
        Request.Headers.Add(HtmxRequestHeaders.Keys.HistoryRestoreRequest, "true");
        Request.Headers.Add(HtmxRequestHeaders.Keys.Trigger, "trigger");
        Request.Headers.Add(HtmxRequestHeaders.Keys.Prompt, "prompt");
        Request.Headers.Add(HtmxRequestHeaders.Keys.CurrentUrl, "/");
        Request.Headers.Add(HtmxRequestHeaders.Keys.Target, "123");
        Request.Headers.Add(HtmxRequestHeaders.Keys.TriggerName, "trigger-name");
        Request.Headers.Add(HtmxRequestHeaders.Keys.Boosted, "true");
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
        Assert.Equal("prompt", values.Prompt);
    }

    [Fact]
    public void Can_get_target()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("123", values.Target);
    }

    [Fact]
    public void Can_get_trigger_name()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("trigger-name", values.TriggerName);
    }

    [Fact]
    public void Can_get_trigger()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("trigger", values.Trigger);
    }

    [Fact]
    public void Can_get_current_url_of_browser()
    {
        Request.IsHtmx(out var values);

        Assert.NotNull(values);
        Assert.Equal("/", values.CurrentUrl);
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
        request.Headers.Add(HtmxRequestHeaders.Keys.Request, "true");
        
        Assert.True(request.IsHtmxNonBoosted());
    }
    
    [Fact]
    public void Can_get_values_when_request_is_htmx_non_boosted()
    {
        var request = new FakeHttpRequest();
        request.Headers.Add(HtmxRequestHeaders.Keys.Request, "true");
        request.Headers.Add(HtmxRequestHeaders.Keys.HistoryRestoreRequest, "true");
        request.Headers.Add(HtmxRequestHeaders.Keys.Trigger, "trigger");
        request.Headers.Add(HtmxRequestHeaders.Keys.Prompt, "prompt");
        request.Headers.Add(HtmxRequestHeaders.Keys.CurrentUrl, "/");
        request.Headers.Add(HtmxRequestHeaders.Keys.Target, "123");
        request.Headers.Add(HtmxRequestHeaders.Keys.TriggerName, "trigger-name");
        request.Headers.Add(HtmxRequestHeaders.Keys.Boosted, "false");

        var isHtmxNonBoosted = request.IsHtmxNonBoosted(out var values);
        Assert.True(isHtmxNonBoosted);
        Assert.NotNull(values);
        Assert.True(values.HistoryRestoreRequest);
        Assert.Equal("trigger", values.Trigger);
        Assert.Equal("prompt", values.Prompt);
        Assert.Equal("/", values.CurrentUrl);
        Assert.Equal("123", values.Target);
        Assert.Equal("trigger-name", values.TriggerName);
    }
}