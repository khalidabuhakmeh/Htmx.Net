using JetBrains.Annotations;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Linq;

namespace Htmx.TagHelpers
{
    /// <summary>
    /// Targets any element that has hx-get, hx-post, hx-put, hx-patch, and hx-delete.
    /// https://htmx.org/attributes/hx-headers/
    /// </summary>
    [PublicAPI]
    [HtmlTargetElement("*", Attributes = "[hx-get]")]
    [HtmlTargetElement("*", Attributes = "[hx-post]")]
    [HtmlTargetElement("*", Attributes = "[hx-put]")]
    [HtmlTargetElement("*", Attributes = "[hx-delete]")]
    [HtmlTargetElement("*", Attributes = "[hx-patch]")]
    public class HtmxHeadersTagHelper : TagHelper
    {
        /// <summary>
        /// Dictionary of hx-headers
        /// </summary>
        [HtmlAttributeName(DictionaryAttributePrefix = "hx-headers-")]
        public IDictionary<string, string?> HeaderAttributes { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var existingHxHeaders = output.Attributes["hx-headers"]?.Value;
            if (existingHxHeaders != null || !HeaderAttributes.Any())
            {
                return;
            }

            // serialize
            var json = JsonSerializer.Serialize(HeaderAttributes);

            output.Attributes.Add("hx-headers", json);
        }
    }
}
