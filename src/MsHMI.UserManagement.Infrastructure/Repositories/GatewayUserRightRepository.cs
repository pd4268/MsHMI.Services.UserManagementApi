using MsHMI.UserManagement.Core.Interfaces;
using MsHMI.UserManagement.Infrastructure.Gateway;

namespace MsHMI.UserManagement.Infrastructure.Repositories;

/// <summary>
/// Repository for user rights using the Oracle Gateway (for Oracle 8i compatibility)
/// </summary>
public class GatewayUserRightRepository : IUserRightRepository
{
    private readonly OracleGatewayClient _gateway;
    private const string GroupPrefix = "*";

    public GatewayUserRightRepository(OracleGatewayClient gateway)
    {
        _gateway = gateway;
    }

    public async Task<IReadOnlyList<string>> GetAllUsernamesAsync(CancellationToken ct = default)
    {
        var users = await _gateway.GetAllUsersAsync(ct);
        return users.Where(u => u != "SYSTEM").OrderBy(u => u).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAllGroupNamesAsync(CancellationToken ct = default)
    {
        var groups = await _gateway.GetAllGroupsAsync(ct);
        return groups.OrderBy(g => g).ToList();
    }

    public async Task<IReadOnlyList<string>> GetRightsForUserAsync(string username, CancellationToken ct = default)
    {
        var rights = await _gateway.GetUserRightsAsync(username.ToUpperInvariant(), ct);
        return rights.OrderBy(r => r).ToList();
    }

    public async Task<IReadOnlyList<string>> GetRightsForGroupAsync(string groupName, CancellationToken ct = default)
    {
        // Groups are stored with * prefix
        var dbName = $"{GroupPrefix}{groupName.ToUpperInvariant()}";
        var rights = await _gateway.GetUserRightsAsync(dbName, ct);
        return rights.OrderBy(r => r).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAllDistinctRightsAsync(CancellationToken ct = default)
    {
        var rights = await _gateway.GetAvailableRightsAsync(ct);
        return rights.Distinct().OrderBy(r => r).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAvailableRightsForUserAsync(string username, CancellationToken ct = default)
    {
        var userRights = await GetRightsForUserAsync(username, ct);
        var allRights = await GetAllDistinctRightsAsync(ct);

        return allRights.Except(userRights, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAvailableRightsForGroupAsync(string groupName, CancellationToken ct = default)
    {
        var groupRights = await GetRightsForGroupAsync(groupName, ct);
        var allRights = await GetAllDistinctRightsAsync(ct);

        return allRights.Except(groupRights, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task DeleteAllRightsAsync(string username, CancellationToken ct = default)
    {
        await _gateway.DeleteUserAsync(username.ToUpperInvariant(), ct);
    }

    public async Task AddRightAsync(string username, string right, CancellationToken ct = default)
    {
        await _gateway.AddRightToUserAsync(username.ToUpperInvariant(), right.ToUpperInvariant(), ct);
    }

    public async Task AddRightsAsync(string username, IEnumerable<string> rights, CancellationToken ct = default)
    {
        var normalizedUsername = username.ToUpperInvariant();
        var rightsList = rights.Select(r => r.ToUpperInvariant()).ToList();
        
        // Get existing rights and add new ones
        var existingRights = await GetRightsForUserAsync(normalizedUsername, ct);
        var allRights = existingRights.Concat(rightsList).Distinct().ToList();
        
        await _gateway.ReplaceUserRightsAsync(normalizedUsername, allRights, ct);
    }

    public async Task RenameAsync(string oldUsername, string newUsername, CancellationToken ct = default)
    {
        var normalizedOld = oldUsername.ToUpperInvariant();
        var normalizedNew = newUsername.ToUpperInvariant();

        // Get existing rights, delete old user, create new user with same rights
        var rights = await GetRightsForUserAsync(normalizedOld, ct);
        await _gateway.DeleteUserAsync(normalizedOld, ct);
        await _gateway.CreateUserAsync(normalizedNew, rights, ct);
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _gateway.UserExistsAsync(username.ToUpperInvariant(), ct);
    }

    public async Task<(int UserCount, int GroupCount)> GetRightStatsAsync(string right, CancellationToken ct = default)
    {
        // Query via raw SQL through the gateway
        var result = await _gateway.ExecuteQueryAsync(
            "SELECT USERNAME FROM ML2DBA.USERRIGHT WHERE UPPER(USERRIGHT) = :right",
            new Dictionary<string, object> { { "right", right.ToUpperInvariant() } },
            ct);

        if (result?.Success != true || result.Rows == null)
            return (0, 0);

        var usernames = result.Rows
            .Select(r => r.TryGetValue("USERNAME", out var v) ? v.GetString() : null)
            .Where(u => u != null)
            .Distinct()
            .ToList();

        var userCount = usernames.Count(u => !u!.StartsWith(GroupPrefix));
        var groupCount = usernames.Count(u => u!.StartsWith(GroupPrefix));

        return (userCount, groupCount);
    }
}
