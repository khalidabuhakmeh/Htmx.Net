using System.Threading.Tasks;
using Htmx.Tests.RazorProbe;
using Microsoft.AspNetCore.TestHost;
using VerifyXunit;
using Xunit;

namespace Htmx.Tests.TagHelpers;

/// <summary>
/// Executes the tag helpers under REAL Razor compilation + MVC/Razor Pages URL generation.
/// This is the proof that colon (":inherited") attribute names work.
/// </summary>
public class RazorExecutionTests
{
    private static async Task<string> RenderAsync()
    {
        using var host = await RazorProbeHost.StartAsync();
        var client = host.GetTestClient();
        var html = await client.GetStringAsync("/Probe/Show");
        return html.Replace("\r\n", "\n");
    }

    [Fact]
    public async Task Snapshot_full_matrix_rendered_by_razor()
    {
        var html = await RenderAsync();
        await Verifier.Verify(html);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("post")]
    [InlineData("put")]
    [InlineData("patch")]
    [InlineData("delete")]
    public async Task Colon_and_data_spellings_are_written_in_authors_spelling(string verb)
    {
        var html = await RenderAsync();
        Assert.Contains($"id=\"{verb}-x-page\" hx-{verb}=\"/Things\"", html);
        Assert.Contains($"id=\"{verb}-x_inherited-page\" hx-{verb}:inherited=\"/Things\"", html);
        Assert.Contains($"id=\"{verb}-data-hx-x-page\" data-hx-{verb}=\"/Things\"", html);
        Assert.Contains($"id=\"{verb}-data-hx-x_inherited-page\" data-hx-{verb}:inherited=\"/Things\"", html);
        Assert.Contains($"hx-{verb}:inherited=\"/named\"", html);
        Assert.Contains($"hx-{verb}:inherited=\"/named/7\"", html);
    }

    [Fact]
    public async Task Valueless_verb_without_binding_is_untouched()
    {
        var html = await RenderAsync();
        // htmx 4: a valueless hx-get is valid (requests the current URL); Razor renders it minimized (no value)
        Assert.Contains("<div id=\"valueless-no-binding\" hx-get></div>", html);
        Assert.Contains("<div id=\"valueless-no-binding-inh\" hx-post:inherited></div>", html);
    }

    [Fact]
    public async Task Existing_value_is_kept()
    {
        var html = await RenderAsync();
        Assert.Contains("id=\"existing\" hx-get=\"/keep\"", html);
    }

    [Fact]
    public async Task Headers_tag_helper_runs_under_razor()
    {
        var html = await RenderAsync();
        Assert.Contains("<div id=\"headers\" hx-headers:inherited='{\"x-a\":\"1\"}'", html);
        Assert.Contains("<div id=\"headers-plain\" hx-headers='{\"x-a\":\"1\"}'", html);
        Assert.Contains("hx-headers='{\"x-b\":\"2\",\"x-a\":\"1\"}'", html);
    }

    [Fact]
    public async Task Config_meta_runs_under_razor()
    {
        var html = await RenderAsync();
        Assert.Contains("content='{\"transitions\":true,\"defaultSwap\":\"outerHTML\"}'", html);
        Assert.True(html.Contains("hxn-page=") == false);
    }
}
