﻿namespace Backend2023.Common;

public class ApplicationConfiguration
{
    public required string OpenAIKey { get; set; }

    public required string CosmosDbConnectionString { get; set; }

    public required string AzureAIServicesKey { get; set; }

    public required string MlServiceUrl { get; set; }

    public string? ProxyHost { get; set; }

    public int? ProxyPort { get; set; }
}