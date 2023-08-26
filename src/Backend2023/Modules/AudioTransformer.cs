using NAudio.Wave;

namespace Backend2023.Modules;

/// <summary>
///     Transforms webm(gocc) to and from wav.
/// </summary>
public class AudioTransformer
{
    public static AudioTransformer CreateNew()
        => new();

    public async Task TransformWebAudioStreamToWavFile(Stream webAudioStream, string fileName)
    {
        webAudioStream.Seek(0, SeekOrigin.Begin);
        await using var fileStream = new FileStream(fileName, FileMode.Create);
        await using StreamMediaFoundationReader mediaFoundationReader = new StreamMediaFoundationReader(webAudioStream);
        await using WaveFileWriter waveWriter = new WaveFileWriter(fileStream, mediaFoundationReader.WaveFormat);
        await mediaFoundationReader.CopyToAsync(waveWriter);
        await waveWriter.FlushAsync();
    }
}
