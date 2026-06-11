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

    public static readonly string[] TiposActividadFontanero =
    [
        "Reparacion de averia",
        "Lectura de medidor",
        "Cambio de medidor",
        "Revision de tuberia",
        "Instalacion de servicio",
        "Mantenimiento preventivo",
        "Revision de presion de agua",
        "Otro"
    ];

    private static readonly string[] TiposSolicitud =
    [
        "Nueva conexion",
        "Disponibilidad de agua",
        "Suspension temporal",
        "Cancelacion del servicio",
        "Cambio de titular"
    ];

    private static readonly string[] TiposActividad =
    [
        "Control de Fugas",
        "Toma de presion",
        "Visita de Campo",
        "Control de Aforos",
        "Control Operativo"
    ];

    public static readonly string[] PrioridadesActividad =
    [
        "Baja",
        "Media",
        "Alta"
    ];

    private static readonly string[] EstadosActividad =
    [
        "Pendiente",
        "En progreso",
        "Completado"
    ];

    public static async Task SeedAsync(AppDbContext context, AdminSettings adminSettings)
    {
        await context.Database.EnsureCreatedAsync();
        await AplicarActualizacionesEsquemaAsync(context);
        await CrearTablasModulosAsync(context);
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

    public static bool EsTipoActividadValido(string tipo) =>
        TiposActividad.Contains(tipo, StringComparer.Ordinal);

    public static bool EsPrioridadActividadValida(string prioridad) =>
        PrioridadesActividad.Contains(prioridad, StringComparer.Ordinal);

    public static bool EsEstadoActividadValido(string estado) =>
        EstadosActividad.Contains(estado, StringComparer.Ordinal);

    public static string SiguienteEstadoActividad(string estadoActual) =>
        estadoActual switch
        {
            "Pendiente" => "En progreso",
            "En progreso" => "Completado",
            _ => "Pendiente"
        };

    private static async Task AplicarActualizacionesEsquemaAsync(AppDbContext context)
    {
        var alteraciones = new[]
        {
            "ALTER TABLE Averias ADD COLUMN FontaneroAsignadoId INTEGER NULL",
            "ALTER TABLE Averias ADD COLUMN NotasAtencion TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN FechaUltimaActualizacion TEXT NULL",
            "ALTER TABLE ActividadesPlomeria ADD COLUMN Prioridad TEXT NOT NULL DEFAULT 'Media'",
            "ALTER TABLE ActividadesPlomeria ADD COLUMN NotasSeguimiento TEXT NULL",
            "ALTER TABLE ActividadesPlomeria ADD COLUMN NumeroAveriaVinculada TEXT NULL",
            "ALTER TABLE ActividadesPlomeria ADD COLUMN FechaActualizacion TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN Prioridad TEXT NOT NULL DEFAULT 'Media'",
            "ALTER TABLE Averias ADD COLUMN ObservacionesAdmin TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN DescripcionTrabajo TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN MaterialesUtilizados TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN EvidenciaTrabajoNombre TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN EvidenciaTrabajoBase64 TEXT NULL"
        };

        foreach (var sql in alteraciones)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // La columna ya existe en bases de datos actualizadas.
            }
        }
    }

    private static async Task CrearTablasModulosAsync(AppDbContext context)
    {
        var tablas = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS AveriasHistorial (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AveriaId INTEGER NOT NULL,
                Accion TEXT NOT NULL,
                ValorAnterior TEXT NULL,
                ValorNuevo TEXT NULL,
                Usuario TEXT NULL,
                Fecha TEXT NOT NULL,
                FOREIGN KEY (AveriaId) REFERENCES Averias(Id) ON DELETE CASCADE
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS ActividadesFontanero (
                Id TEXT PRIMARY KEY,
                FontaneroId INTEGER NOT NULL,
                FechaActividad TEXT NOT NULL,
                HoraInicio TEXT NULL,
                HoraFin TEXT NULL,
                Tipo TEXT NOT NULL,
                Descripcion TEXT NOT NULL,
                Ubicacion TEXT NOT NULL,
                NumeroAveriaVinculada TEXT NULL,
                LecturaMedidorId INTEGER NULL,
                MaterialesUtilizados TEXT NULL,
                Observaciones TEXT NULL,
                Estado TEXT NOT NULL DEFAULT 'Pendiente',
                EstadoValidacion TEXT NOT NULL DEFAULT 'Pendiente',
                ObservacionValidacion TEXT NULL,
                FechaCreacion TEXT NOT NULL,
                FechaActualizacion TEXT NULL,
                FOREIGN KEY (FontaneroId) REFERENCES Usuarios(Id),
                FOREIGN KEY (LecturaMedidorId) REFERENCES LecturasMedidor(Id)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS LecturasMedidor (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NombreAbonado TEXT NOT NULL,
                NumeroMedidor TEXT NOT NULL,
                CedulaAbonado TEXT NULL,
                LecturaAnterior REAL NOT NULL,
                LecturaActual REAL NOT NULL,
                Consumo REAL NOT NULL,
                ConsumoMesAnterior REAL NULL,
                FechaLectura TEXT NOT NULL,
                Observaciones TEXT NULL,
                Estado TEXT NOT NULL DEFAULT 'Pendiente',
                FontaneroId INTEGER NOT NULL,
                FechaRegistro TEXT NOT NULL,
                FOREIGN KEY (FontaneroId) REFERENCES Usuarios(Id)
            )
            """
        };

        foreach (var sql in tablas)
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }

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
                "UPDATE Averias SET Estado = {0} WHERE Estado = {1}", nuevo, anterior);
        }
    }
}
