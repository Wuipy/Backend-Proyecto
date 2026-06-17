using System.Data;
using System.Data.Common;
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

    private static async Task AplicarActualizacionesEsquemaAsync(AppDbContext context)
    {
        var alteraciones = new[]
        {
            "ALTER TABLE Averias ADD COLUMN FontaneroAsignadoId INTEGER NULL",
            "ALTER TABLE Averias ADD COLUMN NotasAtencion TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN FechaUltimaActualizacion TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN Prioridad TEXT NOT NULL DEFAULT 'Media'",
            "ALTER TABLE Averias ADD COLUMN ObservacionesAdmin TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN DescripcionTrabajo TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN MaterialesUtilizados TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN EvidenciaTrabajoNombre TEXT NULL",
            "ALTER TABLE Averias ADD COLUMN EvidenciaTrabajoBase64 TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN AbonadoNumero TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN NombreAbonado TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN LugarVisita TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN MotivoVisita TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN LecturaAnteriorM3 REAL NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN LecturaActualM3 REAL NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ConsumoRegistradoM3 REAL NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN EstadoMedidor TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN DetectoFuga TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ResultadoInspeccion TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN AccionRecomendada TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN FotoMedidorNombre TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN FotoMedidorBase64 TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN AforoNumero TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN LugarPrueba TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN HoraPrueba TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ResultadoPsi REAL NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN DiametroTuberia TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ObservacionesPresion TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN PruebaNumero TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN LugarCasa TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN HoraControl TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN CloroResidual TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN Turbiedad TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN Ph REAL NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN Olor TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN Sabor TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ObservacionesControlOperativo TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN DetalleTrabajoRealizado TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN ResultadoTrabajo TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN RequiereSeguimiento TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN PrioridadSeguimiento TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN FotoEvidenciaNombre TEXT NULL",
            "ALTER TABLE ActividadesFontanero ADD COLUMN FotoEvidenciaBase64 TEXT NULL"
        };

        foreach (var sql in alteraciones)
        {
            var partes = sql.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length < 6)
            {
                continue;
            }

            var tabla = partes[2];
            var columna = partes[5];

            if (await ColumnaExisteAsync(context, tabla, columna))
            {
                continue;
            }

            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }

    private static async Task<bool> ColumnaExisteAsync(AppDbContext context, string tabla, string columna)
    {
        var conexion = context.Database.GetDbConnection();
        var debeCerrar = conexion.State != ConnectionState.Open;

        if (debeCerrar)
        {
            await conexion.OpenAsync();
        }

        try
        {
            await using var comando = conexion.CreateCommand();
            comando.CommandText = $"PRAGMA table_info(\"{tabla}\");";
            await using DbDataReader lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var nombre = lector.GetString(1);
                if (string.Equals(nombre, columna, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (debeCerrar)
            {
                await conexion.CloseAsync();
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
