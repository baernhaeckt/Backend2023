using System.Text.Json.Serialization;

namespace Backend2023.Hubs;

public class EmotionDetectionResponse
{
    [JsonPropertyName("predicted_classes")]
    public string[] PredictedClasses { get; set; } = Array.Empty<string>();

    [JsonPropertyName("probabilities_max")]
    public ProbabilitiesMax[] ProbabilitiesMax { get; set; } = Array.Empty<ProbabilitiesMax>();
}
