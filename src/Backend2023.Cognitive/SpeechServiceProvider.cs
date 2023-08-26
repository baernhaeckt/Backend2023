using Backend2023.Common;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;

namespace Backend2023.Cognitive;

public class SpeechServiceProvider
{
    private readonly IOptions<ApplicationConfiguration> _options;

    public SpeechServiceProvider(IOptions<ApplicationConfiguration> options)
    {
        _options = options;
    }

    public async Task TextToWavFile(TextToSpeedRequest textToSpeedRequest)
    {
        using var result = await Synthesize(textToSpeedRequest);
        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            throw new InvalidOperationException($"Cancelled(Error:{cancellation.ErrorCode},Details:{cancellation.ErrorDetails}");
        }
        
        using var audioStream = AudioDataStream.FromResult(result);
        await audioStream.SaveToWaveFileAsync(textToSpeedRequest.OutFileName);
    }

    public async Task<string> AudioToTextAsync(SpeechToTextRequest request)
    {
        using var audioConfig = AudioConfig.FromWavFileInput(request.FileName);
        using SpeechRecognizer recognizer = CreateRecognizer(request, audioConfig);
        SpeechRecognitionResult? result = await recognizer.RecognizeOnceAsync();

        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

            if (cancellation.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
            }
        }

        return result.Text;
    }

    private SpeechConfig CreateSpeechConfig(SpeechRequest speechRequest)
    {
        SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Value.AzureAIServicesKey, "westeurope");
        if (!string.IsNullOrWhiteSpace(_options.Value.ProxyHost))
        {
            speechConfig.SetProxy(_options.Value.ProxyHost, _options.Value.ProxyPort!.Value);
        }
        speechConfig.SpeechSynthesisLanguage = speechRequest.Language;
        speechConfig.SpeechRecognitionLanguage = speechRequest.Language;
        speechConfig.SpeechSynthesisVoiceName = speechRequest.Voice;

        return speechConfig;
    }

    private SpeechSynthesizer CreateSynthesizer(TextToSpeedRequest request) 
        => new(CreateSpeechConfig(request), AudioConfig.FromStreamOutput(new PullAudioOutputStream()));

    private SpeechRecognizer CreateRecognizer(SpeechToTextRequest request, AudioConfig audioConfig) 
        => new(CreateSpeechConfig(request), audioConfig);

    private Task<SpeechSynthesisResult> Synthesize(TextToSpeedRequest textToSpeedRequest)
        => textToSpeedRequest.IsSpeechSynthesisMarkupLanguage 
            ? CreateSynthesizer(textToSpeedRequest).SpeakSsmlAsync(textToSpeedRequest.Text) 
            : CreateSynthesizer(textToSpeedRequest).SpeakTextAsync(textToSpeedRequest.Text);
}