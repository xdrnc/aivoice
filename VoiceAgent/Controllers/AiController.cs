using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

[ApiController]
[Route("ai")]
public class AiController : ControllerBase
{
    private readonly IHubContext<VoiceHub> _hub;

    public AiController(IHubContext<VoiceHub> hub)
    {
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AiRequest req)
    {
        var client = new HttpClient();

        var body = new
        {
            model = "llama3.1",
            stream = true,   // <-- enable streaming
            messages = new[]
            {
                new { role = "user", content = req.Text }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/chat")
        {
            Content = JsonContent.Create(body)
        };

        // IMPORTANT: do not buffer the whole response
        var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead
        );

        var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Each line is a JSON chunk
            var chunk = JsonSerializer.Deserialize<OllamaResponse>(line);

            var partial = chunk?.message?.content;
            if (!string.IsNullOrEmpty(partial))
            {
                // Send partial text to React
                await _hub.Clients.All.SendAsync("ReceiveText", partial);
            }
        }

        return Ok();
    }


    public class AiRequest
    {
        public string Text { get; set; }
    }

    public class OllamaResponse
    {
        public OllamaMessage message { get; set; }
    }

    public class OllamaMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}