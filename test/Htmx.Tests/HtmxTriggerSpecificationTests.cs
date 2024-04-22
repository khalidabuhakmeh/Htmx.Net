using System.Text.Json;
using System.Text.Json.Serialization;
using Htmx.TagHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Htmx.Tests;

public class HtmxTriggerSpecificationTests
{
    private readonly ITestOutputHelper outputHelper;

    public HtmxTriggerSpecificationTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }
    
    private const string Specifications =
        // lang=json
        """
         {
             "every 1s": [{"trigger": "every", "pollInterval": 1000}],
             "click": [{"trigger": "click"}],
             "customEvent": [{"trigger": "customEvent"}],
             "event changed": [{"trigger": "event", "changed": true}],
             "event once": [{"trigger": "event", "once": true}],
             "event delay:1s": [{"trigger": "event", "delay": 1000}],
             "event throttle:1s": [{"trigger": "event", "throttle": 1000}],
             "event delay:1s, foo": [{"trigger": "event", "delay": 1000}, {"trigger": "foo"}],
             "event throttle:1s, foo": [{"trigger": "event", "throttle": 1000}, {"trigger": "foo"}],
             "event changed once delay:1s": [{"trigger": "event", "changed": true, "once": true, "delay": 1000}],
             "event1,event2": [{"trigger": "event1"}, {"trigger": "event2"}],
             "event1, event2": [{"trigger": "event1"}, {"trigger": "event2"}],
             "event1 once, event2 changed": [{"trigger": "event1", "once": true}, {"trigger": "event2", "changed": true}]
         }
        """;

    [Fact]
    public void Can_Deserialize_HtmxTriggerSpecification()
    {
        var cache = JsonSerializer.Deserialize<HtmxTriggerSpecificationCache>(Specifications);

        Assert.NotNull(cache);
        Assert.Equal(13, cache!.Keys.Count);
    }

    [Fact]
    public void Can_Serialize_HtmxTriggerSpecification()
    {
        HtmxTriggerSpecificationCache cache = new()
        {
            { "every 1s", [new() { Trigger = "event1" }, new() { Trigger = "event2" }] }
        };

        var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        outputHelper.WriteLine(json);

        Assert.Contains("every 1s", json);
        Assert.Contains("\"trigger\":\"event1\"", json);
        Assert.Contains("\"trigger\":\"event2\"", json);
    }
}