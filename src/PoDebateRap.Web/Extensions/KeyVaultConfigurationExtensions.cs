using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace PoDebateRap.Web.Extensions;

/// <summary>
/// Custom Key Vault secret name manager that maps Key Vault secret names to .NET configuration paths.
/// Key Vault secrets use dashes as delimiters (e.g., "AzureOpenAI-ApiKey")
/// which are mapped to configuration paths with colons (e.g., "Azure:OpenAI:ApiKey").
/// </summary>
public class PoDebateRapKeyVaultSecretManager : KeyVaultSecretManager
{
    private static readonly Dictionary<string, string> SecretMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Azure OpenAI secrets
        ["AzureOpenAI-ApiKey"] = "Azure:OpenAI:ApiKey",
        ["AzureOpenAI-Endpoint"] = "Azure:OpenAI:Endpoint",
        ["AzureOpenAI-DeploymentName"] = "Azure:OpenAI:DeploymentName",
        
        // Azure Speech secrets
        ["AzureSpeech-SubscriptionKey"] = "Azure:Speech:SubscriptionKey",
        ["AzureSpeech-Region"] = "Azure:Speech:Region",
        
        // Application Insights
        ["ApplicationInsights-ConnectionString"] = "Azure:ApplicationInsights:ConnectionString",
        
        // Storage connection strings
        ["PoDebateRap-StorageConnection"] = "ConnectionStrings:tables",
        
        // News API (if needed)
        ["NewsApi-ApiKey"] = "NewsApi:ApiKey"
    };

    public override string GetKey(KeyVaultSecret secret)
    {
        // Try to find a mapping for this secret
        if (SecretMappings.TryGetValue(secret.Name, out var configKey))
        {
            return configKey;
        }
        
        // Default behavior: replace double dashes with colons (standard Azure convention)
        return secret.Name.Replace("--", ":");
    }

    public override bool Load(SecretProperties properties)
    {
        // Load all enabled secrets
        return properties.Enabled == true;
    }
}
