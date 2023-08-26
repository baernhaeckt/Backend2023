namespace Backend2023.Cognitive;

public class AzureConfiguration
{
    public const string Path = $"{nameof(AzureConfiguration)}";

    public string SubscriptionKey { get; init; } = null!;

    public string ServiceRegion { get; init; } = null!;
}