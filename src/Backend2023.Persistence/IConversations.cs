namespace Backend2023.Persistence;

public interface IConversations
{
    Task AddResponseMessage(string clientId, string userMessage);
    Task AddUserMessage(string clientId, string userMessage);
    Task<Client> GetConversation(string clientId);
}