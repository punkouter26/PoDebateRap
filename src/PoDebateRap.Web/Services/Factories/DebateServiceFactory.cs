using Microsoft.Extensions.DependencyInjection;
using PoDebateRap.Web.Services.AI;
using PoDebateRap.Web.Services.Speech;
using PoDebateRap.Web.Services.Data;

namespace PoDebateRap.Web.Services.Factories
{
    /// <summary>
    /// Factory implementation for creating debate-related services.
    /// Uses IServiceProvider internally but encapsulates service location logic.
    /// </summary>
    public class DebateServiceFactory : IDebateServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DebateServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDebateServiceScope CreateScope()
        {
            return new DebateServiceScope(_serviceProvider.CreateScope());
        }
    }

    /// <summary>
    /// Internal implementation of IDebateServiceScope.
    /// Wraps IServiceScope and provides typed access to debate services.
    /// </summary>
    internal class DebateServiceScope : IDebateServiceScope
    {
        private readonly IServiceScope _scope;

        public DebateServiceScope(IServiceScope scope)
        {
            _scope = scope;
            AIService = _scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
            TTSService = _scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
            RapperRepository = _scope.ServiceProvider.GetRequiredService<IRapperRepository>();
        }

        public IAzureOpenAIService AIService { get; }
        public ITextToSpeechService TTSService { get; }
        public IRapperRepository RapperRepository { get; }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}
