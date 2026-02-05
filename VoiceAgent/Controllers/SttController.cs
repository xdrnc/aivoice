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

    // Save incoming WebM file
    var inputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.webm");
    using (var stream = System.IO.File.Create(inputPath))
    {
        await audio.CopyToAsync(stream);
    }

    // Convert WebM → WAV using FFmpeg
    var wavPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.wav");

    var ffmpeg = new ProcessStartInfo
    {
        FileName = "ffmpeg",
        Arguments = $"-y -i \"{inputPath}\" -ar 16000 -ac 1 -f wav \"{wavPath}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    var ffmpegProcess = Process.Start(ffmpeg);
    string ffmpegErr = await ffmpegProcess.StandardError.ReadToEndAsync();
    await ffmpegProcess.WaitForExitAsync();

    Console.WriteLine("FFmpeg conversion output:");
    Console.WriteLine(ffmpegErr);

    // Now run Whisper on the WAV file
    var psi = new ProcessStartInfo
    {
        FileName = _settings.ExecutablePath,
        Arguments = $"-m \"{_settings.ModelPath}\" -f \"{wavPath}\" --no-timestamps",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    var process = Process.Start(psi);
    string output = await process.StandardOutput.ReadToEndAsync();

    Console.WriteLine("Whisper output:");
    Console.WriteLine(output);

    string error = await process.StandardError.ReadToEndAsync();
    process.WaitForExit();

    if (!string.IsNullOrWhiteSpace(error))
        Console.WriteLine("Whisper error: " + error);

    string transcription = ExtractWhisperText(output);

    await _hub.Clients.All.SendAsync("ReceiveSTT", transcription);
    Console.WriteLine("Sending STT to clients: " + transcription);

    return Ok(new { text = transcription });
}


private string ExtractWhisperText(string output)
{
    if (string.IsNullOrWhiteSpace(output))
        return "";

    // Whisper.cpp prints the transcription as plain text lines.
    // We take the last non-empty line.
    var lines = output
        .Split('\n')
        .Select(l => l.Trim())
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToList();

    if (lines.Count == 0)
        return "";

    // The actual transcription is usually the last line
    return lines.Last();
}

}
