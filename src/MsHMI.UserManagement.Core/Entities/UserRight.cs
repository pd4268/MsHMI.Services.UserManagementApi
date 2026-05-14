namespace MsHMI.UserManagement.Core.Entities;

/// <summary>
/// Represents a user-right assignment in the USERRIGHT table.
/// This is the legacy table structure - simple username + right pair.
/// </summary>
public class UserRight
{
    /// <summary>
    /// The username or group name.
    /// Groups are prefixed with "*" (e.g., "*OPERATORS").
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The right assigned to this user/group.
    /// Format: "Read:ScreenName" or "Write:ScreenName".
    /// </summary>
    public string Right { get; set; } = string.Empty;

    /// <summary>
    /// Returns true if this entry represents a group (prefixed with *).
    /// </summary>
    public bool IsGroup => Username.StartsWith('*');

    /// <summary>
    /// Gets the display name (without the * prefix for groups).
    /// </summary>
    public string DisplayName => IsGroup ? Username[1..] : Username;

    /// <summary>
    /// Creates a UserRight for a group with the proper prefix.
    /// </summary>
    public static UserRight ForGroup(string groupName, string right) => new()
    {
        Username = groupName.StartsWith('*') ? groupName : $"*{groupName}",
        Right = right
    };

    /// <summary>
    /// Creates a UserRight for a user (no prefix).
    /// </summary>
    public static UserRight ForUser(string username, string right) => new()
    {
        Username = username.TrimStart('*'),
        Right = right
    };
}
