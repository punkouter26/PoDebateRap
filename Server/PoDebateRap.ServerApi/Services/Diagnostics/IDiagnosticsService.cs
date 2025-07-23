using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Diagnostics
{
    public interface IDiagnosticsService
    {
        Task<string> CheckApiHealthAsync();
        Task<string> CheckDataConnectionAsync();
        Task<string> CheckInternetConnectionAsync();
        Task<string> CheckAuthenticationServiceAsync();
        Task<string> CheckAzureOpenAIServiceAsync();
        Task<string> CheckTextToSpeechServiceAsync();
    }
}
