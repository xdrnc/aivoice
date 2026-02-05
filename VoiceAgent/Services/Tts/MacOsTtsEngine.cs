// Services/Tts/MacOsTtsEngine.cs
public class MacOsTtsEngine : ITtsEngine
{
    public async Task<(byte[] Audio, string MimeType)> GenerateAudioAsync(string text)
    {
        // Guard: ignore empty/whitespace text
        if (string.IsNullOrWhiteSpace(text))
            return (Array.Empty<byte>(), "audio/aiff");

        var tempFile = Path.GetTempFileName() + ".aiff";

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "say",
                Arguments = $"-o \"{tempFile}\" \"{text}\"",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (!File.Exists(tempFile))
            return (Array.Empty<byte>(), "audio/aiff");

        var bytes = await File.ReadAllBytesAsync(tempFile);
        File.Delete(tempFile);

        // Guard: avoid sending tiny/invalid audio
        if (bytes.Length < 100)
            return (Array.Empty<byte>(), "audio/aiff");

        Console.WriteLine($"Generated file size: {bytes.Length}");
        File.WriteAllBytes("debug_output.aiff", bytes);

        return (bytes, "audio/aiff");
    }
}
