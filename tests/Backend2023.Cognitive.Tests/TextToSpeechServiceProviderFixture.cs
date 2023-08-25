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
            byte[] result = await _speechServiceProvider.TextToAudioByteArray(request);

            // Assert
            result.Should().NotBeNull();
        }


        [Fact]
        public async Task Should_GenerateText()
        {
            // Arrange

            // Act
            string result = await _speechServiceProvider.AudioToText(new SpeechToTextRequest(null!));

            // Assert
            result.Should().NotBeNull();
        }
    }
}