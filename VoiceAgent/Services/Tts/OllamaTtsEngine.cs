public class OllamaTtsEngine : ITtsEngine
{
    private readonly HttpClient _client = new HttpClient();

    public async Task<byte[]> GenerateAudioAsync(string text)
    {
        var body = new
        {
            model = "gpt-4o-mini-tts",
            input = text,
            format = "wav"
        };

        var response = await _client.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            body
        );

        return await response.Content.ReadAsByteArrayAsync();
    }
}
