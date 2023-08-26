using Backend2023.Cognitive;
using Backend2023.Modules;
using Backend2023.Persistence;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Backend2023.Hubs;

public class AudioHub : Hub
{
    private readonly ILogger<AudioHub> _logger;

    private readonly SpeechServiceProvider _speechServiceProvider;

    private readonly AudioTransformer _audioTransformer;

    private readonly IChatBot _chatBot;

    private readonly IConversations _conversations;

    private readonly IEmotionDetectionClient _emotionDetectionClient;

    // Dictionary to hold audio data for each client
    private static readonly ConcurrentDictionary<string, MemoryStream> AudioData = new();

    public AudioHub(ILogger<AudioHub> logger, 
        SpeechServiceProvider speechServiceProvider,
        AudioTransformer audioTransformer,
        IChatBot chatBot, 
        IConversations conversations, 
        IEmotionDetectionClient emotionDetectionClient)
    {
        _logger = logger;
        _speechServiceProvider = speechServiceProvider;
        _audioTransformer = audioTransformer;
        _chatBot = chatBot;
        _conversations = conversations;
        _emotionDetectionClient = emotionDetectionClient;
    }

    public async Task Handshake(string text)
    {
        _logger.LogInformation("Handshake clientId: {connectionId}", Context.ConnectionId);
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
        var audioDataChunk = Convert.FromBase64String(base64AudioData);
        MemoryStream audioData = AudioData.GetOrAdd(Context.ConnectionId, new MemoryStream());
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
            await using var fileStream = new FileStream($"{messageId}.webm", FileMode.Create);
            audio.Seek(0, SeekOrigin.Begin);
            await audio.CopyToAsync(fileStream);

            await _audioTransformer.TransformWebAudioStreamToWavFileWithFfmpeg(audio, waveUserFile);

            // We run the emotion detection async and await it later because we don't depend on it here.
            Task<EmotionDetectionResponse> emotionDetectionTask = _emotionDetectionClient.ExecuteEmotionDetection(waveUserFile);

            // Get the text from the users voice message using Azure Cognitive Services
            // and safe it to the conversation.
            string userMessage = await _speechServiceProvider.AudioToTextAsync(new SpeechToTextRequest(waveUserFile));
            await _conversations.AddUserMessage(connectionId, userMessage);

            // Generate the response using Open AI and save it to the conversation.
            // Afterwards, use the Azure Cognitive Services to generate the audio.
            string textResponse = await _chatBot.GenerateResponse(userMessage);
            Task addResponseToConversation = _conversations.AddResponseMessage(connectionId, textResponse);
            Task textToWavFile = _speechServiceProvider.TextToWavFile(new TextToSpeedRequest(textResponse, waveResponseFile));
            await Task.WhenAll(addResponseToConversation, textToWavFile, emotionDetectionTask);

            byte[] byteContent = await File.ReadAllBytesAsync(waveResponseFile);
            EmotionDetectionResponse emotionDetectionResponse = await emotionDetectionTask;

            Task emotionsResponse = Clients.Caller.SendAsync("emotions", emotionDetectionResponse);
            AudioResponse response = new(userMessage, textResponse, Convert.ToBase64String(byteContent));
            Task audioResponse = Clients.Caller.SendAsync("audioResponse", response);
            await Task.WhenAll(emotionsResponse, audioResponse);
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
