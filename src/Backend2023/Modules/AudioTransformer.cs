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
        string webAudioFileName = $"{Path.GetTempPath()}/{Guid.NewGuid():N}.webm";
        webAudioStream.Seek(0, SeekOrigin.Begin);
        await using (var fileStream = new FileStream(webAudioFileName, FileMode.Create))
        {
            await webAudioStream.CopyToAsync(fileStream);
            await webAudioStream.FlushAsync();
        }

        _logger.LogInformation($"Created audioFile:{webAudioFileName}");

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
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using (Process process = new Process())
        {
            process.StartInfo = psi;
            process.OutputDataReceived += LogOutputData;
            process.ErrorDataReceived += LogErrorData;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cts.Token);

            if (cts.IsCancellationRequested)
            {
                _logger.LogError("Timeout executing ffmpeg after 30sec.");
                process.Close();
            }

            process.CancelOutputRead();
            process.CancelErrorRead();
            process.OutputDataReceived -= LogOutputData;
            process.ErrorDataReceived -= LogErrorData;

            _logger.LogInformation("Process exited with code: " + process.ExitCode);
        }
    }

    private void LogOutputData(object sender, DataReceivedEventArgs e)
    {
        _logger.LogInformation(e.Data);
    }

    private void LogErrorData(object sender, DataReceivedEventArgs e)
    {
        _logger.LogError(e.Data);
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
