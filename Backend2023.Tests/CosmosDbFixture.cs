using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace Backend2023.Tests;

public class CosmosDbFixture
{
    [Fact]
    public async Task CosmosDb_ShouldPerformOperations()
    {
        CosmosClientOptions options = new()
        {
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
        CosmosClient client = new("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", options);
        Database database = await client.CreateDatabaseIfNotExistsAsync("helpinghands");
        Container container = await database.CreateContainerIfNotExistsAsync(new ContainerProperties("clients", "/id"));
        Client clientItem = new() { id = Guid.NewGuid().ToString() };
        await container.CreateItemAsync(clientItem);
        var clientFromContainer = await container.ReadItemAsync<Client>(clientItem.id, new PartitionKey(clientItem.id));
        clientFromContainer.Resource.id.Should().Be(clientItem.id);
    }
}

class Client
{
    public string id { get; set; } = string.Empty;
}