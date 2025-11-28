using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace PoDebateRap.ServerApi.Configuration;

/// <summary>
/// Custom Key Vault secret manager that maps secret names (with dashes) 
/// to configuration keys (with colons/hierarchy).
/// 
/// Secret naming convention in Key Vault:
/// - OpenAI-Endpoint       -> Azure:OpenAI:Endpoint
/// - OpenAI-ApiKey         -> Azure:OpenAI:ApiKey
/// - OpenAI-DeploymentName -> Azure:OpenAI:DeploymentName
/// - Speech-Region         -> Azure:Speech:Region
/// - Speech-Endpoint       -> Azure:Speech:Endpoint
/// - Speech-SubscriptionKey -> Azure:Speech:SubscriptionKey
/// - NewsApi-ApiKey        -> NewsApi:ApiKey
/// - Storage-ConnectionString -> Azure:StorageConnectionString
/// </summary>
public class PoDebateRapSecretManager : KeyVaultSecretManager
{
    private static readonly Dictionary<string, string> SecretMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        // OpenAI secrets
        ["OpenAI-Endpoint"] = "Azure:OpenAI:Endpoint",
        ["OpenAI-ApiKey"] = "Azure:OpenAI:ApiKey",
        ["OpenAI-DeploymentName"] = "Azure:OpenAI:DeploymentName",
        
        // Speech secrets
        ["Speech-Region"] = "Azure:Speech:Region",
        ["Speech-Endpoint"] = "Azure:Speech:Endpoint",
        ["Speech-SubscriptionKey"] = "Azure:Speech:SubscriptionKey",
        
        // NewsAPI secrets
        ["NewsApi-ApiKey"] = "NewsApi:ApiKey",
        
        // Storage secrets
        ["Storage-ConnectionString"] = "Azure:StorageConnectionString"
    };

    public override string GetKey(KeyVaultSecret secret)
    {
        // If we have a mapping, use it; otherwise, replace dashes with colons
        if (SecretMappings.TryGetValue(secret.Name, out var configKey))
        {
            return configKey;
        }
        
        // Default behavior: replace dashes with colons for nested config
        return secret.Name.Replace("-", ":", StringComparison.OrdinalIgnoreCase);
    }

    public override bool Load(SecretProperties properties)
    {
        // Load all secrets that are enabled
        return properties.Enabled == true;
    }
}
