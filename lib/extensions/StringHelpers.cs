using System.Text.RegularExpressions;
using System;
using System.Text.Json;
using System.Text;
using System.IO;

namespace lib.extensions
{
    public static class StringHelpers
    {
        public static string? ToSnakeCase(this string? input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
        public static string Stringify(this JsonDocument document)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = false});
                document.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
    
}