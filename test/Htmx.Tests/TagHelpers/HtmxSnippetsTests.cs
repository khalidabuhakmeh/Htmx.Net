using System.Text.RegularExpressions;
using Htmx.TagHelpers;
using Xunit;

namespace Htmx.Tests.TagHelpers;

public class HtmxSnippetsTests
{
    public static TheoryData<string, string> Scripts => new()
    {
        { "full", HtmxSnippets.AntiforgeryJavaScript },
        { "minified", HtmxSnippets.AntiforgeryJavaScriptMinified },
    };

    [Theory]
    [MemberData(nameof(Scripts))]
    public void Resource_loads_and_uses_htmx4_event(string _, string script)
    {
        Assert.False(string.IsNullOrWhiteSpace(script));
        Assert.Contains("htmx:config:request", script);
    }

    [Theory]
    [MemberData(nameof(Scripts))]
    public void Has_same_origin_guard(string _, string script) =>
        Assert.Matches(new Regex("origin", RegexOptions.IgnoreCase), script);

    [Theory]
    [MemberData(nameof(Scripts))]
    public void Has_no_htmx2_apis(string _, string script)
    {
        Assert.DoesNotContain("configRequest", script);
        Assert.DoesNotContain("xhr", script);
        Assert.DoesNotContain("afterOnLoad", script);
        Assert.DoesNotContain(".verb", script);
    }

    [Fact]
    public void Minified_is_smaller() =>
        Assert.True(HtmxSnippets.AntiforgeryJavaScriptMinified.Length < HtmxSnippets.AntiforgeryJavaScript.Length);

    [Fact]
    public void GetAntiforgeryJavaScript_selects_variant()
    {
        Assert.Same(HtmxSnippets.AntiforgeryJavaScriptMinified, HtmxSnippets.GetAntiforgeryJavaScript(true));
        Assert.Same(HtmxSnippets.AntiforgeryJavaScript, HtmxSnippets.GetAntiforgeryJavaScript(false));
    }
}
