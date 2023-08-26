namespace Backend2023;

public class ApplicationConfiguration
{
    public required string OpenAIKey { get; set; }

    public required string CosmosDbConnectionString { get; set; }

    public required string AzureAIServicesKey { get; set; }
}