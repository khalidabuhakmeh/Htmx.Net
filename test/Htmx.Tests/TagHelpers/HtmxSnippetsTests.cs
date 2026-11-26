using Htmx.TagHelpers;
using Xunit;

namespace Htmx.Tests.TagHelpers;

public class HtmxSnippetsTests
{
    public static TheoryData<string> Snippets => new()
    {
        HtmxSnippets.AntiforgeryJavaScript,
        HtmxSnippets.AntiforgeryJavaScriptMinified
    };

    [Theory]
    [MemberData(nameof(Snippets))]
    public void Antiforgery_snippet_has_security_guards(string script)
    {
        // same-origin guard resolved against the document base URI
        Assert.Contains("document.baseURI", script);
        Assert.Contains("window.location.origin", script);
        // never put the token in a URL in form-field mode
        Assert.Contains("methodsThatUseUrlParams", script);
        // idempotence guard lives on window, not document.body
        Assert.Contains("__htmxAntiforgery", script);
        // safe methods are skipped
        Assert.Contains("\"HEAD\"", script);
        Assert.Contains("\"OPTIONS\"", script);
        Assert.Contains("\"TRACE\"", script);
        Assert.Contains("htmx:configRequest", script);
    }

    [Theory]
    [MemberData(nameof(Snippets))]
    public void Antiforgery_snippet_does_not_use_removed_patterns(string script)
    {
        // no token refresh by parsing response text (token-config poisoning)
        Assert.DoesNotContain("DOMParser", script);
        Assert.DoesNotContain("responseText", script);
        Assert.DoesNotContain("document.body.attributes", script);
    }

    [Fact]
    public void GetAntiforgeryJavaScript_returns_requested_variant()
    {
        Assert.Same(HtmxSnippets.AntiforgeryJavaScriptMinified, HtmxSnippets.GetAntiforgeryJavaScript(true));
        Assert.Same(HtmxSnippets.AntiforgeryJavaScript, HtmxSnippets.GetAntiforgeryJavaScript(false));
    }
}
