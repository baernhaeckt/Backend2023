namespace Backend2023.Persistence;

public class Client
{
    public string id { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public IEnumerable<Message> Messages { get; set; } = Enumerable.Empty<Message>();
}
