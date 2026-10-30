using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace Htmx.TagHelpers;

/// <summary>
/// Generates a request URL for an htmx verb attribute when a URL-binding attribute
/// (<c>hxn-page</c>, <c>hxn-action</c>, <c>hxn-controller</c> or <c>hxn-route</c>) is present.
/// </summary>
/// <remarks>
/// The binding attributes use the <c>hxn-</c> prefix (htmx + .NET) rather than <c>hx-</c>, because
/// <c>hx-action</c> is a native htmx 4 attribute and <c>hx-*</c> is htmx's reserved namespace.
/// htmx 4 treats a valueless <c>hx-get</c> as valid (it requests the current URL), so this helper
/// no longer matches on the verb attributes. It only runs when a binding attribute is present and then
/// writes the URL into whichever spelling of the verb the author used:
/// <c>hx-get</c>, <c>hx-get:inherited</c>, <c>data-hx-get</c> or <c>data-hx-get:inherited</c>
/// (verbs: get, post, put, patch, delete). The spelling is detected at runtime from
/// <c>context.AllAttributes</c>, so no design-time colon matching is needed.
/// NOTE: in htmx 4, hx-delete no longer includes form inputs; add hx-include yourself if needed (not automatic).
/// </remarks>
[PublicAPI]
[HtmlTargetElement("*", Attributes = "hxn-page")]
[HtmlTargetElement("*", Attributes = "hxn-action")]
[HtmlTargetElement("*", Attributes = "hxn-controller")]
[HtmlTargetElement("*", Attributes = "hxn-route")]
public class HtmxUrlTagHelper : TagHelper
{
    private const string ActionAttributeName = "hxn-action";
    private const string ControllerAttributeName = "hxn-controller";
    private const string AreaAttributeName = "hxn-area";
    private const string PageAttributeName = "hxn-page";
    private const string PageHandlerAttributeName = "hxn-page-handler";
    private const string FragmentAttributeName = "hxn-fragment";
    private const string HostAttributeName = "hxn-host";
    private const string ProtocolAttributeName = "hxn-protocol";
    private const string RouteAttributeName = "hxn-route";
    private const string RouteValuesDictionaryName = "hxn-all-route-data";
    private const string RouteValuesPrefix = "hxn-route-";
    private IDictionary<string, string>? routeValues;

    private static readonly string[] Verbs = { "get", "post", "put", "patch", "delete" };

    // O(1) membership test for the accepted verb spellings (plain / :inherited / data- / data-:inherited).
    private static readonly HashSet<string> HtmxMethods = new(
        Verbs.SelectMany(v => new[] { $"hx-{v}", $"hx-{v}:inherited", $"data-hx-{v}", $"data-hx-{v}:inherited" }),
        StringComparer.OrdinalIgnoreCase);

    private static bool IsMethod(string name) => HtmxMethods.Contains(name);

    /// <summary>
    /// Creates a new <see cref="AnchorTagHelper"/>.
    /// </summary>
    /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
    public HtmxUrlTagHelper(IHtmlGenerator generator)
    {
        Generator = generator;
        routeValues = new Dictionary<string, string>();
    }

    /// <inheritdoc />
    public override int Order => -1000;

    /// <summary>
    /// Gets the <see cref="IHtmlGenerator"/> used to generate the <see cref="AnchorTagHelper"/>'s output.
    /// </summary>
    protected IHtmlGenerator Generator { get; }

    /// <summary>
    /// The name of the action method.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Page"/> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(ActionAttributeName)]
    [AspMvcAction]
    public string? Action { get; set; }

    /// <summary>
    /// The name of the controller.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Page"/> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(ControllerAttributeName)]
    [AspMvcController]
    public string? Controller { get; set; }

    /// <summary>
    /// The name of the area.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if <see cref="Route"/> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(AreaAttributeName)]
    [AspMvcArea]
    public string? Area { get; set; }

    /// <summary>
    /// The name of the page.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Action"/>, <see cref="Controller"/>
    /// is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(PageAttributeName)]
    [AspMvcView]
    public string? Page { get; set; }

    /// <summary>
    /// The name of the page handler.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if <see cref="Route"/> or <see cref="Action"/>, or <see cref="Controller"/>
    /// is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(PageHandlerAttributeName)]
    public string? PageHandler { get; set; }

    /// <summary>
    /// The protocol for the URL, such as &quot;http&quot; or &quot;https&quot;.
    /// </summary>
    [HtmlAttributeName(ProtocolAttributeName)]
    public string? Protocol { get; set; }

    /// <summary>
    /// The host name.
    /// </summary>
    [HtmlAttributeName(HostAttributeName)]
    public string? Host { get; set; }

    /// <summary>
    /// The URL fragment name.
    /// </summary>
    [HtmlAttributeName(FragmentAttributeName)]
    public string? Fragment { get; set; }

    /// <summary>
    /// Name of the route.
    /// </summary>
    /// <remarks>
    /// Must be <c>null</c> if one of <see cref="Action"/>, <see cref="Controller"/>, <see cref="Area"/>
    /// or <see cref="Page"/> is non-<c>null</c>.
    /// </remarks>
    [HtmlAttributeName(RouteAttributeName)]
    public string? Route { get; set; }

    /// <summary>
    /// Additional parameters for the route.
    /// </summary>
    [HtmlAttributeName(RouteValuesDictionaryName, DictionaryAttributePrefix = RouteValuesPrefix)]
    public IDictionary<string, string>? RouteValues
    {
        get => routeValues ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        set => routeValues = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/> for the current request.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public ViewContext? ViewContext { get; set; }

    /// <inheritdoc />
    /// <remarks>Does nothing if user provides an <c>href</c> attribute.</remarks>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        // Single pass over the authored attributes: find the verb attribute (and reject duplicates)
        // with O(1) membership checks, instead of two LINQ passes over a linear-scanned list.
        TagHelperAttribute? hxMethodAttribute = null;
        foreach (var a in context.AllAttributes)
        {
            if (!IsMethod(a.Name))
            {
                continue;
            }

            if (hxMethodAttribute != null)
            {
                throw new InvalidOperationException(
                    $"Too many htmx method attributes found on element. Use only one of the following: {string.Join(",", HtmxMethods)}");
            }

            hxMethodAttribute = a;
        }

        if (hxMethodAttribute == null)
        {
            // binding attributes without a verb: nothing to write into
            return;
        }

        // if it already has a non-empty value, don't do anything
        var existing = hxMethodAttribute.Value switch
        {
            HtmlString hs => hs.Value,
            string str => str,
            null => null,
            var other => other.ToString()
        };
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return;
        }

        var routeLink = Route != null;
        var actionLink = Controller != null || Action != null;
        var pageLink = Page != null || PageHandler != null;

        if (!routeLink && !actionLink && !pageLink)
        {
            // no binding attribute: a valueless verb is valid htmx 4 (current URL), leave it alone
            return;
        }

        if ((routeLink && actionLink) || (routeLink && pageLink) || (actionLink && pageLink))
        {
            var message = string.Join(
                Environment.NewLine,
                "Cannot determine link for element",
                RouteAttributeName,
                ControllerAttributeName + ", " + ActionAttributeName,
                PageAttributeName + ", " + PageHandlerAttributeName);

            throw new InvalidOperationException(message);
        }

        RouteValueDictionary? values = null;
        if (routeValues is {Count: >0})
        {
            values = new RouteValueDictionary((IEnumerable<KeyValuePair<string, string?>>)routeValues!);
        }

        if (Area != null)
        {
            // Unconditionally replace any value from asp-route-area.
            values ??= new RouteValueDictionary();
            values["area"] = Area;
        }

        TagBuilder tagBuilder;
        if (pageLink)
        {
            tagBuilder = Generator.GeneratePageLink(
                ViewContext,
                linkText: string.Empty,
                pageName: Page,
                pageHandler: PageHandler,
                protocol: Protocol,
                hostname: Host,
                fragment: Fragment,
                routeValues: values,
                htmlAttributes: null);
        }
        else if (routeLink)
        {
            tagBuilder = Generator.GenerateRouteLink(
                ViewContext,
                linkText: string.Empty,
                routeName: Route,
                protocol: Protocol,
                hostName: Host,
                fragment: Fragment,
                routeValues: values,
                htmlAttributes: null);
        }
        else
        {
            tagBuilder = Generator.GenerateActionLink(
                ViewContext,
                linkText: string.Empty,
                actionName: Action,
                controllerName: Controller,
                protocol: Protocol,
                hostname: Host,
                fragment: Fragment,
                routeValues: values,
                htmlAttributes: null);
        }

        var href = tagBuilder.Attributes["href"];
        var written = hxMethodAttribute.Name;
        output.Attributes.RemoveAll(written);
        output.Attributes.Add(written, href);
    }
}
