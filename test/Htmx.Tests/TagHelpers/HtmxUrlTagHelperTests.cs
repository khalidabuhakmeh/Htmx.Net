using System;
using System.Collections.Generic;
using System.Linq;
using Htmx.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Htmx.Tests.TagHelpers;

public class HtmxUrlTagHelperTests
{
    private static readonly string[] Verbs = { "get", "post", "put", "patch", "delete" };

    public static IEnumerable<object[]> Matrix()
    {
        foreach (var verb in Verbs)
        foreach (var spelling in new[] { "hx-{0}", "hx-{0}:inherited", "data-hx-{0}", "data-hx-{0}:inherited" })
        foreach (var binding in new[] { "page", "page+handler", "action", "action+area", "route", "route+values" })
            yield return new object[] { string.Format(spelling, verb), binding };
    }

    private static HtmxUrlTagHelper Bind(string binding)
    {
        var t = new HtmxUrlTagHelper(FakeHtmlGenerator.Create());
        switch (binding)
        {
            case "page": t.Page = "/Index"; break;
            case "page+handler": t.Page = "/Index"; t.PageHandler = "Edit"; break;
            case "action": t.Controller = "Home"; t.Action = "Index"; break;
            case "action+area": t.Controller = "Home"; t.Action = "Index"; t.Area = "Admin"; break;
            case "route": t.Route = "named"; break;
            case "route+values": t.Route = "named"; t.RouteValues!["id"] = "7"; break;
        }
        return t;
    }

    private static string Expected(string binding) => binding switch
    {
        "page" => "page:/Index",
        "page+handler" => "page:/Index#handler=Edit",
        "action" => "action:Index/Home",
        "action+area" => "action:Index/Home?area=Admin",
        "route" => "route:named",
        "route+values" => "route:named?id=7",
        _ => throw new ArgumentException(binding)
    };

    private static (TagHelperContext, TagHelperOutput) Make(params (string name, string? value)[] attrs)
    {
        var all = new TagHelperAttributeList();
        var outAttrs = new TagHelperAttributeList();
        foreach (var (n, v) in attrs)
        {
            all.Add(n, v);
            outAttrs.Add(n, v);
        }

        var ctx = new TagHelperContext("div", all, new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput("div", outAttrs,
            (_, _) => System.Threading.Tasks.Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        return (ctx, output);
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public void Writes_url_into_authors_spelling(string attribute, string binding)
    {
        var (ctx, output) = Make((attribute, ""));
        Bind(binding).Process(ctx, output);

        var written = Assert.Single(output.Attributes);
        Assert.Equal(attribute, written.Name);
        Assert.Equal(Expected(binding), written.Value?.ToString());
    }

    [Theory]
    [InlineData("hx-get")]
    [InlineData("hx-post:inherited")]
    [InlineData("data-hx-put")]
    [InlineData("data-hx-delete:inherited")]
    public void Valueless_verb_without_binding_is_left_untouched(string attribute)
    {
        // Binding attrs absent: nothing was bound by Razor, so nothing may be generated.
        var (ctx, output) = Make((attribute, ""));
        var helper = new HtmxUrlTagHelper(FakeHtmlGenerator.Create());
        helper.Process(ctx, output);

        var attr = Assert.Single(output.Attributes);
        Assert.Equal(attribute, attr.Name);
        Assert.Equal("", attr.Value?.ToString());
        Assert.Equal($"{attribute}=\"\"", $"{attr.Name}=\"{attr.Value}\"");
    }

    [Fact]
    public void Binding_without_any_verb_attribute_does_nothing()
    {
        var (ctx, output) = Make(("hxn-page", "/Index"));
        Bind("page").Process(ctx, output);
        Assert.Equal("hxn-page", Assert.Single(output.Attributes).Name);
    }

    [Theory]
    [InlineData("hx-get", "/keep")]
    [InlineData("hx-post:inherited", "/keep")]
    [InlineData("data-hx-put:inherited", "/keep")]
    public void Existing_value_is_left_unchanged(string attribute, string value)
    {
        var (ctx, output) = Make((attribute, value));
        Bind("page").Process(ctx, output);

        var attr = Assert.Single(output.Attributes);
        Assert.Equal(value, attr.Value?.ToString());
    }

    [Fact]
    public void Multiple_verbs_throw()
    {
        var (ctx, output) = Make(("hx-get", ""), ("hx-post:inherited", ""));
        Assert.Throws<InvalidOperationException>(() => Bind("page").Process(ctx, output));
    }

    [Fact]
    public void Conflicting_bindings_throw()
    {
        var (ctx, output) = Make(("hx-get", ""));
        var t = Bind("page");
        t.Route = "named";
        Assert.Throws<InvalidOperationException>(() => t.Process(ctx, output));

        t = Bind("action");
        t.Page = "/Index";
        Assert.Throws<InvalidOperationException>(() => t.Process(ctx, output));

        t = Bind("action");
        t.Route = "x";
        Assert.Throws<InvalidOperationException>(() => t.Process(ctx, output));
    }

    [Fact]
    public void Null_arguments_throw()
    {
        var t = new HtmxUrlTagHelper(FakeHtmlGenerator.Create());
        var (ctx, output) = Make();
        Assert.Throws<ArgumentNullException>(() => t.Process(null!, output));
        Assert.Throws<ArgumentNullException>(() => t.Process(ctx, null!));
    }

    [Fact]
    public void Order_runs_before_other_helpers() =>
        Assert.Equal(-1000, new HtmxUrlTagHelper(FakeHtmlGenerator.Create()).Order);

    [Fact]
    public void Html_string_values_are_treated_as_existing()
    {
        var all = new TagHelperAttributeList { new("hx-get", new Microsoft.AspNetCore.Html.HtmlString("/x")) };
        var ctx = new TagHelperContext("div", all, new Dictionary<object, object>(), "id");
        var output = new TagHelperOutput("div", new TagHelperAttributeList(all),
            (_, _) => System.Threading.Tasks.Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        Bind("page").Process(ctx, output);
        Assert.Equal("/x", output.Attributes["hx-get"].Value.ToString());
    }

    [Fact]
    public void Optional_link_parts_are_forwarded_without_error()
    {
        var (ctx, output) = Make(("hx-get", ""));
        var t = Bind("route");
        t.Host = "h"; t.Protocol = "https"; t.Fragment = "f";
        t.Process(ctx, output);
        Assert.Equal("route:named", output.Attributes["hx-get"].Value.ToString());
    }
}
