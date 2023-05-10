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
/// Targets any element that has hx-get, hx-post, hx-put, hx-patch, and hx-delete. These
/// elements will ultimately hold the request URL generated.
/// </summary>
[PublicAPI]
[HtmlTargetElement("*", Attributes = "hx-get")]
[HtmlTargetElement("*", Attributes = "hx-post")]
[HtmlTargetElement("*", Attributes = "hx-delete")]
[HtmlTargetElement("*", Attributes = "hx-put")]
[HtmlTargetElement("*", Attributes = "hx-patch")]
public class HtmxUrlTagHelper : TagHelper
{
    private const string ActionAttributeName = "hx-action";
    private const string ControllerAttributeName = "hx-controller";
    private const string AreaAttributeName = "hx-area";
    private const string PageAttributeName = "hx-page";
    private const string PageHandlerAttributeName = "hx-page-handler";
    private const string FragmentAttributeName = "hx-fragment";
    private const string HostAttributeName = "hx-host";
    private const string ProtocolAttributeName = "hx-protocol";
    private const string RouteAttributeName = "hx-route";
    private const string RouteValuesDictionaryName = "hx-all-route-data";
    private const string RouteValuesPrefix = "hx-route-";
    private IDictionary<string, string>? routeValues;

    private static readonly List<string> HtmxMethods = new()
    {
        "hx-get", "hx-post", "hx-delete", "hx-put", "hx-patch"
    };

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

        if (output.Attributes.Count(a => HtmxMethods.Contains(a.Name)) > 1)
        {
            throw new InvalidOperationException(
                $"Too many htmx method attributes found on element. Use only one of the following: {string.Join(",", HtmxMethods)}");
        }

        var hxMethodAttribute = context.AllAttributes.First(a => HtmxMethods.Contains(a.Name));
        // if it already has a value, don't do anything
        if (hxMethodAttribute.Value is HtmlString s && !string.IsNullOrWhiteSpace(s.Value))
        {
            // if the attribute already has a value
            // then continue on, we don't want to alter it
            // Note: this will leave other hx-method attributes
            //       in the case the user is sloppy
            return;
        }

        var routeLink = Route != null;
        var actionLink = Controller != null || Action != null;
        var pageLink = Page != null || PageHandler != null;

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
            values = new RouteValueDictionary(routeValues);
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
        foreach (var htmxMethod in HtmxMethods.Where(htmxMethod =>
                     output.Attributes.TryGetAttribute(htmxMethod, out _)))
        {
            // should only loop once... should
            output.Attributes.RemoveAll(htmxMethod);
            output.Attributes.Add(htmxMethod, href);
            break;
        }
    }
}