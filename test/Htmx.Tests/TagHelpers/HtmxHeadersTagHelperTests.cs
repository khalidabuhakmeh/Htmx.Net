using Htmx.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Linq;
using VerifyXunit;
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
        public Task Snapshot_non_inherited()
        {
            _tagHelper.HeaderAttributes.Add("X-One", "1");
            _tagHelper.HeaderAttributes.Add("X-Two", "it's \"2\"");
            _tagHelper.Process(_context, _output);
            return Verifier.Verify(Render(_output));
        }

        [Fact]
        public Task Snapshot_inherited()
        {
            _tagHelper.Inherited = true;
            _tagHelper.HeaderAttributes.Add("X-One", "1");
            _tagHelper.Process(_context, _output);
            return Verifier.Verify(Render(_output));
        }

        [Fact]
        public Task Snapshot_merge_with_existing()
        {
            _output.Attributes.Add("hx-headers", "{\"X-Existing\":\"e\",\"X-One\":\"old\"}");
            _tagHelper.HeaderAttributes.Add("X-One", "new");
            _tagHelper.Process(_context, _output);
            return Verifier.Verify(Render(_output));
        }

        [Fact]
        public void Inherited_flag_via_dictionary_key_is_honored()
        {
            _tagHelper.HeaderAttributes.Add("inherited", "true");
            _tagHelper.HeaderAttributes.Add("X-One", "1");
            _tagHelper.Process(_context, _output);
            Assert.Equal("hx-headers:inherited", Assert.Single(_output.Attributes).Name);
            Assert.DoesNotContain("inherited\"", _output.Attributes[0].Value.ToString());
        }

        [Fact]
        public void Only_inherited_flag_adds_nothing()
        {
            _tagHelper.Inherited = true;
            _tagHelper.Process(_context, _output);
            Assert.Empty(_output.Attributes);
        }

        [Theory]
        [InlineData("hx-headers:inherited")]
        [InlineData("data-hx-headers")]
        [InlineData("data-hx-headers:inherited")]
        public void Existing_spelling_is_preserved_and_merged(string name)
        {
            _output.Attributes.Add(name, "{\"A\":\"1\"}");
            _tagHelper.HeaderAttributes.Add("B", "2");
            _tagHelper.Process(_context, _output);
            var attr = Assert.Single(_output.Attributes);
            Assert.Equal(name, attr.Name);
            var json = JsonSerializer.Deserialize<JsonObject>(attr.Value.ToString()!)!;
            Assert.Equal("1", json["A"]!.GetValue<string>());
            Assert.Equal("2", json["B"]!.GetValue<string>());
        }

        [Fact]
        public void Non_json_existing_value_is_left_untouched()
        {
            _output.Attributes.Add("hx-headers", "js:{a: foo()}");
            _tagHelper.HeaderAttributes.Add("B", "2");
            _tagHelper.Process(_context, _output);
            var attr = Assert.Single(_output.Attributes);
            Assert.Equal("js:{a: foo()}", attr.Value);
        }

        [Fact]
        public void Existing_empty_value_is_replaced()
        {
            _output.Attributes.Add("hx-headers", "");
            _tagHelper.HeaderAttributes.Add("B", "2");
            _tagHelper.Process(_context, _output);
            Assert.Equal("{\"B\":\"2\"}", Assert.Single(_output.Attributes).Value.ToString());
        }

        private static string Render(TagHelperOutput output) =>
            string.Join("\n", output.Attributes.Select(a =>
                a.ValueStyle == HtmlAttributeValueStyle.SingleQuotes
                    ? $"{a.Name}='{a.Value}'"
                    : $"{a.Name}=\"{a.Value}\""));

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
