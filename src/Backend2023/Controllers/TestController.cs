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

    [HttpGet("Conversation")]
    public async Task<IEnumerable<string>> GetConversationAsync([FromServices] IConversations conversations, string clientId)
    {
        return await conversations.GetConversation(clientId);
    }
}
