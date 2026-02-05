public class MacOsTtsEngine : ITtsEngine
{
    public async Task<byte[]> GenerateAudioAsync(string text)
    {
        var tempFile = Path.GetTempFileName() + ".aiff";

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "say",
                Arguments = $"-o {tempFile} \"{text}\"",
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        var bytes = await File.ReadAllBytesAsync(tempFile);
        File.Delete(tempFile);

        return bytes;
    }
}
