using Backend_Proyecto.Configuration;
using Backend_Proyecto.Data;
using Backend_Proyecto.Middleware;
using Backend_Proyecto.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

var connectionString = DatabaseConfiguration.RequireConnectionString(builder.Configuration);
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection(AdminSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("La configuracion JWT es obligatoria.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
builder.Services.AddScoped<IAveriaService, AveriaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IConsultaSeguimientoService, ConsultaSeguimientoService>();
builder.Services.AddScoped<IActividadFontaneroService, ActividadFontaneroService>();
builder.Services.AddScoped<ILecturaMedidorService, LecturaMedidorService>();
builder.Services.AddScoped<IContenidoService, ContenidoService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIGASJ API",
        Version = "v1",
        Description = "API REST — ASADA San Juan de Santa Cruz"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Token JWT. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var corsOrigins = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()?.AllowedOrigins
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminSettings = builder.Configuration.GetSection(AdminSettings.SectionName).Get<AdminSettings>()
        ?? new AdminSettings();

    logger.LogInformation(
        "Conectando a PostgreSQL: {Connection}",
        DatabaseConfiguration.DescribeConnection(connectionString));

    try
    {
        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(context, adminSettings);
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.InvalidPassword)
    {
        throw new InvalidOperationException(
            $"""
             Autenticacion fallida en PostgreSQL para el usuario "{ex.MessageText.Split('"').ElementAtOrDefault(1) ?? "desconocido"}".

             Revise ConnectionStrings__DefaultConnection:
               1) Password correcto (Supabase -> Database -> Database password)
               2) Username=postgres.SU_PROJECT_REF (no solo "postgres" con pooler)
               3) Si la password tiene caracteres especiales, codifiquela en URI o escapela en la cadena

             Referencia SQL opcional: Migrations/sql/setup_completo_lecturas_supabase.sql
             """,
            ex);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo migrar o sembrar la base de datos al iniciar. La API arrancara igual; revise logs en MonsterASP.");
    }
}

app.UseForwardedHeaders();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGASJ API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
