using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Htmx.Tests;

public class VerifyToolingSmokeTest
{
    [Fact]
    public Task SnapshotString() => Verifier.Verify("hello htmx 4");
}
