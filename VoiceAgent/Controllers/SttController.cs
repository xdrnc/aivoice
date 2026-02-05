using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using VoiceAgent.Config;

[ApiController]
[Route("[controller]")]
public class SttController : ControllerBase
{
    private readonly IHubContext<VoiceHub> _hub;
    private readonly WhisperSettings _settings;

    public SttController(IHubContext<VoiceHub> hub, IOptions<WhisperSettings> settings)
    {
        _hub = hub;
        _settings = settings.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Transcribe([FromForm] IFormFile audio)
    {
        if (audio == null || audio.Length == 0)
            return BadRequest("No audio uploaded.");

        var tempDir = Path.Combine(Path.GetTempPath(), "voiceagent");
        Directory.CreateDirectory(tempDir);

        var inputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.webm");
        using (var stream = System.IO.File.Create(inputPath))
        {
            await audio.CopyToAsync(stream);
        }

        var psi = new ProcessStartInfo
        {
            FileName = _settings.ExecutablePath,
            Arguments = $"-m \"{_settings.ModelPath}\" -f \"{inputPath}\" --no-timestamps",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(error))
            Console.WriteLine("Whisper error: " + error);

        string transcription = ExtractWhisperText(output);

        await _hub.Clients.All.SendAsync("ReceiveSTT", transcription);

        return Ok(new { text = transcription });
    }

    private string ExtractWhisperText(string output)
    {
        var lines = output.Split('\n');
        var textLines = lines
            .Where(l => l.Contains("]"))
            .Select(l => l.Substring(l.IndexOf("]") + 1).Trim());

        return string.Join(" ", textLines);
    }
}
