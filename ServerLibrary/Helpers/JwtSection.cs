namespace ServerLibrary.Helpers;

public class JwtSection
{
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public int AccessTokenExpiryMinutes { get; set; }
}