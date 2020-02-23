using System;
using System.Linq;
using OpenScraping;
using OpenScraping.Config;

namespace Azurite.Wiki
{
    internal static class ScrapingExtensions {
        internal static Newtonsoft.Json.Linq.JContainer ExtractFromHtml(this string html, string resourceName) {
            var config = ScrapingExtensions.CreateConfig(resourceName);
            var scraper = new StructuredDataExtractor(config);
            var result = scraper.Extract(html);
            return result;
        }

        internal static TimeSpan? ParseTimeSpan(this string s) {
            return s.All(c => char.IsDigit(c) || c == ':')
                ? (TimeSpan?)TimeSpan.Parse(s)
                : null;
        }

        internal static string Before(this string s, string separator) {
            return s.Split(new[] { separator }, StringSplitOptions.None).First().Trim();
        }

        private static ConfigSection CreateConfig(string resourceName) {
            var assembly = typeof(WikiSearcher).Assembly;
            string[] names = assembly.GetManifestResourceNames();
            if (!names.Any(n => n.Equals(resourceName))) {
                resourceName = names.FirstOrDefault(n => n.Contains(resourceName));
            }
            var stream = assembly.GetManifestResourceStream(resourceName);
            var reader = new System.IO.StreamReader(stream);
            return StructuredDataConfig.ParseJsonString(reader.ReadToEnd());
        }
    }
}
