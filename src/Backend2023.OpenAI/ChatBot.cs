using OpenAI.API.Completions;
using OpenAI.API;
using Microsoft.Extensions.Options;
using Backend2023.Common;

namespace Backend2023.Modules;

public class ChatBot : IChatBot
{
    private readonly IOptions<ApplicationConfiguration> _options;

    public ChatBot(IOptions<ApplicationConfiguration> options)
    {
        _options = options;
    }

    public async Task<string> GenerateResponse(string userMessage)
    {
        string query = CreateQuery(userMessage);

        OpenAIAPI openai = new(_options.Value.OpenAIKey);
        CompletionRequest completionRequest = new()
        {
            Prompt = query,
            Model = "gpt-3.5-turbo",
            MaxTokens = 10
        };

        var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

        string outputResult = "";
        foreach (var completion in completions.Completions)
        {
            outputResult += completion.Text;
        }

        return outputResult;
    }

    private static string CreateQuery(string userMessage)
    {
        // TODO: Create a promt to return the desired result.
        return "Schreibe ein Haiku";
    }
}
