using Npgsql;

namespace Backend_Proyecto.Configuration;

public static class DatabaseConfiguration
{
    private const string PlaceholderPassword = "[YOUR-PASSWORD]";

    public static string RequireConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                """
                No hay cadena de conexion a PostgreSQL configurada.

                Configure una de estas opciones:
                  1) appsettings.Development.json (solo local)
                  2) Variable de entorno ConnectionStrings__DefaultConnection
                     (Railway, MonsterASP Control panel -> Scripting -> Environment Variables, etc.)

                Formato Supabase (pooler, puerto 5432):
                Host=aws-0-REGION.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.SU_PROJECT_REF;Password=SU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true

                Importante: con el pooler el usuario NO es "postgres" solo, debe ser "postgres.SU_PROJECT_REF".
                """);
        }

        if (connectionString.Contains(PlaceholderPassword, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                """
                La cadena de conexion sigue usando el placeholder [YOUR-PASSWORD].

                Defina ConnectionStrings__DefaultConnection con la contraseña real de Supabase
                (Project Settings -> Database -> Database password).
                """);
        }

        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "La cadena de conexion DefaultConnection no es valida para Npgsql.", ex);
        }

        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            throw new InvalidOperationException(
                "La cadena de conexion no incluye Password. Revise ConnectionStrings__DefaultConnection.");
        }

        if (IsSupabasePooler(builder) && builder.Username == "postgres")
        {
            throw new InvalidOperationException(
                """
                Supabase pooler detectado con Username=postgres (incorrecto).

                Use el usuario del panel de Supabase, por ejemplo:
                  Username=postgres.wgedltybvsgmlbbnxnkr

                Copie la cadena completa desde:
                  Supabase -> Project Settings -> Database -> Connection string -> URI / Session pooler
                """);
        }

        return connectionString;
    }

    public static string DescribeConnection(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"Host={builder.Host};Port={builder.Port};Database={builder.Database};Username={builder.Username}";
    }

    private static bool IsSupabasePooler(NpgsqlConnectionStringBuilder builder) =>
        builder.Host?.Contains("pooler.supabase.com", StringComparison.OrdinalIgnoreCase) == true;
}
