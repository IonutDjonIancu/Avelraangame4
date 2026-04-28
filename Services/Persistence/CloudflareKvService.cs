using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Services.Persistence;

public interface ICloudflareKvService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SaveAsync<T>(string key, T value);
}

public class CloudflareKvService : ICloudflareKvService
{
    private readonly HttpClient _httpClient;
    private readonly string _accountId;
    private readonly string _namespaceId;
    private readonly string _baseUrl;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CloudflareKvService(HttpClient httpClient, IConfiguration configuration)
    {
        _accountId = configuration["Cloudflare:AccountId"] ?? throw new InvalidOperationException("Cloudflare:AccountId missing.");
        _namespaceId = configuration["Cloudflare:NamespaceId"] ?? throw new InvalidOperationException("Cloudflare:NamespaceId missing.");
        var apiToken = configuration["Cloudflare:ApiToken"] ?? throw new InvalidOperationException("Cloudflare:ApiToken missing.");

        _baseUrl = $"https://api.cloudflare.com/client/v4/accounts/{_accountId}/storage/kv/namespaces/{_namespaceId}/values";

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/{key}");

        if (!response.IsSuccessStatusCode)
            return default;

        var body = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(body, _jsonOptions);
    }

    public async Task<bool> SaveAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"{_baseUrl}/{key}", content);
        return response.IsSuccessStatusCode;
    }
}
