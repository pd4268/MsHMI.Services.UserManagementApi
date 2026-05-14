namespace MsHMI.Common.Auth;

/// <summary>
/// Represents the currently authenticated user.
/// Inject this interface to access user information and check permissions.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The username of the authenticated user (e.g., "JSMITH").
    /// </summary>
    string Username { get; }

    /// <summary>
    /// The computer/station name where the user logged in from.
    /// </summary>
    string Station { get; }

    /// <summary>
    /// List of all rights assigned to this user (including inherited group rights).
    /// Rights follow the pattern: "Read:ScreenName" or "Write:ScreenName".
    /// </summary>
    IReadOnlyList<string> Rights { get; }

    /// <summary>
    /// Windows domain groups the user belongs to.
    /// Groups are prefixed with "*" in the database (e.g., "*OPERATORS").
    /// </summary>
    IReadOnlyList<string> Groups { get; }

    /// <summary>
    /// Check if the user has a specific right.
    /// Note: Having "Write:X" implicitly grants "Read:X".
    /// </summary>
    /// <param name="right">The right to check (e.g., "Write:ManageUsers").</param>
    /// <returns>True if the user has the right, false otherwise.</returns>
    bool HasRight(string right);

    /// <summary>
    /// Check if the user has write access to a specific screen.
    /// </summary>
    /// <param name="screenName">The screen name (e.g., "ManageUsers").</param>
    /// <returns>True if the user can write, false otherwise.</returns>
    bool CanWrite(string screenName);

    /// <summary>
    /// Check if the user has read access to a specific screen.
    /// </summary>
    /// <param name="screenName">The screen name (e.g., "ManageUsers").</param>
    /// <returns>True if the user can read, false otherwise.</returns>
    bool CanRead(string screenName);
}
