using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinFlux.Models
{
    public class ApplicationInfo
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("choco")]
        public string Chocolatey { get; set; }

        [JsonPropertyName("content")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("winget")]
        public string Winget { get; set; }
    }

    public class ApplicationsData
    {
        [JsonExtensionData]
        public Dictionary<string, ApplicationInfo> Applications { get; set; } = new Dictionary<string, ApplicationInfo>();
    }
} 