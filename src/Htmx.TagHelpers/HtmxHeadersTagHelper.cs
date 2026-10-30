using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Htmx.TagHelpers
{
    /// <summary>
    /// Builds <c>hx-headers</c> from <c>hx-headers-*</c> attributes.
    /// Set <c>hx-headers-inherited="true"</c> to emit <c>hx-headers:inherited</c> (htmx 4 explicit inheritance).
    /// Any existing <c>hx-headers</c> / <c>hx-headers:inherited</c> (optionally <c>data-</c> prefixed) JSON is merged.
    /// https://four.htmx.org/reference/attributes/hx-headers
    /// </summary>
    /// <remarks>
    /// Matches on the <c>hx-headers-*</c> prefix rather than the verb attributes, so it works on elements that
    /// inherit their verb and never conflicts with <see cref="HtmxUrlTagHelper"/> (which keeps Order -1000).
    /// </remarks>
    [PublicAPI]
    [HtmlTargetElement("*", Attributes = "hx-headers-*")]
    public class HtmxHeadersTagHelper : TagHelper
    {
        private static readonly string[] Spellings =
        {
            "hx-headers", "hx-headers:inherited", "data-hx-headers", "data-hx-headers:inherited"
        };

        /// <summary>
        /// Dictionary of hx-headers
        /// </summary>
        [HtmlAttributeName(DictionaryAttributePrefix = "hx-headers-")]
        public IDictionary<string, string?> HeaderAttributes { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// When true, emits <c>hx-headers:inherited</c> instead of <c>hx-headers</c>.
        /// </summary>
        [HtmlAttributeName("hx-headers-inherited")]
        public bool Inherited { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Razor may route "hx-headers-inherited" into the dictionary instead of the bool property
            // (the dictionary prefix also matches it); honor both.
            var inherited = Inherited ||
                (HeaderAttributes.TryGetValue("inherited", out var inheritedValue) &&
                 bool.TryParse(inheritedValue, out var parsed) && parsed);
            var headers = HeaderAttributes.Where(h => !string.Equals(h.Key, "inherited", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!headers.Any())
            {
                return;
            }

            var obj = new JsonObject();
            string? name = null;
            foreach (var spelling in Spellings)
            {
                var attr = output.Attributes.FirstOrDefault(a => string.Equals(a.Name, spelling, StringComparison.OrdinalIgnoreCase));
                if (attr == null) continue;
                name ??= attr.Name;
                var text = attr.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    try
                    {
                        if (JsonNode.Parse(text) is JsonObject existing)
                        {
                            foreach (var kv in existing) obj[kv.Key] = kv.Value?.DeepClone();
                        }
                    }
                    catch (JsonException)
                    {
                        // existing value is not JSON (e.g. js: expression); leave it alone
                        return;
                    }
                }
                output.Attributes.RemoveAll(attr.Name);
            }

            // prefixed attributes win over the existing JSON
            foreach (var h in headers) obj[h.Key] = h.Value;

            // single quotes: JSON uses double quotes; the default JSON encoder escapes ' as \u0027
            output.Attributes.Add(new TagHelperAttribute(
                name ?? (inherited ? "hx-headers:inherited" : "hx-headers"),
                new HtmlString(obj.ToJsonString()),
                HtmlAttributeValueStyle.SingleQuotes));
        }
    }
}
