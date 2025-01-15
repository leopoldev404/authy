namespace Auth;

internal sealed class JwtSettings
{
    public const string CONFIGURATION_SECTION_NAME = "Jwt";
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }
    public required int ExpirationInMinutes { get; set; }
}
