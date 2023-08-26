using Backend2023.Common;
using Microsoft.Extensions.Options;

namespace Backend2023.Cognitive.Tests;

public class TextToSpeechServiceProviderFixture
{
    private readonly SpeechServiceProvider _speechServiceProvider;

    public TextToSpeechServiceProviderFixture()
    {
        var applicationConfiguration = new ApplicationConfiguration
        {
            AzureAIServicesKey = "098a9c0a3b1648ffb4ae57288c58d827",
            CosmosDbConnectionString = "dummy",
            OpenAIKey = "dummy",
            MlServiceUrl = null!
        };

        IOptions<ApplicationConfiguration> applicationConfigurationOptions = Options.Create(applicationConfiguration);
        _speechServiceProvider = new SpeechServiceProvider(applicationConfigurationOptions);
    }

    [Fact]
    public async Task Should_GenerateSpeech()
    {
        // Arrange
        TextToSpeedRequest request = new("Ich bin die Leni, und ich mag es durch den Regen zu tanzen.", "test.wav");

        // Act
        await _speechServiceProvider.TextToWavFile(request);

        // Assert
    }
    [Fact]
    public async Task Should_GenerateSpeech_WithSSML()
    {
        // Arrange
        string ssmlText = @"<speak version=""1.0"" xmlns=""http://www.w3.org/2001/10/synthesis"" xml:lang=""en-GB"">
  <voice name=""en-GB-HollieNeural"">
    Hello, this is an example of using SSML.
    <prosody rate=""slow"">This part is spoken slowly.</prosody>
    <prosody pitch=""-10%"">This part is spoken with a lower pitch.</prosody>
    <break time=""500ms""/> <!-- Pause for 500 milliseconds -->
    Now I'm going to spell a word: <say-as interpret-as=""characters"">SSML</say-as>.
    <emphasis level=""strong"">This part is emphasized strongly.</emphasis>
  </voice>
</speak>";
        TextToSpeedRequest request = new TextToSpeedRequest(ssmlText, "test.wav", IsSpeechSynthesisMarkupLanguage: true);

        // Act
        await _speechServiceProvider.TextToWavFile(request);

        // Assert
    }
}