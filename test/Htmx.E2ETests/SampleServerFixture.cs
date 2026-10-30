using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Htmx.E2ETests;

/// <summary>
/// Launches samples/Htmx4.Sample as a child process (real Kestrel on a free loopback port) and installs
/// Chromium once. The sample is built by the ProjectReference in the csproj, so --no-build is safe.
/// </summary>
public sealed class SampleServerFixture : IAsyncLifetime
{
    private static readonly object InstallLock = new();
    private static bool _installed;
    private Process? _process;

    public string BaseUrl { get; private set; } = "";

    public async Task InitializeAsync()
    {
        EnsureChromium();

        var repoRoot = FindRepoRoot();
        var project = Path.Combine(repoRoot, "samples", "Htmx4.Sample", "Htmx4.Sample.csproj");
        // .../bin/<Configuration>/net10.0/ -> Configuration
        var config = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var port = FreePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(project)!
        };
        foreach (var a in new[] { "run", "--no-build", "--no-launch-profile", "-c", config, "--project", project, "--urls", BaseUrl })
            psi.ArgumentList.Add(a);
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["DOTNET_NOLOGO"] = "1";

        _process = Process.Start(psi)!;
        var output = new System.Text.StringBuilder();
        _process.OutputDataReceived += (_, e) => { if (e.Data is not null) lock (output) output.AppendLine(e.Data); };
        _process.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (output) output.AppendLine(e.Data); };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        using var http = new HttpClient();
        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            if (_process.HasExited)
                throw new InvalidOperationException($"Sample exited early ({_process.ExitCode}):\n{output}");
            try
            {
                var r = await http.GetAsync(BaseUrl + "/");
                if (r.StatusCode == HttpStatusCode.OK) return;
            }
            catch (HttpRequestException) { }
            await Task.Delay(200);
        }
        throw new TimeoutException($"Sample did not start at {BaseUrl}:\n{output}");
    }

    public Task DisposeAsync()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(5000);
            }
        }
        catch (InvalidOperationException) { }
        _process?.Dispose();
        return Task.CompletedTask;
    }

    private static void EnsureChromium()
    {
        lock (InstallLock)
        {
            if (_installed) return;
            var code = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (code != 0)
                throw new InvalidOperationException(
                    "Playwright Chromium install failed. Run: pwsh test/Htmx.E2ETests/bin/<cfg>/net10.0/playwright.ps1 install chromium");
            _installed = true;
        }
    }

    private static int FreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private static string FindRepoRoot()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "Htmx.sln")) && !Directory.Exists(Path.Combine(d.FullName, "samples", "Htmx4.Sample")))
            d = d.Parent;
        return d?.FullName ?? throw new DirectoryNotFoundException("Repo root not found");
    }
}
