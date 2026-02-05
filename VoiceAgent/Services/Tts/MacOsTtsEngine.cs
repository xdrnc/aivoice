using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace VoiceAgent.Services.Tts
{
    public class MacOsTtsEngine : ITtsEngine
    {
        public async Task<(byte[] Audio, string MimeType)> GenerateAudioAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (Array.Empty<byte>(), "audio/wav");

            // Temporary files
            var aiffFile = Path.GetTempFileName() + ".aiff";
            var wavFile = Path.GetTempFileName() + ".wav";

            try
            {
                //
                // 1. Generate AIFF using macOS "say"
                //
                var say = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "say",
                        Arguments = $"-o \"{aiffFile}\" \"{text}\"",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                say.Start();
                await say.WaitForExitAsync();

                if (!File.Exists(aiffFile))
                    return (Array.Empty<byte>(), "audio/wav");

                //
                // 2. Convert AIFF → WAV using afconvert
                //
                var convert = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "afconvert",
                        Arguments = $"\"{aiffFile}\" \"{wavFile}\" -d LEI16@44100 -f WAVE",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                convert.Start();
                await convert.WaitForExitAsync();

                if (!File.Exists(wavFile))
                    return (Array.Empty<byte>(), "audio/wav");

                //
                // 3. Read WAV bytes
                //
                var bytes = await File.ReadAllBytesAsync(wavFile);

                if (bytes.Length < 200) // sanity check
                    return (Array.Empty<byte>(), "audio/wav");

                return (bytes, "audio/wav");
            }
            finally
            {
                // Cleanup temp files
                if (File.Exists(aiffFile)) File.Delete(aiffFile);
                if (File.Exists(wavFile)) File.Delete(wavFile);
            }
        }
    }
}
