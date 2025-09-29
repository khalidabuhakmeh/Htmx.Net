using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Htmx.TagHelpers;

[PublicAPI]
[HtmlTargetElement("meta", Attributes = "[name=htmx-config]", TagStructure = TagStructure.WithoutEndTag)]
public class HtmxConfigMetaTagHelper : TagHelper
{
    /// <summary>
    /// defaults to true, really only useful for testing
    /// </summary>
    [HtmlAttributeName("historyEnabled")]
    public bool? HistoryEnabled { get; set; }

    /// <summary>
    /// defaults to 10
    /// </summary>
    [HtmlAttributeName("historyCacheSize")]
    public int? HistoryCacheSize { get; set; } = 10;

    /// <summary>
    /// defaults to false, if set to true htmx will issue a full page refresh on history misses rather than use an AJAX request
    /// </summary>
    [HtmlAttributeName("refreshOnHistoryMiss")]
    public bool? RefreshOnHistoryMiss { get; set; }

    /// <summary>
    /// defaults to innerHTML
    /// </summary>
    [HtmlAttributeName("defaultSwapStyle")]
    public string? DefaultSwapStyle { get; set; }

    /// <summary>
    /// defaults to 0
    /// </summary>
    [HtmlAttributeName("defaultSwapDelay")]
    public int? DefaultSwapDelay { get; set; }

    /// <summary>
    /// defaults to 100
    /// </summary>
    [HtmlAttributeName("defaultSettleDelay")]
    public int? DefaultSettleDelay { get; set; }

    /// <summary>
    /// defaults to true (determines if the indicator styles are loaded)
    /// </summary>
    [HtmlAttributeName("includeIndicatorStyles")]
    public bool? IncludeIndicatorStyles { get; set; }

    /// <summary>
    /// defaults to htmx-indicator
    /// </summary>
    [HtmlAttributeName("indicatorClass")]
    public string? IndicatorClass { get; set; }

    /// <summary>
    /// defaults to htmx-request
    /// </summary>
    [HtmlAttributeName("requestClass")]
    public string? RequestClass { get; set; }

    /// <summary>
    /// defaults to htmx-settling
    /// </summary>
    [HtmlAttributeName("settlingClass")]
    public string? SettlingClass { get; set; }

    /// <summary>
    /// defaults to htmx-swapping
    /// </summary>
    [HtmlAttributeName("swappingClass")]
    public string? SwappingClass { get; set; }

    /// <summary>
    /// defaults to htmx-added
    /// </summary>
    [HtmlAttributeName("addedClass")]
    public string? AddedClass { get; set; }

    /// <summary>
    /// defaults to false
    /// </summary>
    [HtmlAttributeName("selfRequestsOnly")]
    public bool SelfRequestsOnly { get; set; }

    /// <summary>
    /// defaults to true
    /// </summary>
    [HtmlAttributeName("allowScriptTags")]
    public bool AllowScriptTags { get; set; } = true;

    /// <summary>
    /// defaults to true
    /// </summary>
    [HtmlAttributeName("allowEval")]
    public bool? AllowEval { get; set; }

    /// <summary>
    /// defaults to false, HTML template tags for parsing content from the server (not IE11 compatible!)
    /// </summary>
    [HtmlAttributeName("useTemplateFragments")]
    public bool? UseTemplateFragments { get; set; }

    /// <summary>
    /// defaults to full-jitter
    /// </summary>
    [HtmlAttributeName("wsReconnectDelay")]
    public string? WsReconnectDelay { get; set; }

    /// <summary>
    /// defaults to blob
    /// </summary>
    [HtmlAttributeName("wsBinaryType")]
    public string? WsBinaryType { get; set; }

    /// <summary>
    /// defaults to [disable-htmx], [data-disable-htmx], htmx will not process elements with this attribute on it or a parent
    /// </summary>
    [HtmlAttributeName("disableSelector")]
    public string? DisableSelector { get; set; }
        
    /// <summary>
    /// default timeout of an HTMX triggered HTTP Request. Defaults to 0 in HTMX.
    /// </summary>
    [HtmlAttributeName("timeout")]
    public int? Timeout { get; set; }

    [HtmlAttributeName("includeAspNetAntiforgeryToken")]
    public bool IncludeAntiForgery { get; set; }

    /// <summary>
    /// defaults to ""
    /// </summary>
    [HtmlAttributeName("inlineScriptNonce")]
    public string? InlineScriptNonce { get; set; }

    /// <summary>
    /// defaults to false
    /// </summary>
    [HtmlAttributeName("withCredentials")]
    public bool WithCredentials { get; set; }

    /// <summary>
    /// defaults to smooth
    /// </summary>
    [HtmlAttributeName("scrollBehavior")]
    public string? ScrollBehavior { get; set; }

    /// <summary>
    /// defaults to false
    /// </summary>
    [HtmlAttributeName("defaultFocusScroll")]
    public bool DefaultFocusScroll { get; set; }

    /// <summary>
    /// defaults to false
    /// </summary>
    [HtmlAttributeName("getCacheBusterParam")]
    public bool GetCacheBusterParam { get; set; }

    /// <summary>
    /// defaults to false
    /// </summary>
    [HtmlAttributeName("globalViewTransitions")]
    public bool GlobalViewTransitions { get; set; }

    /// <summary>
    /// defaults to ["class", "style", "width", "height"]
    /// </summary>
    [HtmlAttributeName("attributesToSettle")]
    public IEnumerable<string>? AttributesToSettle { get; set; }

    /// <summary>
    /// defaults to ["get"]
    /// </summary>
    [HtmlAttributeName("methodsThatUseUrlParams")]
    public IEnumerable<string>? MethodsThatUseUrlParams { get; set; }
    
    /// <summary>
    /// If set to `true` htmx will not update the title of the document when a title tag is found in new content
    /// </summary>
    [HtmlAttributeName("ignoreTitle")]
    public bool? IgnoreTitle { get; set; }
    
    /// <summary>
    /// Defaults to true, whether or not the target of a boosted element is scrolled into the viewport. If hx-target is omitted on a boosted element, the target defaults to body, causing the page to scroll to the top.
    /// </summary>
    [HtmlAttributeName("scrollIntoViewOnBoost")]
    public bool? ScrollIntoViewOnBoost { get; set; }
    
    /// <summary>
    /// Pro feature: defaults to null, the cache to store evaluated trigger specifications into, improving parsing performance at the cost of more memory usage.
    /// </summary>
    [HtmlAttributeName("triggerSpecsCache")]
    public HtmxTriggerSpecificationCache? TriggerSpecsCache { get; set; }
    
    [ViewContext] 
    public ViewContext ViewContext { get; set; } = null!;

    private HttpContext HttpContext => ViewContext.HttpContext;

    static JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = JsonSerializer.Serialize(new
        {
            AllowEval,
            SelfRequestsOnly,
            AllowScriptTags,
            DisableSelector,
            HistoryEnabled,
            IndicatorClass,
            AddedClass,
            RequestClass,
            SettlingClass,
            SwappingClass,
            DefaultSettleDelay,
            DefaultSwapDelay,
            DefaultSwapStyle,
            HistoryCacheSize,
            IncludeIndicatorStyles,
            UseTemplateFragments,
            WsBinaryType,
            WsReconnectDelay,
            RefreshOnHistoryMiss,
            Timeout,
            InlineScriptNonce,
            WithCredentials,
            ScrollBehavior,
            DefaultFocusScroll,
            GetCacheBusterParam,
            GlobalViewTransitions,
            AttributesToSettle,
            MethodsThatUseUrlParams,
            IgnoreTitle,
            ScrollIntoViewOnBoost,
            TriggerSpecsCache,
            AntiForgery = TryGetAntiForgeryConfig()
        }, serializerOptions);

        output.Attributes.RemoveAll("content");

        // make sure to use single quotes
        // because JSON uses double quotes
        output.Attributes.Add(new TagHelperAttribute(
            "content",
            new HtmlString(config),
            HtmlAttributeValueStyle.SingleQuotes)
        );
    }

    private AntiForgeryConfig? TryGetAntiForgeryConfig()
    {
        if (!IncludeAntiForgery)
            return null;

        var antiforgery = HttpContext.RequestServices.GetService<IAntiforgery>();
            
        // need to call GetAndStoreTokens or else
        // all requests will fail until something calls
        // GetAndStoreTokens. This ensures the token
        // is added to the user's session cookies
        if (antiforgery?.GetAndStoreTokens(HttpContext) is { } tokens)
        {
            return new AntiForgeryConfig(tokens);
        }

        return null;
    }

    private class AntiForgeryConfig
    {
        public AntiForgeryConfig(AntiforgeryTokenSet? antiforgery)
        {
            ArgumentNullException.ThrowIfNull(antiforgery);

            FormFieldName = antiforgery.FormFieldName;
            HeaderName = antiforgery.HeaderName;
            // important or token gets warped
            RequestToken = HttpUtility.HtmlAttributeEncode(antiforgery.RequestToken)!;
        }

        // Serialized
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string FormFieldName { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string? HeaderName { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string RequestToken { get; set; }
    }
}

public class HtmxTriggerSpecificationCache: Dictionary<string, HtmxTriggerSpecification[]> {}

public class HtmxTriggerSpecification
{
    [JsonPropertyName("trigger")] 
    public string Trigger { get;  set; } = default!;
    [JsonPropertyName("sseEvent")]
    public string? SseEvent { get; set; }
    [JsonPropertyName("eventFilter")]
    public string? EventFilter { get; set; }
    [JsonPropertyName("changed")]
    public bool? Changed { get; set; }
    [JsonPropertyName("once")]
    public bool? Once { get; set; }
    [JsonPropertyName("consume")]
    public bool? Consume { get; set; }
    [JsonPropertyName("from")]
    public string? From { get; set; }
    [JsonPropertyName("target")]
    public string? Target { get; set; }
    [JsonPropertyName("throttle")]
    public int? Throttle { get; set; }
    [JsonPropertyName("queue")]
    public string? Queue { get; set; }
    [JsonPropertyName("root")]
    public string? Root { get; set; }
    [JsonPropertyName("threshold")]
    public string? Threshold { get; set; }
    [JsonPropertyName("delay")]
    public int? Delay { get; set; }
    [JsonPropertyName("pollInterval")]
    public int? PollInterval { get; set; }
}