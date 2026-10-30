using Xunit;

namespace Htmx.Tests;

public class HtmxElementReferenceTests
{
    [Fact]
    public void Parses_tag_and_id()
    {
        Assert.True(HtmxElementReference.TryParse("button#submit", out var r));
        Assert.Equal("button", r.TagName);
        Assert.Equal("submit", r.Id);
        Assert.Equal("button#submit", r.Raw);
    }

    [Fact]
    public void Parses_tag_without_id()
    {
        Assert.True(HtmxElementReference.TryParse("div", out var r));
        Assert.Equal("div", r.TagName);
        Assert.Null(r.Id);
        Assert.Null(r.DecodedId);
    }

    [Fact]
    public void Parses_id_without_tag()
    {
        Assert.True(HtmxElementReference.TryParse("#x", out var r));
        Assert.Equal("", r.TagName);
        Assert.Equal("x", r.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_or_null_fails(string? value)
    {
        Assert.False(HtmxElementReference.TryParse(value, out var r));
        Assert.Equal(default, r);
    }

    [Fact]
    public void Id_with_special_characters_is_preserved()
    {
        Assert.True(HtmxElementReference.TryParse("a#b:c.d", out var r));
        Assert.Equal("a", r.TagName);
        Assert.Equal("b:c.d", r.Id);
    }

    [Fact]
    public void Splits_on_first_hash_only()
    {
        Assert.True(HtmxElementReference.TryParse("a#b#c", out var r));
        Assert.Equal("a", r.TagName);
        Assert.Equal("b#c", r.Id);
    }

    [Fact]
    public void Preserves_case()
    {
        Assert.True(HtmxElementReference.TryParse("DIV#MyId", out var r));
        Assert.Equal("DIV", r.TagName);
        Assert.Equal("MyId", r.Id);
    }

    [Fact]
    public void Decodes_uri_encoded_id()
    {
        Assert.True(HtmxElementReference.TryParse("li#item%20one", out var r));
        Assert.Equal("item%20one", r.Id);
        Assert.Equal("item one", r.DecodedId);
    }
}
