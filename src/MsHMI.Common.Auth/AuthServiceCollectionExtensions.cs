using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MsHMI.Common.Auth;

/// <summary>
/// Extension methods for registering MsHMI authentication services.
/// </summary>
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Adds MsHMI authentication services including JWT bearer authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing JWT settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMsHMIAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind JWT settings
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Register ICurrentUser
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Configure JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.ValidateIssuer,
                ValidateAudience = jwtSettings.ValidateAudience,
                ValidateLifetime = jwtSettings.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

        // Add authorization with dynamic policies for rights
        services.AddAuthorization(options =>
        {
            // Default policy requires authentication
            options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // Register dynamic policy provider for [RequireRight("...")] attributes
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, RightPolicyProvider>();
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, RightRequirementHandler>();

        return services;
    }

    /// <summary>
    /// Adds a policy that requires a specific right.
    /// Use this during service configuration to register custom right-based policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="policyName">The policy name (e.g., "RequireWriteManageUsers").</param>
    /// <param name="requiredRight">The required right (e.g., "Write:ManageUsers").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRightPolicy(
        this IServiceCollection services,
        string policyName,
        string requiredRight)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy =>
            {
                policy.RequireAssertion(context =>
                {
                    var rightClaim = context.User.FindAll("right")
                        .Any(c => c.Value.Equals(requiredRight, StringComparison.OrdinalIgnoreCase));
                    
                    // Also check for Write implying Read
                    if (!rightClaim && requiredRight.StartsWith("Read:", StringComparison.OrdinalIgnoreCase))
                    {
                        var screenName = requiredRight[5..];
                        rightClaim = context.User.FindAll("right")
                            .Any(c => c.Value.Equals($"Write:{screenName}", StringComparison.OrdinalIgnoreCase));
                    }
                    
                    return rightClaim;
                });
            });

        return services;
    }
}
