using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Htmx.Tests.TagHelpers;

/// <summary>
/// IHtmlGenerator stand-in. Link generation methods return an anchor whose href encodes the inputs,
/// e.g. <c>page:/Index?handler=Edit</c>, <c>action:Area/Controller/Action</c>, <c>route:name?id=7</c>.
/// </summary>
public class FakeHtmlGenerator : DispatchProxy
{
    public static IHtmlGenerator Create() => DispatchProxy.Create<IHtmlGenerator, FakeHtmlGenerator>();

    private static string Rv(object? values)
    {
        if (values is not Microsoft.AspNetCore.Routing.RouteValueDictionary d || d.Count == 0) return "";
        return "?" + string.Join("&", d.OrderBy(k => k.Key).Select(k => $"{k.Key}={k.Value}"));
    }

    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        var a = args!;
        var href = method!.Name switch
        {
            "GeneratePageLink" => $"page:{a[2]}" + (a[3] is { } h ? $"#handler={h}" : "") + Rv(a[7]),
            "GenerateRouteLink" => $"route:{a[2]}" + Rv(a[6]),
            "GenerateActionLink" => $"action:{a[2]}/{a[3]}" + Rv(a[7]),
            _ => throw new System.NotSupportedException(method.Name)
        };
        var tb = new TagBuilder("a");
        tb.Attributes["href"] = href;
        return tb;
    }
}
