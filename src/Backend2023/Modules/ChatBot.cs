namespace Backend2023.Modules;

public class ChatBot
{
    public Task<string> GenerateResponse(string userId, string userMessage)
    {
        return Task.FromResult($"Du hast gesagt, dass: {userMessage}");
    }
}
