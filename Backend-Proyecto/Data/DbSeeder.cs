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

        await SeedLecturasMedidorAsync(context, adminSettings);
    }

    private static async Task SeedLecturasMedidorAsync(AppDbContext context, AdminSettings adminSettings)
    {
        if (await context.LecturasMedidor.AnyAsync())
        {
            return;
        }

        var fontanero = await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == "fontanero");
        var admin = await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == adminSettings.Usuario);

        if (fontanero is null)
        {
            return;
        }

        var hoy = DateTime.UtcNow.Date;
        var mesAnterior = hoy.AddMonths(-1);

        context.LecturasMedidor.AddRange(
            new LecturaMedidor
            {
                NombreAbonado = "Maria Fernandez Solano",
                NumeroAbonado = "A-1024",
                NumeroMedidor = "M-45821",
                CedulaAbonado = "1-0234-0567",
                Ubicacion = "Palmares Centro, 150 m norte de la iglesia",
                LecturaAnterior = 1245.50m,
                LecturaActual = 1268.30m,
                Consumo = 22.80m,
                ConsumoMesAnterior = 21.50m,
                ConsumoAlto = false,
                FechaLectura = mesAnterior,
                HoraLectura = "08:30",
                Observaciones = "Lectura normal. Medidor en buen estado.",
                Estado = "Validada",
                EstadoMedidor = "Bueno",
                MotivoVisita = "Lectura mensual",
                FontaneroId = fontanero.Id,
                RevisadaPorAdminId = admin?.Id,
                FechaRegistro = mesAnterior.AddHours(8)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Carlos Mora Jimenez",
                NumeroAbonado = "A-1087",
                NumeroMedidor = "M-45822",
                CedulaAbonado = "2-0456-0789",
                Ubicacion = "Barrio El Carmen, contiguo al pulpería La Unión",
                LecturaAnterior = 890.00m,
                LecturaActual = 942.50m,
                Consumo = 52.50m,
                ConsumoMesAnterior = 24.00m,
                ConsumoAlto = true,
                FechaLectura = mesAnterior.AddDays(2),
                HoraLectura = "09:15",
                Observaciones = "Consumo elevado. Se inspeccionó el medidor, no presenta fuga visible.",
                ResultadoInspeccion = "Medidor no presenta fuga",
                Estado = "Con inconsistencia",
                EstadoMedidor = "Bueno",
                MotivoVisita = "Inspeccion por consumo alto",
                FontaneroId = fontanero.Id,
                FechaRegistro = mesAnterior.AddDays(2).AddHours(9)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Ana Lucia Vargas",
                NumeroAbonado = "A-1156",
                NumeroMedidor = "M-45823",
                Ubicacion = "San Juan de Dios, frente al parque",
                LecturaAnterior = 560.25m,
                LecturaActual = 560.25m,
                Consumo = 0m,
                ConsumoMesAnterior = 18.75m,
                ConsumoAlto = false,
                FechaLectura = mesAnterior.AddDays(5),
                HoraLectura = "10:00",
                Observaciones = "Abonado ausente en visita anterior. Consumo cero por ausencia prolongada.",
                Estado = "Revisada",
                EstadoMedidor = "Inaccesible",
                MotivoVisita = "Lectura mensual",
                FontaneroId = fontanero.Id,
                RevisadaPorAdminId = admin?.Id,
                FechaRegistro = mesAnterior.AddDays(5).AddHours(10)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Roberto Chaves Castro",
                NumeroAbonado = "A-1203",
                NumeroMedidor = "M-45824",
                Ubicacion = "Urbanización Los Laureles, casa 12",
                LecturaAnterior = 2100.00m,
                LecturaActual = 2135.75m,
                Consumo = 35.75m,
                ConsumoMesAnterior = 32.00m,
                ConsumoAlto = false,
                FechaLectura = hoy.AddDays(-3),
                HoraLectura = "07:45",
                Observaciones = "Lectura registrada sin novedad.",
                Estado = "Registrada",
                EstadoMedidor = "Bueno",
                MotivoVisita = "Lectura mensual",
                FontaneroId = fontanero.Id,
                FechaRegistro = hoy.AddDays(-3).AddHours(8)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Elena Rojas Mora",
                NumeroAbonado = "A-1245",
                NumeroMedidor = "M-45825",
                Ubicacion = "Barrio San Martín, 200 m oeste de la escuela",
                LecturaAnterior = 1780.00m,
                LecturaActual = 1780.00m,
                Consumo = 0m,
                FechaLectura = hoy,
                HoraLectura = "14:00",
                Observaciones = "Pendiente de lectura en campo.",
                Estado = "Pendiente",
                EstadoMedidor = "Bueno",
                FontaneroId = fontanero.Id,
                FechaRegistro = hoy
            },
            new LecturaMedidor
            {
                NombreAbonado = "Jose Alberto Quesada",
                NumeroAbonado = "A-1301",
                NumeroMedidor = "M-45826",
                Ubicacion = "Comunidad El Roble, calle principal",
                LecturaAnterior = 450.00m,
                LecturaActual = 520.00m,
                Consumo = 70.00m,
                ConsumoMesAnterior = 22.00m,
                ConsumoAlto = true,
                FechaLectura = hoy.AddDays(-1),
                HoraLectura = "11:30",
                Observaciones = "Consumo muy alto. Se tomó fotografía del medidor.",
                ResultadoInspeccion = "Se recomienda revision administrativa",
                Estado = "Con inconsistencia",
                EstadoMedidor = "Con posible fuga",
                MotivoVisita = "Inspeccion por consumo alto",
                FontaneroId = fontanero.Id,
                FechaRegistro = hoy.AddDays(-1).AddHours(12)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Patricia Solis Brenes",
                NumeroAbonado = "A-1350",
                NumeroMedidor = "M-45827",
                Ubicacion = "Palmares, Barrio La Granja",
                LecturaAnterior = 320.50m,
                LecturaActual = 298.00m,
                Consumo = -22.50m,
                ConsumoMesAnterior = 15.00m,
                ConsumoAlto = false,
                FechaLectura = hoy.AddDays(-5),
                HoraLectura = "16:20",
                Observaciones = "Lectura menor a la anterior. Posible error de digitación.",
                ObservacionAdmin = "Rechazada: lectura inconsistente, solicitar nueva visita.",
                Estado = "Rechazada",
                EstadoMedidor = "Bueno",
                FontaneroId = fontanero.Id,
                RevisadaPorAdminId = admin?.Id,
                FechaRegistro = hoy.AddDays(-5).AddHours(16)
            },
            new LecturaMedidor
            {
                NombreAbonado = "Francisco Navarro",
                NumeroAbonado = "A-1402",
                NumeroMedidor = "M-45828",
                Ubicacion = "San Juan, contiguo al colegio",
                LecturaAnterior = 675.00m,
                LecturaActual = 675.00m,
                Consumo = 0m,
                FechaLectura = hoy.AddDays(2),
                Observaciones = "Asignada para lectura del mes en curso.",
                Estado = "Pendiente",
                FontaneroId = fontanero.Id,
                FechaRegistro = hoy
            }
        );

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
