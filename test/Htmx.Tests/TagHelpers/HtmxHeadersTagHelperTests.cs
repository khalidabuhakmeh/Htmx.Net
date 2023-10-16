using Htmx.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

namespace Htmx.Tests.TagHelpers
{
    public class HtmxHeadersTagHelperTests
    {
        private readonly HtmxHeadersTagHelper _tagHelper;
        private readonly TagHelperContext _context;
        private readonly TagHelperOutput _output;

        public HtmxHeadersTagHelperTests()
        {
            _tagHelper = new HtmxHeadersTagHelper();

            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test_unique_id");

            _output = new TagHelperOutput(
                "div",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    var tagBuilder = new TagBuilder("div");
                    tagBuilder.Attributes.Add("hx-post", "url");
                    tagHelperContent.SetHtmlContent(tagBuilder);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }

        [Fact]
        public void Is_HeaderAttributes_NotNull()
        {
            Assert.NotNull(_tagHelper.HeaderAttributes);
            Assert.Empty(_tagHelper.HeaderAttributes);
        }

        [Fact]
        public void Process_AddsHeadersAttribute_WhenNoExistingHeadersAndHeaderAttributesPresent()
        {
            // Arrange
            _tagHelper.HeaderAttributes.Add("Key1", "Value1");
            _tagHelper.HeaderAttributes.Add("Key2", "Value2");

            // Act
            _tagHelper.Process(_context, _output);

            // Assert
            Assert.True(_output.Attributes.TryGetAttribute("hx-headers", out var headersAttribute));
            Assert.NotNull(headersAttribute);

            var json = JsonSerializer.Deserialize<JsonObject>(headersAttribute.Value.ToString()!)!;
            Assert.Equal("Value1", json["Key1"]!.GetValue<string>());
            Assert.Equal("Value2", json["Key2"]!.GetValue<string>());
        }

        [Fact]
        public void Process_DoesNotAddHeadersAttribute_WhenExistingHeadersPresent()
        {
            // Arrange
            var existingHeaders = new TagHelperAttribute("hx-headers", "{\"ExistingKey1\":\"ExistingValue1\"}");
            _output.Attributes.Add(existingHeaders);

            // Act
            _tagHelper.Process(_context, _output);

            // Assert
            var json = JsonSerializer.Deserialize<JsonObject>(_output.Attributes["hx-headers"].Value.ToString()!)!;
            Assert.Equal("ExistingValue1", json["ExistingKey1"]!.GetValue<string>());
        }

        [Fact]
        public void Process_DoesNotAddHeadersAttribute_WhenNoHeaderAttributes()
        {
            // Act
            _tagHelper.Process(_context, _output);

            // Assert
            Assert.False(_output.Attributes.TryGetAttribute("hx-headers", out _));
        }
    }

}
