using JetBrains.Annotations;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Htmx.TagHelpers
{
    /// <summary>
    /// Targets any element that has hx-get, hx-post, hx-put, hx-patch, and hx-delete.
    /// https://htmx.org/attributes/hx-headers/
    /// </summary>
    [PublicAPI]
    [HtmlTargetElement("*", Attributes = "[hx-post]")]
    [HtmlTargetElement("*", Attributes = "[hx-put]")]
    [HtmlTargetElement("*", Attributes = "[hx-delete]")]
    [HtmlTargetElement("*", Attributes = "[hx-patch]")]
    public class HxHeaders : TagHelper
    {
        /// <summary>
        /// JSON formatted string, a Dictionary or any object.
        /// </summary>
        [HtmlAttributeName("hx-headers")]
        public object Headers { get; set; } = default!;

        /// <summary>
        /// Dictionary of hx-headers's html attributes
        /// </summary>
        [HtmlAttributeName("hx-headers", DictionaryAttributePrefix = "hx-headers-")]
        public IDictionary<string, string?> HeaderAttributes { get; set; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var jsonObject = Headers switch
            {
                string headerJson => JsonSerializer.Deserialize<JsonObject>(headerJson),
                null => null,
                _ => JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(Headers))
            } ?? new JsonObject();

            // merge
            foreach (var jObject in jsonObject)
            {
                HeaderAttributes[jObject.Key] = jObject.Value?.ToString();
            }

            // serialize
            var json = JsonSerializer.Serialize(HeaderAttributes);

            output.Attributes.Add("hx-headers", json);
        }
    }
}
