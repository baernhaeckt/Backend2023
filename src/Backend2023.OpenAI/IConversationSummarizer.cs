namespace Backend2023.OpenAI;

public interface IConversationSummarizer
{
    Task<string> Summarize(IEnumerable<string> conversation);
}