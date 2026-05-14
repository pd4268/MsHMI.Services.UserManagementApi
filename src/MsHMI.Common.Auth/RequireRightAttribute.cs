using Microsoft.AspNetCore.Authorization;

namespace MsHMI.Common.Auth;

/// <summary>
/// Authorization attribute that requires the user to have a specific right.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// [RequireRight("Write:ManageUsers")]
/// [HttpPost]
/// public async Task&lt;IActionResult&gt; CreateUser(...) { }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRightAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "RequireRight:";

    /// <summary>
    /// Creates a new RequireRightAttribute that requires the specified right.
    /// </summary>
    /// <param name="right">The required right (e.g., "Write:ManageUsers").</param>
    public RequireRightAttribute(string right) : base(policy: $"{PolicyPrefix}{right}")
    {
        Right = right;
    }

    /// <summary>
    /// The required right.
    /// </summary>
    public string Right { get; }
}
