using Backend2023.Cognitive;
using Backend2023.Modules;
using Backend2023.Persistence;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Backend2023.Hubs;

public class AudioHub : Hub
{
    private readonly SpeechServiceProvider _speechServiceProvider;
    
    private readonly IChatBot _chatBot;

    private readonly IConversations _conversations;

    // Dictionary to hold audio data for each client
    private static readonly ConcurrentDictionary<string, MemoryStream> AudioData = new();

    public AudioHub(SpeechServiceProvider speechServiceProvider, IChatBot chatBot, IConversations conversations)
    {
        _speechServiceProvider = speechServiceProvider;
        _chatBot = chatBot;
        _conversations = conversations;
    }

    public async Task Handshake(string text)
    {
        await Clients.Caller.SendAsync("handshake", $"ack: {text}");
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
        AudioTransformer audioTransformer = AudioTransformer.CreateNew();

        await audioTransformer.TransformWebAudioStreamToWavFile(audio, waveUserFile);
        string userMessage = await _speechServiceProvider.AudioToTextAsync(new SpeechToTextRequest(waveUserFile));
        await _conversations.AddUserMessage(connectionId, userMessage);
        string textResponse = await _chatBot.GenerateResponse(userMessage);
        await _conversations.AddResponseMessage(connectionId, userMessage);
        await _speechServiceProvider.TextToWavFile(new TextToSpeedRequest(textResponse, waveResponseFile));
        audio?.DisposeAsync();
        var byteContent = await File.ReadAllBytesAsync(waveResponseFile);

        File.Delete(waveUserFile);
        File.Delete(waveResponseFile);

        await Clients.Caller.SendAsync("audioResponse", new AudioResponse(
            userMessage,
            textResponse,
            Convert.ToBase64String(byteContent)));
    }
}
