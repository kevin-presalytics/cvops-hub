using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;
using System;

namespace api.controllers.options
{
    public class SlugifyUrls : IOutboundParameterTransformer
    {

        // Slugify value
        public string? TransformOutbound(object? value)
        {
            if (value == null) { return null; }

            return Regex.Replace(value.ToString()!,
                                "([a-z])([A-Z])",
                                "$1-$2",
                                RegexOptions.CultureInvariant,
                                TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
        }
    
    }

}

