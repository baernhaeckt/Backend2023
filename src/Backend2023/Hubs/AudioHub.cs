using System.IO;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace Backend2023.Hubs;

public class AudioHub : Hub
{
    private const int WAVHeaderSize = 44;

    // Dictionary to hold audio data for each client
    private static readonly Dictionary<string, MemoryStream> AudioData = new();

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
        if (!AudioData.ContainsKey(connectionId))
        {
            AudioData[connectionId] = new MemoryStream();
        }

        await AudioData[connectionId].WriteAsync(audioDataChunk);
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

        string filePath = "audio.webm"; // Replace with your desired file path
        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            // Copy the MemoryStream content to the FileStream
            AudioData[connectionId].Seek(0, SeekOrigin.Begin);
            await AudioData[connectionId].CopyToAsync(fileStream);
        }

        AudioData.Remove(connectionId);

        // TODO: Generate Response 
    }
}
