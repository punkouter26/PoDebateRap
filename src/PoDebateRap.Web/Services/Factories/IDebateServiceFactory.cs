using PoDebateRap.Web.Services.AI;
using PoDebateRap.Web.Services.Speech;
using PoDebateRap.Web.Services.Data;

namespace PoDebateRap.Web.Services.Factories
{
    /// <summary>
    /// Factory interface for creating debate-related services with proper scoping.
    /// Replaces Service Locator anti-pattern with Factory pattern.
    /// </summary>
    public interface IDebateServiceFactory
    {
        /// <summary>
        /// Creates a new scope containing scoped services for a debate turn.
        /// Caller is responsible for disposing the scope.
        /// </summary>
        /// <returns>A disposable service scope with AI, TTS, and data services</returns>
        IDebateServiceScope CreateScope();
    }

    /// <summary>
    /// Represents a scoped set of services needed for a debate turn.
    /// Implements IDisposable to properly clean up scoped resources.
    /// </summary>
    public interface IDebateServiceScope : IDisposable
    {
        IAzureOpenAIService AIService { get; }
        ITextToSpeechService TTSService { get; }
        IRapperRepository RapperRepository { get; }
    }
}
