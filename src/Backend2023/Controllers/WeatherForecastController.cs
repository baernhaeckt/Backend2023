using Microsoft.AspNetCore.Mvc;
using OpenAI.API.Completions;
using OpenAI.API;

namespace Backend2023.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetAsync()
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

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = outputResult
        })
        .ToArray();
    }
}
