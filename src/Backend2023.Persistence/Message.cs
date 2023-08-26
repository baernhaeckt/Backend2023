namespace Backend2023.Persistence;

public class Message
{
    public DateTime TimeStamp { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;
}