using MsHMI.UserManagement.Core.DTOs;

namespace MsHMI.UserManagement.Core.Interfaces;

/// <summary>
/// Service for managing users and their rights.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users with summary information.
    /// </summary>
    Task<IReadOnlyList<UserSummary>> GetAllUsersAsync(CancellationToken ct = default);

    /// <summary>
    /// Get detailed information about a specific user.
    /// </summary>
    Task<UserDetail?> GetUserAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Create a new user with optional initial rights.
    /// </summary>
    /// <param name="username">The username (will be uppercased).</param>
    /// <param name="rights">Initial rights. If null, grants "Read:MMI" as default.</param>
    /// <returns>True if created successfully, false if user already exists.</returns>
    Task<bool> CreateUserAsync(string username, IEnumerable<string>? rights = null, CancellationToken ct = default);

    /// <summary>
    /// Update a user's rights (replaces all existing rights).
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="rights">The complete list of rights to assign.</param>
    /// <returns>True if successful.</returns>
    Task<bool> UpdateUserRightsAsync(string username, IEnumerable<string> rights, CancellationToken ct = default);

    /// <summary>
    /// Rename a user.
    /// </summary>
    /// <returns>True if successful, false if new name already exists or user not found.</returns>
    Task<bool> RenameUserAsync(string oldUsername, string newUsername, CancellationToken ct = default);

    /// <summary>
    /// Delete a user and all their rights.
    /// </summary>
    Task<bool> DeleteUserAsync(string username, CancellationToken ct = default);
}

/// <summary>
/// Service for managing groups and their rights.
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Get all groups with summary information.
    /// </summary>
    Task<IReadOnlyList<GroupSummary>> GetAllGroupsAsync(CancellationToken ct = default);

    /// <summary>
    /// Get detailed information about a specific group.
    /// </summary>
    Task<GroupDetail?> GetGroupAsync(string groupName, CancellationToken ct = default);

    /// <summary>
    /// Create a new group with optional initial rights.
    /// </summary>
    Task<bool> CreateGroupAsync(string groupName, IEnumerable<string>? rights = null, CancellationToken ct = default);

    /// <summary>
    /// Update a group's rights.
    /// </summary>
    Task<bool> UpdateGroupRightsAsync(string groupName, IEnumerable<string> rights, CancellationToken ct = default);

    /// <summary>
    /// Rename a group.
    /// </summary>
    Task<bool> RenameGroupAsync(string oldName, string newName, CancellationToken ct = default);

    /// <summary>
    /// Delete a group.
    /// </summary>
    Task<bool> DeleteGroupAsync(string groupName, CancellationToken ct = default);
}

/// <summary>
/// Service for querying available rights.
/// </summary>
public interface IRightsService
{
    /// <summary>
    /// Get all distinct rights in the system with usage statistics.
    /// </summary>
    Task<IReadOnlyList<RightInfo>> GetAllRightsAsync(CancellationToken ct = default);
}
