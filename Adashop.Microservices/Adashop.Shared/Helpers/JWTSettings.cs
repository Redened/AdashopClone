namespace Adashop.Shared.Helpers;

public class JWTSettings
{
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpirationMinutes { get; set; } = 60;
}