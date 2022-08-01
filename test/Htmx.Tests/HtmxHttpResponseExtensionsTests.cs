using System;
using Microsoft.AspNetCore.Http;
using NuGet.Frameworks;
using Xunit;
using Xunit.Abstractions;
using static Htmx.HtmxResponseHeaders;

namespace Htmx.Tests
{
    public class HtmxHttpResponseExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public HtmxHttpResponseExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
            Response = new FakeHttpResponse();
        }

        private HttpResponse Response { get; set; }
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
        public void Cant_use_legacy_trigger_and_with_trigger()
        {
            Response.Htmx(h => h.Trigger("cool"));
            Assert.Throws<Exception>(() => Response.Htmx(h => h.WithTrigger("neat")));
        }
        
        [Fact]
        public void Cant_use_legacy_triggeraftersettle_and_with_trigger()
        {
            Response.Htmx(h => h.TriggerAfterSettle("cool"));
            Assert.Throws<Exception>(() => Response.Htmx(h => h.WithTrigger("neat", timing: HtmxTriggerTiming.AfterSettle)));
        }
        
        [Fact]
        public void Cant_use_legacy_triggerafterswap_and_with_trigger()
        {
            Response.Htmx(h => h.TriggerAfterSwap("cool"));
            Assert.Throws<Exception>(() => Response.Htmx(h => h.WithTrigger("neat", timing: HtmxTriggerTiming.AfterSwap)));
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

    }
}