using Microsoft.AspNetCore.Http;
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