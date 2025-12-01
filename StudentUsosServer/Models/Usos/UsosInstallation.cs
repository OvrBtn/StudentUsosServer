using System.Text.Json.Serialization;

namespace StudentUsosServer.Models.Usos
{
    public class UsosInstallation
    {
        [JsonPropertyName("InstallationId")]
        public string InstallationId { get; set; }
        [JsonPropertyName("InstallationUrl")]
        public string InstallationUrl { get; set; }
        [JsonPropertyName("LocalizedName")]
        public Dictionary<string, string> LocalizedName { get; set; }
        public bool IsSupported { get; set; }
    }
}
