using OpenAI.API;
using OpenAI.API.Completions;

namespace Backend2023.Tests;

public class OpenAIFixture
{
    [Fact]
    public async Task OpenAI_ShouldPerformOperations()
    {
        string query = "Schreibe ein Haiku";
        string outputResult = "";

        var openai = new OpenAIAPI("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");
        CompletionRequest completionRequest = new()
        {
            Prompt = query,
            Model = "gpt-3.5-turbo",
            MaxTokens = 10
        };

        var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

        foreach (var completion in completions.Completions)
        {
            outputResult += completion.Text;
        }

        Assert.Fail(outputResult);
    }
}
