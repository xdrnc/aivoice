public interface ITtsEngine
{
    Task<(byte[] Audio, string MimeType)> GenerateAudioAsync(string text);
}
