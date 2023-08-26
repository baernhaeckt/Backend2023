using Backend2023.Hubs;
using System.Text.Json;

namespace Backend2023.Tests;

public class EmotionDetectionResponseFixture
{
    private const string ExampleJson = """
          {
            "predicted_classes": [ "neutral" ],
            "probabilities_max": [
            {
              "class": "anger",
              "probability": 50.00000596046448
            },
            {
              "class": "anxiety",
              "probability": 50.00000596046448
            },
            {
              "class": "boredom",
              "probability": 73.05106520652771
            },
            {
              "class": "disgust",
              "probability": 50.91985464096069
            },
            {
              "class": "happiness",
              "probability": 51.71198844909668
            },
            {
              "class": "neutral",
              "probability": 73.10585975646973
            },
            {
              "class": "sadness",
              "probability": 61.59616708755493
            }
          ]
        }
        """;

    [Fact]
    public void ShouldDeserializeExampleJson()
    {
        // Arrange & Act
        EmotionDetectionResponse? result = JsonSerializer.Deserialize<EmotionDetectionResponse>(ExampleJson);

        // Assert
        result.Should().NotBeNull();
        result!.PredictedClasses.Should().ContainSingle(s => s == "neutral");
    }
}
