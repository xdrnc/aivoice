using Microsoft.AspNetCore.Mvc;
using VoiceAgent.Models;


[ApiController]
[Route("tts")]
public class TtsController : ControllerBase
{
    private readonly ITtsEngine _tts;

    public TtsController(ITtsEngine tts)
    {
        _tts = tts;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TtsRequest req)
    {
        var audio = await _tts.GenerateAudioAsync(req.Text);
        return File(audio, "audio/wav");
    }
}
