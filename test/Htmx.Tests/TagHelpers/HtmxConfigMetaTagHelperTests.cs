using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Htmx.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Htmx.Tests.TagHelpers;

public class HtmxConfigMetaTagHelperTests
{
    private static JsonObject Render(HtmxConfigMetaTagHelper tagHelper)
    {
        var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput("meta", new TagHelperAttributeList(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        tagHelper.Process(context, output);

        Assert.True(output.Attributes.TryGetAttribute("content", out var content));
        var json = ((HtmlString)content.Value).Value!;
        return JsonNode.Parse(json)!.AsObject();
    }

    [Fact]
    public void SelfRequestsOnly_is_omitted_when_not_set_so_htmx_default_applies()
    {
        var config = Render(new HtmxConfigMetaTagHelper());

        Assert.False(config.ContainsKey("selfRequestsOnly"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SelfRequestsOnly_is_emitted_when_set(bool value)
    {
        var config = Render(new HtmxConfigMetaTagHelper { SelfRequestsOnly = value });

        Assert.Equal(value, config["selfRequestsOnly"]!.GetValue<bool>());
    }
}
