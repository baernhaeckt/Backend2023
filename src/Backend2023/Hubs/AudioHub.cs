using Microsoft.AspNetCore.SignalR;

namespace Backend2023.Hubs;

public class AudioHub : Hub
{
    private const int WAVHeaderSize = 44;

    // Dictionary to hold audio data for each client
    private static readonly Dictionary<string, MemoryStream> AudioData = new();

    /// <summary>
    ///     Stream uploaded audio chunks with a single WAV header to a memory stream.
    /// </summary>
    /// <param name="audioDataChunk">Audio chunk with a WAV Header of <see cref="WAVHeaderSize"/>.</param>
    /// <returns>Upload Task.</returns>
    public async Task TransmitUserAudio(byte[] audioDataChunk)
    {
        var connectionId = Context.ConnectionId;
        if (!AudioData.ContainsKey(connectionId))
        {
            AudioData[connectionId] = new MemoryStream();
            await AudioData[connectionId].WriteAsync(audioDataChunk);
        }
        else
        {
            await AudioData[connectionId].WriteAsync(audioDataChunk, WAVHeaderSize, audioDataChunk.Length - WAVHeaderSize);
        }
    }

    /// <summary>
    ///     Closes the uploaded audio stream after verifying the completion.
    /// </summary>
    /// <returns>Completion Task.</returns>
    public async Task CloseAudioStream()
    {
        var connectionId = Context.ConnectionId;
        if (!AudioData.ContainsKey(connectionId))
        {
            await Clients.Client(connectionId).SendAsync("CloseAudioStreamResponse", 0);
            return;
        }

        await Clients.Client(connectionId).SendAsync("CloseAudioStreamResponse", AudioData[connectionId].Length);

        // TODO: Generate Response 
    }
}
