using Xunit;

namespace Htmx.E2ETests;

public class SmokeTests
{
    // TODO(D4): replace with real Playwright tests using SampleServerFixture.
    [Trait("Category", "E2E")]
    [Fact]
    public void Placeholder() => Assert.True(true);
}
