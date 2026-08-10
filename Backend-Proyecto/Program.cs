using Backend_Proyecto.Configuration;
using Backend_Proyecto.Data;
using Backend_Proyecto.Middleware;
using Backend_Proyecto.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection(AdminSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("La configuracion JWT es obligatoria.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddOpenApi();

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
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminSettings = builder.Configuration.GetSection(AdminSettings.SectionName).Get<AdminSettings>()
        ?? new AdminSettings();

    try
    {
        await context.Database.MigrateAsync();
        await DatabaseSchemaEnsurer.EnsureLecturasMedidorColumnsAsync(context);
        await DbSeeder.SeedAsync(context, adminSettings);
    }
    catch (PostgresException ex) when (ex.SqlState == "28P01")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("ERROR: Autenticacion fallida con Supabase (password incorrecto).");
        Console.WriteLine("Configure la contraseña real en una de estas opciones:");
        Console.WriteLine("  1) Copie appsettings.Development.example.json -> appsettings.Development.json");
        Console.WriteLine("  2) Variable de entorno ConnectionStrings__DefaultConnection");
        Console.WriteLine("  3) Ejecute el SQL de seed en Supabase: Migrations/sql/setup_completo_lecturas_supabase.sql");
        Console.ResetColor();
        throw;
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
