using Backend2023.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace Backend2023.Persistence;

public class Conversations : IDisposable, IConversations
{
    private const string DatabaseName = "helpinghands";

    private const string ContainerName = "clients";

    private const string PartitionKeyPath = "/id";

    private readonly IOptions<ApplicationConfiguration> _options;

    private readonly CosmosClient _client;

    public Conversations(IOptions<ApplicationConfiguration> options)
    {
        _options = options;

        CosmosClientOptions cosmosClientOptions = new()
        {
            // This is only necessary for localhost (Azure CosmosDB Emulator).
            HttpClientFactory = () =>
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                return new HttpClient(httpMessageHandler);
            },
            ConnectionMode = ConnectionMode.Gateway
        };
        _client = new(_options.Value.CosmosDbConnectionString, cosmosClientOptions);
    }

    public async Task AddUserMessage(string clientId, string userMessage)
    {
        await AddMessage(clientId, userMessage, "user");
    }

    public async Task AddResponseMessage(string clientId, string userMessage)
    {
        await AddMessage(clientId, userMessage, "bot");
    }

    private async Task AddMessage(string clientId, string userMessage, string kind)
    {
        Container container = await Container();

        Message message = new()
        {
            Content = userMessage,
            TimeStamp = DateTime.UtcNow,
            Kind = kind
        };

        try
        {
            Client client = await container.ReadItemAsync<Client>(clientId, new PartitionKey(clientId));
            PatchOperation patchOperation = PatchOperation.Add("/Messages/-", message);
            await container.PatchItemAsync<Client>(clientId, new PartitionKey(clientId), new[] { patchOperation });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // We accept the race condition here.
            // In theory we could lose messages but that would be a mistake anyway.
            await container.UpsertItemAsync(new Client
            {
                id = clientId,
                Messages = new[]
                {
                    message
                }
            });
        }
    }

    public async Task<IEnumerable<string>> GetConversation(string clientId)
    {
        Container container = await Container();
        Client client = await container.ReadItemAsync<Client>(clientId, new PartitionKey(clientId));
        return client.Messages.Select(m => m.Content);
    }

    private async Task<Container> Container()
    {
        Database database = await _client.CreateDatabaseIfNotExistsAsync(DatabaseName);
        Container container = await database.CreateContainerIfNotExistsAsync(new ContainerProperties(ContainerName, PartitionKeyPath));
        return container;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
