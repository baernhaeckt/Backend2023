using Backend2023.Common;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace Backend2023.Modules;

public class ChatBot : IChatBot
{
    private static readonly string[] _systemPrompts =
    {
        "Du bist ein erfahrener psychologe, dir werden persönliche fragen gestellt die du mit einer kurzen einfühlsamen antwort beantworten sollst."
    };

    private readonly IOptions<ApplicationConfiguration> _options;

    public ChatBot(IOptions<ApplicationConfiguration> options)
    {
        _options = options;
    }

    private OpenAIAPI OpenAIAPI
        => new(_options.Value.OpenAIKey);

    public async Task<string> GenerateResponse(string userMessage)
    {
        var result = await OpenAIAPI.Chat.CreateChatCompletionAsync(CreateChatRequest(userMessage));
        return result.Choices.FirstOrDefault()?.Message.Content ?? "Häää?";
    }

    private static ChatRequest CreateChatRequest(string userMessage)
    {
        var messages = new List<ChatMessage>(_systemPrompts.Select(msg => new ChatMessage(ChatMessageRole.System, msg)));
        messages.Add(new ChatMessage(ChatMessageRole.User, userMessage));

        return new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Messages = messages,
            Temperature = 0,
            MaxTokens = 200
        };
    }
}