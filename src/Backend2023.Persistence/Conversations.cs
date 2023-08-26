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
        Container container = await Container();
        await container.UpsertItemAsync(new Client
        {
            id = clientId,
            Messages = new[] { userMessage }
        });
    }

    public async Task AddResponseMessage(string clientId, string userMessage)
    {
        Container container = await Container();
        // TODO: Append to the user message sub document
    }

    public async Task<IEnumerable<string>> GetConversation(string clientId)
    {
        Container container = await Container();
        Client client = await container.ReadItemAsync<Client>(clientId, new PartitionKey(clientId));
        return client.Messages; 
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
