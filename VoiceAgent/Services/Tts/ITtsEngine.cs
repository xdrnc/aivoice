public interface ITtsEngine
{
    Task<byte[]> GenerateAudioAsync(string text);
}
