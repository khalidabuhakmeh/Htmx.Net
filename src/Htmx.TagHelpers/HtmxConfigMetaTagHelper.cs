using System;
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

/// <summary>
/// Renders the htmx 4 config meta tag. Every property is nullable; unset
/// properties are not serialized so htmx defaults apply.
/// </summary>
[PublicAPI]
[HtmlTargetElement("meta", Attributes = "[name=htmx-config]", TagStructure = TagStructure.WithoutEndTag)]
public class HtmxConfigMetaTagHelper : TagHelper
{
    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>htmx 4 default: false</summary>
    [HtmlAttributeName("logAll")]
    public bool? LogAll { get; set; }

    /// <summary>htmx 4 default: "data-hx-"</summary>
    [HtmlAttributeName("prefix")]
    public string? Prefix { get; set; }

    /// <summary>htmx 4 default: undefined</summary>
    [HtmlAttributeName("metaCharacter")]
    public string? MetaCharacter { get; set; }

    /// <summary>htmx 4 default: false</summary>
    [HtmlAttributeName("transitions")]
    public bool? Transitions { get; set; }

    /// <summary>htmx 4 default: true; also accepts a history mode string</summary>
    [HtmlAttributeName("history")]
    public string? History { get; set; }

    /// <summary>htmx 4 default: "same-origin"</summary>
    [HtmlAttributeName("mode")]
    public string? Mode { get; set; }

    /// <summary>htmx 4 default: "innerHTML"</summary>
    [HtmlAttributeName("defaultSwap")]
    public string? DefaultSwap { get; set; }

    /// <summary>htmx 4 default: false</summary>
    [HtmlAttributeName("allowEmptySwapAfterOOB")]
    public bool? AllowEmptySwapAfterOOB { get; set; }

    /// <summary>htmx 4 default: false</summary>
    [HtmlAttributeName("defaultFocusScroll")]
    public bool? DefaultFocusScroll { get; set; }

    /// <summary>htmx 4 default: 1</summary>
    [HtmlAttributeName("defaultSettleDelay")]
    public int? DefaultSettleDelay { get; set; }

    /// <summary>htmx 4 default: "htmx-indicator"</summary>
    [HtmlAttributeName("indicatorClass")]
    public string? IndicatorClass { get; set; }

    /// <summary>htmx 4 default: "htmx-request"</summary>
    [HtmlAttributeName("requestClass")]
    public string? RequestClass { get; set; }

    /// <summary>htmx 4 default: true</summary>
    [HtmlAttributeName("includeIndicatorCSS")]
    public bool? IncludeIndicatorCSS { get; set; }

    /// <summary>htmx 4 default: 60000</summary>
    [HtmlAttributeName("defaultTimeout")]
    public int? DefaultTimeout { get; set; }

    /// <summary>htmx 4 default: ""</summary>
    [HtmlAttributeName("extensions")]
    public string? Extensions { get; set; }

    /// <summary>htmx 4 default: ["data-htmx-powered"]</summary>
    [HtmlAttributeName("morphIgnore")]
    public string[]? MorphIgnore { get; set; }

    /// <summary>htmx 4 default: "[hx-morph-skip]"</summary>
    [HtmlAttributeName("morphSkip")]
    public string? MorphSkip { get; set; }

    /// <summary>htmx 4 default: "[hx-morph-skip-children]"</summary>
    [HtmlAttributeName("morphSkipChildren")]
    public string? MorphSkipChildren { get; set; }

    /// <summary>htmx 4 default: 10</summary>
    [HtmlAttributeName("morphScanLimit")]
    public int? MorphScanLimit { get; set; }

    /// <summary>htmx 4 default: [204, 304]; ints and/or strings</summary>
    [HtmlAttributeName("noSwap")]
    public object[]? NoSwap { get; set; }

    /// <summary>htmx 4 default: false</summary>
    [HtmlAttributeName("implicitInheritance")]
    public bool? ImplicitInheritance { get; set; }

    /// <summary>htmx 4 default: undefined</summary>
    [HtmlAttributeName("inlineScriptNonce")]
    public string? InlineScriptNonce { get; set; }

    [HtmlAttributeName("includeAspNetAntiforgeryToken")]
    public bool IncludeAntiForgery { get; set; }

    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    private HttpContext HttpContext => ViewContext.HttpContext;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var config = JsonSerializer.Serialize(BuildConfig(), ConfigJsonOptions);

        output.Attributes.RemoveAll("content");

        // make sure to use single quotes
        // because JSON uses double quotes
        output.Attributes.Add(new TagHelperAttribute(
            "content",
            new HtmlString(config),
            HtmlAttributeValueStyle.SingleQuotes)
        );
    }

    // htmx 4's `history` config accepts a boolean OR a history-mode string. Emit a real JSON boolean
    // for "true"/"false" so markup like history="false" serializes as {"history":false}, not "false".
    private static object? NormalizeHistory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (bool.TryParse(value, out var b)) return b;
        return value;
    }

    private HtmxConfig BuildConfig() => new()
    {                LogAll = LogAll,
                Prefix = Prefix,
                MetaCharacter = MetaCharacter,
                Transitions = Transitions,
                History = NormalizeHistory(History),
                Mode = Mode,
                DefaultSwap = DefaultSwap,
                AllowEmptySwapAfterOOB = AllowEmptySwapAfterOOB,
                DefaultFocusScroll = DefaultFocusScroll,
                DefaultSettleDelay = DefaultSettleDelay,
                IndicatorClass = IndicatorClass,
                RequestClass = RequestClass,
                IncludeIndicatorCSS = IncludeIndicatorCSS,
                DefaultTimeout = DefaultTimeout,
                Extensions = Extensions,
                MorphIgnore = MorphIgnore,
                MorphSkip = MorphSkip,
                MorphSkipChildren = MorphSkipChildren,
                MorphScanLimit = MorphScanLimit,
                NoSwap = NoSwap,
                ImplicitInheritance = ImplicitInheritance,
                InlineScriptNonce = InlineScriptNonce,
                AntiForgery = TryGetAntiForgeryConfig()
    };

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

    /// <summary>Serialized shape of the htmx 4 config (explicit JSON names, no naming policy).</summary>
    private sealed class HtmxConfig
    {
        [JsonPropertyName("logAll")]
        public bool? LogAll { get; init; }

        [JsonPropertyName("prefix")]
        public string? Prefix { get; init; }

        [JsonPropertyName("metaCharacter")]
        public string? MetaCharacter { get; init; }

        [JsonPropertyName("transitions")]
        public bool? Transitions { get; init; }

        [JsonPropertyName("history")]
        public object? History { get; init; }

        [JsonPropertyName("mode")]
        public string? Mode { get; init; }

        [JsonPropertyName("defaultSwap")]
        public string? DefaultSwap { get; init; }

        [JsonPropertyName("allowEmptySwapAfterOOB")]
        public bool? AllowEmptySwapAfterOOB { get; init; }

        [JsonPropertyName("defaultFocusScroll")]
        public bool? DefaultFocusScroll { get; init; }

        [JsonPropertyName("defaultSettleDelay")]
        public int? DefaultSettleDelay { get; init; }

        [JsonPropertyName("indicatorClass")]
        public string? IndicatorClass { get; init; }

        [JsonPropertyName("requestClass")]
        public string? RequestClass { get; init; }

        [JsonPropertyName("includeIndicatorCSS")]
        public bool? IncludeIndicatorCSS { get; init; }

        [JsonPropertyName("defaultTimeout")]
        public int? DefaultTimeout { get; init; }

        [JsonPropertyName("extensions")]
        public string? Extensions { get; init; }

        [JsonPropertyName("morphIgnore")]
        public string[]? MorphIgnore { get; init; }

        [JsonPropertyName("morphSkip")]
        public string? MorphSkip { get; init; }

        [JsonPropertyName("morphSkipChildren")]
        public string? MorphSkipChildren { get; init; }

        [JsonPropertyName("morphScanLimit")]
        public int? MorphScanLimit { get; init; }

        [JsonPropertyName("noSwap")]
        public object[]? NoSwap { get; init; }

        [JsonPropertyName("implicitInheritance")]
        public bool? ImplicitInheritance { get; init; }

        [JsonPropertyName("inlineScriptNonce")]
        public string? InlineScriptNonce { get; init; }

        [JsonPropertyName("antiForgery")]
        public AntiForgeryConfig? AntiForgery { get; init; }
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

        [JsonPropertyName("formFieldName")]
        public string FormFieldName { get; set; }
        [JsonPropertyName("headerName")]
        public string? HeaderName { get; set; }
        [JsonPropertyName("requestToken")]
        public string RequestToken { get; set; }
    }
}
