namespace PoDebateRap.ServerApi.Configuration
{
    /// <summary>
    /// Unified configuration options for all Azure services used in the application.
    /// Uses the Options Pattern for strongly-typed configuration access.
    /// </summary>
    public class AzureSettings
    {
        /// <summary>
        /// Configuration section name in appsettings.json.
        /// </summary>
        public const string SectionName = "Azure";

        /// <summary>
        /// Azure Storage connection string for Table Storage.
        /// </summary>
        public string StorageConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Azure Key Vault configuration.
        /// </summary>
        public KeyVaultSettings KeyVault { get; set; } = new();

        /// <summary>
        /// Azure OpenAI service configuration.
        /// </summary>
        public OpenAISettings OpenAI { get; set; } = new();

        /// <summary>
        /// Azure Speech service configuration.
        /// </summary>
        public SpeechSettings Speech { get; set; } = new();

        /// <summary>
        /// Application Insights configuration.
        /// </summary>
        public ApplicationInsightsSettings ApplicationInsights { get; set; } = new();
    }

    /// <summary>
    /// Azure Key Vault configuration options.
    /// </summary>
    public class KeyVaultSettings
    {
        /// <summary>
        /// The URI of the Azure Key Vault.
        /// </summary>
        public string VaultUri { get; set; } = string.Empty;
    }

    /// <summary>
    /// Azure OpenAI service configuration options.
    /// </summary>
    public class OpenAISettings
    {
        /// <summary>
        /// The Azure OpenAI service endpoint URL.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// The API key for authenticating with Azure OpenAI.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The name of the deployed model (e.g., "gpt-4o").
        /// </summary>
        public string DeploymentName { get; set; } = "gpt-4o";
    }

    /// <summary>
    /// Azure Speech service configuration options.
    /// </summary>
    public class SpeechSettings
    {
        /// <summary>
        /// The Azure region where the Speech service is deployed.
        /// </summary>
        public string Region { get; set; } = "eastus";

        /// <summary>
        /// The Speech service endpoint URL (optional, can be derived from region).
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// The subscription key for authenticating with Azure Speech.
        /// </summary>
        public string SubscriptionKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Application Insights configuration options.
    /// </summary>
    public class ApplicationInsightsSettings
    {
        /// <summary>
        /// The Application Insights connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }

    /// <summary>
    /// NewsAPI configuration options.
    /// </summary>
    public class NewsApiSettings
    {
        /// <summary>
        /// Configuration section name in appsettings.json.
        /// </summary>
        public const string SectionName = "NewsApi";

        /// <summary>
        /// The API key for NewsAPI.org.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
