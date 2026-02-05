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
        var (audio, mime) = await _tts.GenerateAudioAsync(req.Text);
        
        if (audio == null || audio.Length < 100) { 
            return BadRequest("TTS engine returned invalid audio."); 
        }

        return File(audio, mime);
    }
}
