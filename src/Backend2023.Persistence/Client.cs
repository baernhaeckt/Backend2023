namespace Backend2023.Persistence;

public class Client
{
    public string id { get; set; } = string.Empty;

    public IEnumerable<string> Messages { get; set; } = Enumerable.Empty<string>();
}