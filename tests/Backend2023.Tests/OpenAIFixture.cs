
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Backend2023.Tests;
/// <summary>
/// 
/// </summary>
/// <see href="https://platform.openai.com/docs/models/model-endpoint-compatibility">
public class OpenAIFixture
{
    [Fact]
    public async Task OpenAI_ShouldPerformOperations()
    {
        string userInput = "Mein Tag heute war sehr anstrengend, mein Kollege hat mir gesagt ich stinke. Das hat mich wütend gemacht.";
        string? outputResult = "";

        var api = new OpenAI_API.OpenAIAPI("--secret--");
        ChatRequest chatRequest = new()
        {
            Model = Model.ChatGPTTurbo,
            Messages = new List<ChatMessage>
            {
                new(ChatMessageRole.System, "Du bist ein erfahrener psychologe, dir werden persönliche fragen gestellt die du mit einer kurzen einfühlsamen antwort beantworten sollst."),
                new(ChatMessageRole.User, userInput)
            },
            Temperature = 0.9,
            MaxTokens = 200
        };

        ChatResult result = await api.Chat.CreateChatCompletionAsync(chatRequest);

        outputResult = result.Choices.FirstOrDefault()?.Message.Content;

        Assert.Fail(outputResult);
    }
}
