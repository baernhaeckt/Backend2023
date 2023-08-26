namespace Backend2023.Hubs;

public interface IEmotionDetectionClient
{
    Task<EmotionDetectionResponse> ExecuteEmotionDetection(string filepath);
}