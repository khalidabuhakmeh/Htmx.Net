using System;
using System.IO;

namespace Htmx.TagHelpers
{
    public static class HtmxSnippets
    {
        public static string AntiforgeryJavaScript
            => GetString(nameof(AntiforgeryJavaScript));

        private static string GetString(string name)
        {
            var assembly = typeof(HtmxSnippets).Assembly;
            using var resource = assembly.GetManifestResourceStream(name);

            if (resource == null)
                throw new ArgumentException($"Resource \"{name}\" was not found.", nameof(name));
            
            using var reader = new StreamReader(resource);
            return reader.ReadToEnd();
        }
    }
}