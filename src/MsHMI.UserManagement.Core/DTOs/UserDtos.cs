namespace MsHMI.UserManagement.Core.DTOs;

/// <summary>
/// Summary information about a user.
/// </summary>
public record UserSummary(
    string Username,
    int RightCount
);

/// <summary>
/// Detailed information about a user including their rights.
/// </summary>
public record UserDetail(
    string Username,
    IReadOnlyList<string> Rights,
    IReadOnlyList<string> AvailableRights
);

/// <summary>
/// Summary information about a group.
/// </summary>
public record GroupSummary(
    string Name,
    int RightCount
);

/// <summary>
/// Detailed information about a group including its rights.
/// </summary>
public record GroupDetail(
    string Name,
    IReadOnlyList<string> Rights,
    IReadOnlyList<string> AvailableRights
);

/// <summary>
/// Information about a security right.
/// </summary>
public record RightInfo(
    string Name,
    int UserCount,
    int GroupCount
);

/// <summary>
/// Request to create a new user.
/// </summary>
public record CreateUserRequest(
    string Username,
    IReadOnlyList<string>? Rights = null
);

/// <summary>
/// Request to update a user's rights.
/// </summary>
public record UpdateRightsRequest(
    IReadOnlyList<string> Rights
);

/// <summary>
/// Request to rename a user or group.
/// </summary>
public record RenameRequest(
    string NewName
);

/// <summary>
/// Login request for authentication.
/// </summary>
public record LoginRequest(
    string Username,
    string? Password = null  // Optional - may use Windows auth
);

/// <summary>
/// Login response with JWT token.
/// </summary>
public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    UserInfo User
);

/// <summary>
/// Basic user info returned with login.
/// </summary>
public record UserInfo(
    string Username,
    IReadOnlyList<string> Rights
);
