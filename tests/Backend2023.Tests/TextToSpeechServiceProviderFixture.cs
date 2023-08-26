using Backend2023.Common;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Backend2023.Cognitive.Tests;

public class TextToSpeechServiceProviderFixture
{
    private readonly SpeechServiceProvider _speechServiceProvider;

    public TextToSpeechServiceProviderFixture()
    {
        var applicationConfiguration = new ApplicationConfiguration()
        {
            AzureAIServicesKey = "098a9c0a3b1648ffb4ae57288c58d827",
            CosmosDbConnectionString = "dummy",
            OpenAIKey = "dummy"
        };

        IOptions<ApplicationConfiguration> applicationConfigurationOptions = Options.Create(applicationConfiguration);
        _speechServiceProvider = new SpeechServiceProvider(applicationConfigurationOptions);
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