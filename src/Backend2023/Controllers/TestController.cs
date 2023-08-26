using Backend2023.Hubs;
using Backend2023.Modules;
using Backend2023.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Backend2023.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpPost("Message")]
    public async Task<string> GetAsync([FromServices] IChatBot chatBot, string message)
    {
        return await chatBot.GenerateResponse(message);
    }

    [HttpPost("AddRequest")]
    public async Task AddRequest([FromServices] IConversations conversations, string clientId, string message)
    {
        await conversations.AddUserMessage(clientId, message);
    }

    [HttpPost("EmotionDetection")]
    public async Task EmotionDetection([FromServices] IEmotionDetectionClient client, IFormFile formFile)
    {
        FileInfo fileInfo = new(Path.GetTempFileName());
        using(Stream stream = fileInfo.OpenWrite())
        {
            await formFile.CopyToAsync(stream);
        }

        await client.ExecuteEmotionDetection(fileInfo.FullName);
    }

    [HttpGet("Conversation")]
    public async Task<IEnumerable<string>> GetConversationAsync([FromServices] IConversations conversations, string clientId)
    {
        return await conversations.GetConversation(clientId);
    }
}
