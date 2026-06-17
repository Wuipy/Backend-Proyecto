using Backend_Proyecto.Configuration;
using Backend_Proyecto.Models.Entities;
using Backend_Proyecto.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Data;

public static class DbSeeder
{
    private static readonly string[] TiposAveria =
    [
        "Fuga de agua",
        "Tuberia dañada",
        "Falta de agua",
        "Medidor dañado",
        "Otro",
        "Fuga",
        "Baja presion",
        "Ruptura de tuberia"
    ];

    public const string TipoVisitaCampo = "Visita de campo";
    public const string TipoTomaPresion = "Toma de presion";
    public const string TipoControlOperativo = "Control operativo";
    public const string TipoActividadGeneral = "Actividad general";

    public static readonly string[] TiposActividadFontanero =
    [
        TipoVisitaCampo,
        TipoTomaPresion,
        TipoControlOperativo,
        TipoActividadGeneral
    ];

    private static readonly string[] TiposSolicitud =
    [
        "Nueva conexion",
        "Disponibilidad de agua",
        "Suspension temporal",
        "Cancelacion del servicio",
        "Cambio de titular"
    ];

    public static async Task SeedAsync(AppDbContext context, AdminSettings adminSettings)
    {
        await MigrarEstadosAveriasAsync(context);

        if (!await context.SecuenciasContador.AnyAsync())
        {
            context.SecuenciasContador.AddRange(
                new SecuenciaContador { Prefijo = "AV", UltimoValor = 0 },
                new SecuenciaContador { Prefijo = "SOL", UltimoValor = 0 }
            );
        }

        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == adminSettings.Usuario))
        {
            context.Usuarios.Add(new Usuario
            {
                NombreUsuario = adminSettings.Usuario,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(adminSettings.Contrasena),
                Rol = Roles.Admin
            });
        }

        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "fontanero"))
        {
            context.Usuarios.Add(new Usuario
            {
                NombreUsuario = "fontanero",
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword("fontanero1234"),
                Rol = Roles.Fontanero
            });
        }

        if (!await context.Comunicados.AnyAsync())
        {
            context.Comunicados.AddRange(
                new Comunicado
                {
                    Fecha = "20 mayo 2026",
                    Titulo = "Aviso de mantenimiento programado",
                    Descripcion = "Revision preventiva de valvulas y lineas principales durante la manana.",
                    Estado = "Programado"
                },
                new Comunicado
                {
                    Fecha = "18 mayo 2026",
                    Titulo = "Interrupcion temporal del servicio",
                    Descripcion = "Corte temporal por reparacion de tuberia en el sector central.",
                    Estado = "Informativo"
                },
                new Comunicado
                {
                    Fecha = "15 mayo 2026",
                    Titulo = "Uso responsable del agua",
                    Descripcion = "Se recomienda evitar desperdicios y reportar fugas visibles oportunamente.",
                    Estado = "Recomendacion"
                }
            );
        }

        if (!await context.Proyectos.AnyAsync())
        {
            context.Proyectos.AddRange(
                new Proyecto
                {
                    Titulo = "Mejora de redes de distribucion",
                    Descripcion = "Sustitucion progresiva de tuberias antiguas en sectores prioritarios.",
                    Estado = "Planificado"
                },
                new Proyecto
                {
                    Titulo = "Nuevos tanques de almacenamiento",
                    Descripcion = "Evaluacion tecnica para aumentar la capacidad de reserva comunal.",
                    Estado = "En estudio"
                },
                new Proyecto
                {
                    Titulo = "Colocacion de hidrantes",
                    Descripcion = "Instalacion en puntos estrategicos para fortalecer la respuesta local.",
                    Estado = "Gestion"
                },
                new Proyecto
                {
                    Titulo = "Expansion y mejora del acueducto",
                    Descripcion = "Analisis de crecimiento de demanda y futuras ampliaciones del sistema.",
                    Estado = "Futuro"
                }
            );
        }

        await context.SaveChangesAsync();
    }

    public static bool EsTipoAveriaValido(string tipo) =>
        TiposAveria.Contains(tipo, StringComparer.Ordinal);

    public static bool EsTipoActividadFontaneroValido(string tipo) =>
        TiposActividadFontanero.Contains(tipo, StringComparer.Ordinal);

    public static bool EsEstadoActividadFontaneroValido(string estado) =>
        EstadosActividadFontanero.EsValido(estado);

    public static bool EsEstadoValidacionActividadValido(string estado) =>
        EstadosValidacionActividad.EsValido(estado);

    public static bool EsEstadoLecturaMedidorValido(string estado) =>
        EstadosLecturaMedidor.EsValido(estado);

    public static bool EsPrioridadAveriaValida(string prioridad) =>
        PrioridadesAveria.EsValida(prioridad);

    public static bool EsTipoSolicitudValido(string tipo) =>
        TiposSolicitud.Contains(tipo, StringComparer.Ordinal);

    private static async Task MigrarEstadosAveriasAsync(AppDbContext context)
    {
        var migraciones = new Dictionary<string, string>
        {
            ["Recibido"] = "Pendiente",
            ["En atencion"] = "En proceso",
            ["Atendido"] = "Finalizada"
        };

        foreach (var (anterior, nuevo) in migraciones)
        {
            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "Averias" SET "Estado" = {0} WHERE "Estado" = {1}""",
                nuevo,
                anterior);
        }
    }
}
