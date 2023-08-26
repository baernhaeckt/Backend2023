﻿using Backend2023.Common;
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

    public async Task<byte[]> TextToAudioByteArrayAsync(TextToSpeedRequest textToSpeedRequest)
    {
        using var result = await Synthesize(textToSpeedRequest);
        if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
            throw new InvalidOperationException($"Cancelled(Error:{cancellation.ErrorCode},Details:{cancellation.ErrorDetails}");
        }

        using AudioDataStream audioStream = AudioDataStream.FromResult(result);
        return result.AudioData;
    }

    public async Task<string> AudioToTextAsync(SpeechToTextRequest request)
    {
        var audioConfig = AudioConfig.FromWavFileInput(request.FileName);
        SpeechRecognizer recognizer = CreateRecognizer(request, audioConfig);
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
        speechConfig.SetProxy("localhost", 3128);
        speechConfig.SpeechSynthesisLanguage = speechRequest.Language;
        speechConfig.SpeechRecognitionLanguage = speechRequest.Language;
        speechConfig.SpeechSynthesisVoiceName = speechRequest.Voice;

        return speechConfig;
    }

    private SpeechSynthesizer CreateSynthesizer(TextToSpeedRequest request) 
        => new(CreateSpeechConfig(request));

    private SpeechRecognizer CreateRecognizer(SpeechToTextRequest request, AudioConfig audioConfig) 
        => new(CreateSpeechConfig(request), audioConfig);

    private Task<SpeechSynthesisResult> Synthesize(TextToSpeedRequest textToSpeedRequest)
        => textToSpeedRequest.IsSpeechSynthesisMarkupLanguage 
            ? CreateSynthesizer(textToSpeedRequest).SpeakSsmlAsync(textToSpeedRequest.Text) 
            : CreateSynthesizer(textToSpeedRequest).SpeakTextAsync(textToSpeedRequest.Text);
}