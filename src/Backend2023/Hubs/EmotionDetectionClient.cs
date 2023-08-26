using System.Net.Http.Headers;

namespace Backend2023.Hubs;

public class EmotionDetectionClient : IEmotionDetectionClient
{
    private readonly HttpClient _httpClient;

    public EmotionDetectionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EmotionDetectionResponse> ExecuteEmotionDetection(string filepath)
    {
        var content = new MultipartFormDataContent();
        using FileStream fileStream = new(filepath, FileMode.Open);
        using StreamContent streamContent = new(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        content.Add(streamContent, "file", Path.GetFileName(fileStream.Name));
        HttpResponseMessage response = await _httpClient.PostAsync("/api/v1/emotion-detection/audio", content);
        response.EnsureSuccessStatusCode();

        EmotionDetectionResponse? result = await response.Content.ReadFromJsonAsync<EmotionDetectionResponse>();
        return result!; // Nobody knows when this can be null..
    }
}
