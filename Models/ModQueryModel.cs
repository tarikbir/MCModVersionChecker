using System.Text.Json.Serialization;

namespace MCModVersionChecker.Models;
internal class ModQueryModel
{
    [JsonPropertyName("modIds")]
    public required List<string> ModIds { get; set; }

    [JsonPropertyName("filterPcOnly")]
    public bool FilterPcOnly { get; set; } = true;
}
