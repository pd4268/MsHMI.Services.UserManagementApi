using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MsHMI.Common.Auth;

/// <summary>
/// Dynamic policy provider that creates policies for RequireRight attributes.
/// This allows [RequireRight("Write:ManageUsers")] to work without pre-registering each policy.
/// </summary>
public class RightPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public RightPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(RequireRightAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var requiredRight = policyName[RequireRightAttribute.PolicyPrefix.Length..];
            
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new RightRequirement(requiredRight))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }
}

/// <summary>
/// Authorization requirement for a specific right.
/// </summary>
public class RightRequirement : IAuthorizationRequirement
{
    public string RequiredRight { get; }

    public RightRequirement(string requiredRight)
    {
        RequiredRight = requiredRight;
    }
}

/// <summary>
/// Handler for RightRequirement that checks if the user has the required right.
/// </summary>
public class RightRequirementHandler : AuthorizationHandler<RightRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RightRequirement requirement)
    {
        var requiredRight = requirement.RequiredRight.ToUpperInvariant();

        // Check for direct match
        var hasRight = context.User.FindAll("right")
            .Any(c => c.Value.Equals(requiredRight, StringComparison.OrdinalIgnoreCase));

        // If checking for Read:X, also accept Write:X
        if (!hasRight && requiredRight.StartsWith("READ:", StringComparison.OrdinalIgnoreCase))
        {
            var screenName = requiredRight[5..];
            hasRight = context.User.FindAll("right")
                .Any(c => c.Value.Equals($"WRITE:{screenName}", StringComparison.OrdinalIgnoreCase));
        }

        if (hasRight)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
