using MsHMI.UserManagement.Core.Entities;

namespace MsHMI.UserManagement.Core.Interfaces;

/// <summary>
/// Repository for accessing user rights data.
/// </summary>
public interface IUserRightRepository
{
    /// <summary>
    /// Get all distinct usernames (excluding groups).
    /// </summary>
    Task<IReadOnlyList<string>> GetAllUsernamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all distinct group names (without the * prefix).
    /// </summary>
    Task<IReadOnlyList<string>> GetAllGroupNamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get all rights assigned to a specific user (not including group rights).
    /// </summary>
    /// <param name="username">The username (without * prefix).</param>
    Task<IReadOnlyList<string>> GetRightsForUserAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Get all rights assigned to a specific group.
    /// </summary>
    /// <param name="groupName">The group name (without * prefix).</param>
    Task<IReadOnlyList<string>> GetRightsForGroupAsync(string groupName, CancellationToken ct = default);

    /// <summary>
    /// Get all distinct rights that exist in the system.
    /// </summary>
    Task<IReadOnlyList<string>> GetAllDistinctRightsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get rights NOT assigned to a user.
    /// </summary>
    Task<IReadOnlyList<string>> GetAvailableRightsForUserAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Get rights NOT assigned to a group.
    /// </summary>
    Task<IReadOnlyList<string>> GetAvailableRightsForGroupAsync(string groupName, CancellationToken ct = default);

    /// <summary>
    /// Delete all rights for a user.
    /// </summary>
    /// <param name="username">The username (with or without * prefix for groups).</param>
    Task DeleteAllRightsAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Add a right to a user or group.
    /// </summary>
    /// <param name="username">The username (with * prefix for groups).</param>
    /// <param name="right">The right to add.</param>
    Task AddRightAsync(string username, string right, CancellationToken ct = default);

    /// <summary>
    /// Add multiple rights to a user or group.
    /// </summary>
    Task AddRightsAsync(string username, IEnumerable<string> rights, CancellationToken ct = default);

    /// <summary>
    /// Rename a user (updates all their right entries).
    /// </summary>
    Task RenameAsync(string oldUsername, string newUsername, CancellationToken ct = default);

    /// <summary>
    /// Check if a username already exists.
    /// </summary>
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Get statistics about a right (how many users/groups have it).
    /// </summary>
    Task<(int UserCount, int GroupCount)> GetRightStatsAsync(string right, CancellationToken ct = default);
}
