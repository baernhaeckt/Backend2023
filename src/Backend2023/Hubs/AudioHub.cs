using Backend2023.Cognitive;
using Backend2023.Modules;
using Backend2023.Persistence;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;

namespace Backend2023.Hubs;

public class AudioHub : Hub
{
    private readonly ILogger<AudioHub> _logger;

    private readonly SpeechServiceProvider _speechServiceProvider;
    
    private readonly IChatBot _chatBot;

    private readonly IConversations _conversations;

    private readonly IEmotionDetectionClient _emotionDetectionClient;

    // Dictionary to hold audio data for each client
    private static readonly ConcurrentDictionary<string, MemoryStream> AudioData = new();

    public AudioHub(ILogger<AudioHub> logger, SpeechServiceProvider speechServiceProvider, IChatBot chatBot, IConversations conversations, IEmotionDetectionClient emotionDetectionClient)
    {
        _logger = logger;
        _speechServiceProvider = speechServiceProvider;
        _chatBot = chatBot;
        _conversations = conversations;
        _emotionDetectionClient = emotionDetectionClient;
    }

    public async Task Handshake(string text)
    {
        await Clients.Caller.SendAsync("handshake", $"ack: {text}");
    }

    public async Task SendNickname(string nickname)
    {
        await _conversations.CreateClientAsync(Context.ConnectionId, new Client {Nickname = nickname});
    }

    /// <summary>
    ///     Stream uploaded audio chunks with a single WAV header to a memory stream.
    /// </summary>
    /// <param name="base64AudioData">Audio chunk with a WAV Header of <see cref="WAVHeaderSize"/>.</param>
    /// <returns>Upload Task.</returns>
    public async Task TransmitUserAudio(string base64AudioData)
    {
        var connectionId = Context.ConnectionId;
        var audioDataChunk = Convert.FromBase64String(base64AudioData);
        MemoryStream audioData =  AudioData.GetOrAdd(connectionId, new MemoryStream());
        await audioData.WriteAsync(audioDataChunk);
    }

    /// <summary>
    ///     Closes the uploaded audio stream after verifying the completion.
    /// </summary>
    /// <returns>Completion Task.</returns>
    public async Task CloseAudioStream()
    {
        var connectionId = Context.ConnectionId;
        if (!AudioData.TryRemove(connectionId, out MemoryStream? audio))
        {
            await Clients.Client(connectionId).SendAsync("CloseAudioStreamResponse", 0);
            return;
        }

        string messageId = Guid.NewGuid().ToString("N");
        string waveUserFile = $"query_{messageId}.wav";
        string waveResponseFile = $"response_{messageId}.wav";
        try
        {
            AudioTransformer audioTransformer = AudioTransformer.CreateNew();

            await audioTransformer.TransformWebAudioStreamToWavFile(audio, waveUserFile);
            
            string userMessage = await _speechServiceProvider.AudioToTextAsync(new SpeechToTextRequest(waveUserFile));
            await _conversations.AddUserMessage(connectionId, userMessage);

            string textResponse = await _chatBot.GenerateResponse(userMessage);

            await _conversations.AddResponseMessage(connectionId, textResponse);
            await _speechServiceProvider.TextToWavFile(new TextToSpeedRequest(textResponse, waveResponseFile));
            var byteContent = await File.ReadAllBytesAsync(waveResponseFile);

            await Clients.Caller
                .SendAsync("audioResponse", new AudioResponse(userMessage, textResponse, Convert.ToBase64String(byteContent)));
        }
        finally
        {
            audio?.Dispose();

            if (File.Exists(waveUserFile))
            {
                File.Delete(waveUserFile);
            }

            if (File.Exists(waveResponseFile))
            {
                File.Delete(waveResponseFile);
            }
        }
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Connceted clientId: {connectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogError(exception, "Disconnected clientId: {connectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
