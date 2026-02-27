using Htmx.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;
using Xunit.Abstractions;
using static Htmx.HtmxResponseHeaders;

#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable CS0618 // Type or member is obsolete

namespace Htmx.Tests;

public class HtmxHttpResponseExtensionsTests(ITestOutputHelper output)
{
    private HttpResponse Response { get; } = new FakeHttpResponse();
    private IHeaderDictionary Headers => Response.Headers;

    [Fact]
    public void Can_set_trigger()
    {
        const string expected = "cool";
        Response.Htmx(h => {
            h.Trigger(expected);
        });

        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_add_single_trigger()
    {
        const string expected = "cool";
        Response.Htmx(h =>
        {
            h.WithTrigger(expected);
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_add_single_trigger_at_timings()
    {
        const string expected = "cool";
        Response.Htmx(h =>
        {
            h.WithTrigger(expected)
                .WithTrigger(expected, timing: HtmxTriggerTiming.AfterSettle)
                .WithTrigger(expected, timing: HtmxTriggerTiming.AfterSwap);
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.True(Headers.ContainsKey(Keys.TriggerAfterSettle));
        Assert.True(Headers.ContainsKey(Keys.TriggerAfterSwap));
        Assert.Equal(expected, Headers[Keys.Trigger]);
        Assert.Equal(expected, Headers[Keys.TriggerAfterSettle]);
        Assert.Equal(expected, Headers[Keys.TriggerAfterSwap]);
    }
        
    [Fact]
    public void Can_add_multiple_triggers()
    {
        const string expected = @"{""cool"":"""",""neat"":""""}";
        Response.Htmx(h =>
        {
            h.WithTrigger("cool");
            h.WithTrigger("neat");
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_add_single_trigger_with_basic_detail()
    {
        const string expected = @"{""cool"":""magic""}";
        Response.Htmx(h =>
        {
            h.WithTrigger("cool", "magic");
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }
        
    [Fact]
    public void Can_add_single_trigger_with_detail()
    {
        const string expected = @"{""cool"":{""magic"":42}}";
        Response.Htmx(h =>
        {
            h.WithTrigger("cool", new { magic = 42 });
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }
        
    [Fact]
    public void Can_add_multiple_triggers_with_detail()
    {
        const string expected = @"{""cool"":{""magic"":""something""},""neat"":{""moremagic"":false}}";
        Response.Htmx(h =>
        {
            h.WithTrigger("cool", new { magic = "something" });
            h.WithTrigger("neat", new { moremagic = false });
        });
            
        Assert.True(Headers.ContainsKey(Keys.Trigger));
        Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_use_existing_trigger_with_trigger()
    {
	    const string expected = @"{""neat"":"""",""cool"":""""}";

	    Response.Htmx(h => h.WithTrigger("cool"));
	    Response.Htmx(h => h.WithTrigger("neat"));

	    Assert.True(Headers.ContainsKey(Keys.Trigger));
	    Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_use_existing_trigger_with_multiple_triggers_with_detail()
    {
	    const string expected = @"{""cool"":{""magic"":""something""},""neat"":{""moremagic"":false},""wow"":""""}";

	    Response.Htmx(h => h.WithTrigger("wow"));
	    Response.Htmx(h =>
	    {
		    h.WithTrigger("cool", new { magic = "something" });
		    h.WithTrigger("neat", new { moremagic = false });
	    });

	    Assert.True(Headers.ContainsKey(Keys.Trigger));
	    Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_use_existing_trigger_with_detail_with_multiple_triggers_with_detail()
    {
	    const string expected = @"{""cool"":{""magic"":""something""},""neat"":{""moremagic"":false},""wow"":{""display"":true}}";

	    Response.Htmx(h => h.WithTrigger("wow", new { display = true }));
	    Response.Htmx(h =>
	    {
		    h.WithTrigger("cool", new { magic = "something" });
		    h.WithTrigger("neat", new { moremagic = false });
	    });

	    Assert.True(Headers.ContainsKey(Keys.Trigger));
	    Assert.Equal(expected, Headers[Keys.Trigger]);
    }

    [Fact]
    public void Can_set_redirect()
    {
        const string expected = "cool";
        Response.Htmx(h => {
            h.Redirect(expected);
        });

        Assert.True(Headers.ContainsKey(Keys.Redirect));
        Assert.Equal(expected, Headers[Keys.Redirect]);
    }

    [Fact]
    public void Can_set_refresh()
    {
        Response.Htmx(h => {
            h.Refresh();
        });

        Assert.True(Headers.ContainsKey(Keys.Refresh));
    }

    [Fact]
    public void Can_set_trigger_after_swap()
    {
        const string expected = "cool";
        Response.Htmx(h => {
            h.TriggerAfterSwap(expected);
        });

        Assert.True(Headers.ContainsKey(Keys.TriggerAfterSwap));
        Assert.Equal(expected, Headers[Keys.TriggerAfterSwap]);
    }

    [Fact]
    public void Can_set_trigger_after_settle()
    {
        const string expected = "cool";
        Response.Htmx(h => {
            h.TriggerAfterSettle(expected);
        });

        Assert.True(Headers.ContainsKey(Keys.TriggerAfterSettle));
        Assert.Equal(expected, Headers[Keys.TriggerAfterSettle]);
    }
        
    [Fact]
    public void Can_retarget()
    {
        const string expected = "#header";
        Response.Htmx(h => {
            h.Retarget(expected);
        });

        Assert.True(Headers.ContainsKey(Keys.Retarget));
        Assert.Equal(expected, Headers[Keys.Retarget]);
    }

    [Fact]
    public void Can_prevent_push()
    {
        Response.Htmx(h => h.PreventPush());
        Assert.True(Headers.ContainsKey(Keys.PushUrl));
        Assert.Equal("false", Headers[Keys.PushUrl]);
    }

    [Fact]
    public void Can_prevent_replace()
    {
        Response.Htmx(h => h.PreventReplace());
        Assert.True(Headers.ContainsKey(Keys.ReplaceUrl));
        Assert.Equal("false", Headers[Keys.ReplaceUrl]);
    }

    [Fact]
    public void Can_set_location_with_object()
    {
        const string expected = @"{""path"":""/new-location"",""target"":""#target"",""select"":""#content""}";
        Response.Htmx(h => {
            h.Location(new HtmxLocation
            {
                Path = "/new-location",
                Target = "#target",
                Select = "#content"
            });
        });

        Assert.True(Headers.ContainsKey(Keys.Location));
        Assert.Equal(expected, Headers[Keys.Location]);
    }

    [Fact]
    public void Can_add_vary()
    {
        Response.Htmx(h => h.WithVary());
        Assert.True(Headers.ContainsKey("Vary"));
        Assert.Equal("HX-Request", Headers["Vary"]);
    }

    [Fact]
    public void Can_stop_polling()
    {
        Response.StopPolling();
        Assert.Equal(286, Response.StatusCode);
    }

}
