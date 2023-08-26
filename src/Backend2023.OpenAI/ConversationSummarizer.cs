﻿using Backend2023.Common;
using Microsoft.Extensions.Options;
using OpenAI.API.Completions;
using OpenAI.API;

namespace Backend2023.OpenAI;

public class ConversationSummarizer : IConversationSummarizer
{
    private readonly IOptions<ApplicationConfiguration> _options;

    public ConversationSummarizer(IOptions<ApplicationConfiguration> options)
    {
        _options = options;
    }

    public async Task<string> Summarize(IEnumerable<string> conversation)
    {
        string query = CreateQuery(conversation);

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

    private static string CreateQuery(IEnumerable<string> userMessage)
    {
        // TODO: Create a promt to return the desired result.
        return "Schreibe ein Haiku";
    }
}
