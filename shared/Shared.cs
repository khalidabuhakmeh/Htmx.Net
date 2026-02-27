using System.Text.Json;
using System.Text.Json.Serialization;

namespace Htmx;

internal static class Shared
{
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}