using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.Web.Services.Speech
{
    public interface ITextToSpeechService
    {
        /// <summary>
        /// Indicates whether the Text-to-Speech service is properly configured.
        /// </summary>
        bool IsConfigured { get; }

        Task<byte[]> GenerateSpeechAsync(string text, string voiceName, CancellationToken cancellationToken);
    }
}
