namespace Backend2023.Persistence;

public interface IConversations
{
    Task AddResponseMessage(string clientId, string userMessage);

    Task AddUserMessage(string clientId, string userMessage);

    Task CreateClientAsync(string clientId, Client client);

    Task<IEnumerable<string>> GetConversation(string clientId);
}