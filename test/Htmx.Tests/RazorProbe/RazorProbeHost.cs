using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Htmx.Tests.RazorProbe;

/// <summary>Minimal MVC + Razor Pages host used to execute the tag helpers under real Razor.</summary>
public static class RazorProbeHost
{
    public static async Task<IHost> StartAsync()
    {
        var host = new HostBuilder()
            .ConfigureWebHost(web => web
                .UseTestServer()
                .ConfigureServices(s =>
                {
                    s.AddRouting();
                    s.AddControllersWithViews().AddApplicationPart(typeof(RazorProbeHost).Assembly);
                    s.AddRazorPages();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(e =>
                    {
                        e.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                        e.MapRazorPages();
                    });
                }))
            .Build();
        await host.StartAsync();
        return host;
    }
}
