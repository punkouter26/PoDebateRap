using System.Net.Http.Json;

namespace PoDebateRap.ServerApi.Extensions;

/// <summary>
/// Extension methods for HTTP API client operations.
/// Eliminates duplicate GetInitial API call patterns.
/// </summary>
public static class ApiClientExtensions
{
    /// <summary>
    /// Gets data from an API endpoint with error handling and optional default value.
    /// </summary>
    public static async Task<T?> GetInitialAsync<T>(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken cancellationToken = default,
        T? defaultValue = default) where T : class
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(requestUri, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return defaultValue;
        }
        catch (TaskCanceledException)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets a list from an API endpoint, returning empty list on error.
    /// </summary>
    public static async Task<List<T>> GetListAsync<T>(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<List<T>>(requestUri, cancellationToken) ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
        catch (TaskCanceledException)
        {
            return [];
        }
    }
}
