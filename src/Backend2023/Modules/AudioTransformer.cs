using System.Diagnostics;
using System.Net.Http.Headers;
using Backend2023.Hubs;
using NAudio.Wave;

namespace Backend2023.Modules;

/// <summary>
///     Transforms webm(gocc) to and from wav.
/// </summary>
public class AudioTransformer
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AudioTransformer> _logger;

    public AudioTransformer(HttpClient httpClient, ILogger<AudioTransformer> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task TransformWebAudioStreamToWavFileWithFfmpeg(Stream webAudioStream, string wavFileName)
    {
        string webAudioFileName = $"{Path.GetTempFileName()}";
        await using (var fileStream = new FileStream(webAudioFileName, FileMode.Create))
        {
            await webAudioStream.CopyToAsync(fileStream);
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ffmpeg", // Use the shell executable (bash)
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"-i {webAudioFileName} {wavFileName}" // Pass the command as an argument to the shell
        };

        // Start the process
        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.Start();                // Handle the output and error streams
            process.OutputDataReceived += (sender, e) => _logger.LogInformation(e.Data);
            process.ErrorDataReceived += (sender, e) => _logger.LogError(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            _logger.LogInformation("Process exited with code: " + process.ExitCode);
        }
    }

    public async Task TransformWebAudioStreamToWavFileOld(Stream webAudioStream, string fileName)
    {
        webAudioStream.Seek(0, SeekOrigin.Begin);
        await using var fileStream = new FileStream(fileName, FileMode.Create);
        await using StreamMediaFoundationReader mediaFoundationReader = new StreamMediaFoundationReader(webAudioStream);
        await using WaveFileWriter waveWriter = new WaveFileWriter(fileStream, mediaFoundationReader.WaveFormat);
        await mediaFoundationReader.CopyToAsync(waveWriter);
        await waveWriter.FlushAsync();
    }
}
