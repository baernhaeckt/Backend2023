using System.Text.Json.Serialization;

namespace Backend2023.Hubs;

public class ProbabilitiesMax
{
    [JsonPropertyName("class")]
    public string @Class { get; set; } = string.Empty;

    [JsonPropertyName("probability")]
    public float Probability { get; set; }
}
