using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IHubContext<VoiceHub> _hub;

    public TestController(IHubContext<VoiceHub> hub)
    {
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> SendTestMessage()
    {
        await _hub.Clients.All.SendAsync("ReceiveText", "Hello from backend!");
        return Ok("Sent");
    }
}
