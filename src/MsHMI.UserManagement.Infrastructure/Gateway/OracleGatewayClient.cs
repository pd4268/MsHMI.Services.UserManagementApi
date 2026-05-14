using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MsHMI.UserManagement.Infrastructure.Gateway;

/// <summary>
/// Configuration for the Oracle Gateway
/// </summary>
public class OracleGatewayOptions
{
    public string BaseUrl { get; set; } = "http://ms-oracle-gateway";
}

/// <summary>
/// Response from gateway query operations
/// </summary>
public class GatewayQueryResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("rows")]
    public List<Dictionary<string, JsonElement>>? Rows { get; set; }

    [JsonPropertyName("columns")]
    public List<string>? Columns { get; set; }

    [JsonPropertyName("rowsAffected")]
    public int RowsAffected { get; set; }
}

/// <summary>
/// Response for user existence check
/// </summary>
public class UserExistsResponse
{
    [JsonPropertyName("exists")]
    public bool Exists { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Response for create/update operations
/// </summary>
public class MutationResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("inserted")]
    public int Inserted { get; set; }

    [JsonPropertyName("rightsAdded")]
    public int RightsAdded { get; set; }
}

/// <summary>
/// HTTP client for Oracle 8i Gateway
/// </summary>
public class OracleGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OracleGatewayClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public OracleGatewayClient(HttpClient httpClient, ILogger<OracleGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Check if the gateway and Oracle are healthy
    /// </summary>
    public async Task<bool> HealthCheckAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/oracle/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway health check failed");
            return false;
        }
    }

    /// <summary>
    /// Check if a user exists
    /// </summary>
    public async Task<bool> UserExistsAsync(string username, CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<UserExistsResponse>(
            $"/api/userrights/exists/{Uri.EscapeDataString(username)}", ct);
        return response?.Exists ?? false;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    public async Task<List<string>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GatewayQueryResponse>(
            "/api/userrights/users", ct);
        
        if (response?.Success != true || response.Rows == null)
            return new List<string>();

        return response.Rows
            .Select(r => GetStringValue(r, "USERNAME"))
            .Where(u => u != null)
            .Select(u => u!)
            .ToList();
    }

    /// <summary>
    /// Get all groups
    /// </summary>
    public async Task<List<string>> GetAllGroupsAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GatewayQueryResponse>(
            "/api/userrights/groups", ct);
        
        if (response?.Success != true || response.Rows == null)
            return new List<string>();

        return response.Rows
            .Select(r => GetStringValue(r, "GROUPNAME"))
            .Where(g => g != null)
            .Select(g => g!.TrimStart('*'))
            .ToList();
    }

    /// <summary>
    /// Get rights for a user
    /// </summary>
    public async Task<List<string>> GetUserRightsAsync(string username, CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GatewayQueryResponse>(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}", ct);
        
        if (response?.Success != true || response.Rows == null)
            return new List<string>();

        return response.Rows
            .Select(r => GetStringValue(r, "USERRIGHT"))
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();
    }

    /// <summary>
    /// Get effective rights for a user (including group memberships)
    /// </summary>
    public async Task<List<string>> GetEffectiveRightsAsync(string username, CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GatewayQueryResponse>(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}/effective", ct);
        
        if (response?.Success != true || response.Rows == null)
            return new List<string>();

        return response.Rows
            .Select(r => GetStringValue(r, "USERRIGHT"))
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();
    }

    /// <summary>
    /// Get all available rights
    /// </summary>
    public async Task<List<string>> GetAvailableRightsAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetFromJsonAsync<GatewayQueryResponse>(
            "/api/userrights/available", ct);
        
        if (response?.Success != true || response.Rows == null)
            return new List<string>();

        return response.Rows
            .Select(r => GetStringValue(r, "USERRIGHT"))
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();
    }

    /// <summary>
    /// Create a new user with rights
    /// </summary>
    public async Task<bool> CreateUserAsync(string username, IEnumerable<string> rights, CancellationToken ct = default)
    {
        var payload = new { username, rights = rights.ToList() };
        var response = await _httpClient.PostAsJsonAsync("/api/userrights/user", payload, ct);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Replace all rights for a user
    /// </summary>
    public async Task<bool> ReplaceUserRightsAsync(string username, IEnumerable<string> rights, CancellationToken ct = default)
    {
        var payload = new { rights = rights.ToList() };
        var response = await _httpClient.PutAsJsonAsync(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}/rights", payload, ct);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Add a single right to a user
    /// </summary>
    public async Task<bool> AddRightToUserAsync(string username, string right, CancellationToken ct = default)
    {
        var payload = new { right };
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}/rights", payload, ct);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Remove a right from a user
    /// </summary>
    public async Task<bool> RemoveRightFromUserAsync(string username, string right, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}/rights/{Uri.EscapeDataString(right)}", ct);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    public async Task<bool> DeleteUserAsync(string username, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"/api/userrights/user/{Uri.EscapeDataString(username)}", ct);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Execute a raw SQL query
    /// </summary>
    public async Task<GatewayQueryResponse?> ExecuteQueryAsync(string sql, Dictionary<string, object>? parameters = null, CancellationToken ct = default)
    {
        var payload = new { sql, parameters };
        var response = await _httpClient.PostAsJsonAsync("/api/oracle/query", payload, ct);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<GatewayQueryResponse>(ct);
    }

    /// <summary>
    /// Execute a raw SQL statement (INSERT/UPDATE/DELETE)
    /// </summary>
    public async Task<GatewayQueryResponse?> ExecuteStatementAsync(string sql, Dictionary<string, object>? parameters = null, CancellationToken ct = default)
    {
        var payload = new { sql, parameters };
        var response = await _httpClient.PostAsJsonAsync("/api/oracle/execute", payload, ct);
        
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<GatewayQueryResponse>(ct);
    }

    private static string? GetStringValue(Dictionary<string, JsonElement> row, string key)
    {
        if (row.TryGetValue(key, out var element))
        {
            return element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString();
        }
        return null;
    }
}
