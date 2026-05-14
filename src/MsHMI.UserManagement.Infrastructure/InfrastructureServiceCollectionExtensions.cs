using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsHMI.UserManagement.Core.Interfaces;
using MsHMI.UserManagement.Core.Services;
using MsHMI.UserManagement.Infrastructure.Gateway;
using MsHMI.UserManagement.Infrastructure.Repositories;

namespace MsHMI.UserManagement.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services using the Oracle Gateway for Oracle 8i compatibility.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing gateway settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Oracle Gateway options
        services.Configure<OracleGatewayOptions>(configuration.GetSection("OracleGateway"));
        
        // Register HTTP client for Oracle Gateway
        services.AddHttpClient<OracleGatewayClient>((sp, client) =>
        {
            var gatewayUrl = configuration.GetValue<string>("OracleGateway:BaseUrl") 
                ?? "http://oracle-gateway:8081";
            client.BaseAddress = new Uri(gatewayUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register gateway-based repository
        services.AddScoped<IUserRightRepository, GatewayUserRightRepository>();
        
        // Register a no-op unit of work since gateway handles transactions
        services.AddScoped<IUnitOfWork, GatewayUnitOfWork>();

        // Register domain services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IRightsService, RightsService>();

        return services;
    }
}
