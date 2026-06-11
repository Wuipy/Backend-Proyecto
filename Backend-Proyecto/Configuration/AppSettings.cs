namespace Backend_Proyecto.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SIGASJ";
    public string Audience { get; set; } = "SIGASJ-Frontend";
    public int ExpirationHours { get; set; } = 8;
}

public class AdminSettings
{
    public const string SectionName = "Admin";

    public string Usuario { get; set; } = "admin";
    public string Contrasena { get; set; } = "admin1234";
}

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173"];
}
