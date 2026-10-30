using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace Htmx.E2ETests;

/// <summary>Base for sample-page tests: navigates to a page and fails the test on any browser console error or page error.</summary>
[Trait("Category", "E2E")]
public abstract class E2EBase(SampleServerFixture server) : PageTest, IClassFixture<SampleServerFixture>
{
    private readonly List<string> _errors = [];

    /// <summary>Set by a test that intentionally provokes browser console errors (e.g. a fetch blocked by CORS/mode).</summary>
    protected bool ExpectedConsoleErrors { get; set; }

    protected string BaseUrl => server.BaseUrl;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Page.SetDefaultTimeout(10_000);
        SetDefaultExpectTimeout(10_000);
        Page.Console += (_, m) =>
        {
            // Browsers log intentional 4xx/5xx demo responses as "Failed to load resource"; not a script error.
            if (m.Type == "error" && !m.Text.StartsWith("Failed to load resource"))
                lock (_errors) _errors.Add(m.Text);
        };
        Page.PageError += (_, e) => { lock (_errors) _errors.Add("pageerror: " + e); };
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        lock (_errors)
            Assert.True(ExpectedConsoleErrors || _errors.Count == 0, "Browser console errors:\n" + string.Join("\n", _errors));
    }

    protected async Task GoAsync(string path)
    {
        await Page.GotoAsync(BaseUrl + path);
        // htmx is deferred; wait until it has loaded and initialized.
        await Page.WaitForFunctionAsync("() => typeof window.htmx !== 'undefined'");
    }

    protected ILocator T(string id) => Page.GetByTestId(id);
}
