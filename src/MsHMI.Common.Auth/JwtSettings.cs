namespace MsHMI.Common.Auth;

/// <summary>
/// Configuration settings for JWT authentication.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key used for signing JWT tokens.
    /// Should be at least 256 bits (32 characters) for HS256.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The issuer of the JWT token.
    /// </summary>
    public string Issuer { get; set; } = "MsHMI";

    /// <summary>
    /// The intended audience for the JWT token.
    /// </summary>
    public string Audience { get; set; } = "MsHMI.Applications";

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 480; // 8 hours

    /// <summary>
    /// Whether to validate the issuer.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate the audience.
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate the token lifetime.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
}
