namespace Backend2023.Modules;

public interface IChatBot
{
    Task<string> GenerateResponse(string userMessage);
}