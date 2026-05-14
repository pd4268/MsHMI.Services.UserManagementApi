using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MsHMI.Common.Auth;

/// <summary>
/// Implementation of ICurrentUser that reads user information from JWT claims.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private IReadOnlyList<string>? _rights;
    private IReadOnlyList<string>? _groups;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string Username => User?.FindFirst(ClaimTypes.Name)?.Value 
        ?? User?.FindFirst("username")?.Value 
        ?? string.Empty;

    public string Station => User?.FindFirst("station")?.Value ?? string.Empty;

    public IReadOnlyList<string> Rights
    {
        get
        {
            if (_rights is null)
            {
                var claims = User?.FindAll("right");
                _rights = claims is not null
                    ? claims.Select(c => c.Value.ToUpperInvariant()).ToList().AsReadOnly()
                    : Array.Empty<string>();
            }
            return _rights;
        }
    }

    public IReadOnlyList<string> Groups
    {
        get
        {
            if (_groups is null)
            {
                var claims = User?.FindAll("group");
                _groups = claims is not null
                    ? claims.Select(c => c.Value).ToList().AsReadOnly()
                    : Array.Empty<string>();
            }
            return _groups;
        }
    }

    public bool HasRight(string right)
    {
        if (string.IsNullOrWhiteSpace(right))
            return false;

        var upperRight = right.ToUpperInvariant();
        
        // Direct match
        if (Rights.Contains(upperRight))
            return true;

        // If checking for Read:X, also check if user has Write:X
        if (upperRight.StartsWith("READ:", StringComparison.OrdinalIgnoreCase))
        {
            var screenName = upperRight[5..]; // Remove "READ:" prefix
            var writeRight = $"WRITE:{screenName}";
            if (Rights.Contains(writeRight))
                return true;
        }

        return false;
    }

    public bool CanWrite(string screenName)
    {
        return HasRight($"Write:{screenName}");
    }

    public bool CanRead(string screenName)
    {
        return HasRight($"Read:{screenName}");
    }
}
