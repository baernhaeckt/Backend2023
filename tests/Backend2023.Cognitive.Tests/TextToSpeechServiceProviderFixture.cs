using FluentAssertions;

namespace Backend2023.Cognitive.Tests
{
    public class TextToSpeechServiceProviderFixture
    {
        private readonly AzureConfiguration _azureConfiguration = new()
        {
            ServiceRegion = "westeurope",
            //SubscriptionKey = "67ff4bd3-1dcc-44ae-80a8-65b1251fbd2b"
            SubscriptionKey = "098a9c0a3b1648ffb4ae57288c58d827"
        };

        private SpeechServiceProvider _speechServiceProvider;

        public TextToSpeechServiceProviderFixture()
        {
            _speechServiceProvider = new SpeechServiceProvider(_azureConfiguration);
        }
        
        [Fact]
        public async Task Should_GenerateSpeech()
        {
            // Arrange
            TextToSpeedRequest request = new TextToSpeedRequest("Ich bin die Leni, und ich mag es durch den Regen zu tanzen.");

            // Act
            byte[] result = await _speechServiceProvider.TextToAudioByteArrayAsync(request);

            // Assert
            result.Should().NotBeNull();
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
            TextToSpeedRequest request = new TextToSpeedRequest(ssmlText, IsSpeechSynthesisMarkupLanguage: true);

            // Act
            byte[] result = await _speechServiceProvider.TextToAudioByteArrayAsync(request);

            // Assert
            result.Should().NotBeNull();
        }
    }
}