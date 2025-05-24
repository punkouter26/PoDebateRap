using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Speech
{
    public interface ITextToSpeechService
    {
        Task<byte[]> GenerateSpeechAsync(string text, string voiceName);
    }
}
