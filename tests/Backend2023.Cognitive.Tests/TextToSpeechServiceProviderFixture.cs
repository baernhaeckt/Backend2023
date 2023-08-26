using FluentAssertions;

namespace Backend2023.Cognitive.Tests;

public class TextToSpeechServiceProviderFixture
{
    private readonly AzureConfiguration _azureConfiguration = new()
    {
        ServiceRegion = "westeurope",
        SubscriptionKey = "098a9c0a3b1648ffb4ae57288c58d827"
    };

    private readonly SpeechServiceProvider _speechServiceProvider;

    public TextToSpeechServiceProviderFixture()
    {
        _speechServiceProvider = new SpeechServiceProvider(_azureConfiguration);
    }
    
    [Fact]
    public async Task Should_GenerateSpeech()
    {
        // Arrange
        TextToSpeedRequest request = new("Ich bin die Leni, und ich mag es durch den Regen zu tanzen.");

        // Act
        byte[] result = await _speechServiceProvider.TextToAudioByteArrayAsync(request);

        // Assert
        result.Should().NotBeNull();
    }


    [Fact]
    public async Task Should_GenerateText()
    {
        // Arrange

        // Act
        string result = await _speechServiceProvider.AudioToTextAsync(new SpeechToTextRequest(null!));

        // Assert
        result.Should().NotBeNull();
    }
}