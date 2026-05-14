using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MsHMI.Common.Auth;
using MsHMI.UserManagement.Core.DTOs;
using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Api.Controllers;

/// <summary>
/// Authentication controller for generating JWT tokens.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRightRepository _userRightRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRightRepository userRightRepository,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _userRightRepository = userRightRepository;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Login and receive a JWT token.
    /// Currently supports Windows authentication passthrough.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest("Username is required");

        var username = request.Username.ToUpperInvariant();

        // Check if user exists (has any rights)
        if (!await _userRightRepository.ExistsAsync(username, ct))
        {
            _logger.LogWarning("Login attempt for unknown user: {Username}", username);
            return Unauthorized("User not found");
        }

        // Get user's rights (for token claims)
        var rights = await _userRightRepository.GetRightsForUserAsync(username, ct);

        // TODO: Add group rights lookup based on Windows domain groups
        // For now, just include direct user rights

        // Generate JWT token
        var token = GenerateJwtToken(username, rights);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        _logger.LogInformation("User {Username} logged in successfully", username);

        return Ok(new LoginResponse(
            token,
            expiresAt,
            new UserInfo(username, rights)
        ));
    }

    /// <summary>
    /// Get current user info from token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser([FromServices] ICurrentUser currentUser)
    {
        return Ok(new UserInfo(currentUser.Username, currentUser.Rights.ToList()));
    }

    /// <summary>
    /// Refresh the current token.
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Refresh([FromServices] ICurrentUser currentUser, CancellationToken ct)
    {
        var rights = await _userRightRepository.GetRightsForUserAsync(currentUser.Username, ct);
        
        var token = GenerateJwtToken(currentUser.Username, rights);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return Ok(new LoginResponse(
            token,
            expiresAt,
            new UserInfo(currentUser.Username, rights)
        ));
    }

    private string GenerateJwtToken(string username, IEnumerable<string> rights)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new("username", username),
            new("station", Environment.MachineName)
        };

        // Add each right as a claim
        foreach (var right in rights)
        {
            claims.Add(new Claim("right", right));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
