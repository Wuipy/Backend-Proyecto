using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Models.Entities;
using Backend_Proyecto.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Services;

public interface ISeguimientoService
{
    Task<string> GenerarNumeroAsync(string prefijo);
}

public class SeguimientoService(AppDbContext context) : ISeguimientoService
{
    public async Task<string> GenerarNumeroAsync(string prefijo)
    {
        var secuencia = await context.SecuenciasContador
            .FirstOrDefaultAsync(s => s.Prefijo == prefijo);

        if (secuencia is null)
        {
            secuencia = new SecuenciaContador { Prefijo = prefijo, UltimoValor = 0 };
            context.SecuenciasContador.Add(secuencia);
        }

        secuencia.UltimoValor += 1;
        await context.SaveChangesAsync();

        return SeguimientoGenerator.Crear(prefijo, secuencia.UltimoValor);
    }
}

public interface IAveriaService
{
    Task<RegistroAveriaResponseDto> CrearAsync(CrearAveriaDto dto);
    Task<IReadOnlyList<AveriaResponseDto>> ListarAsync();
    Task<IReadOnlyList<AveriaResponseDto>> ListarParaGestionAsync();
    Task<IReadOnlyList<AveriaResponseDto>> ListarAsignadasAsync(int fontaneroId);
    Task<IReadOnlyList<AveriaHistorialDto>> ObtenerHistorialAsync(string numeroSeguimiento);
    Task<AveriaResponseDto?> ObtenerPorNumeroAsync(string numeroSeguimiento);
    Task<AveriaResponseDto?> ActualizarEstadoAsync(string numeroSeguimiento, string estado, string? usuario);
    Task<AveriaResponseDto?> AsignarFontaneroAsync(string numeroSeguimiento, int fontaneroId, string? usuario, bool forzarAsignacion);
    Task<AveriaResponseDto?> ActualizarPrioridadAsync(string numeroSeguimiento, string prioridad, string? usuario);
    Task<AveriaResponseDto?> ActualizarObservacionesAdminAsync(string numeroSeguimiento, string? observaciones, string? usuario);
    Task<AveriaResponseDto?> ActualizarAtencionFontaneroAsync(string numeroSeguimiento, AtencionFontaneroAveriaDto dto, string? usuario);
    Task<AveriaResponseDto?> ActualizarNotasAsync(string numeroSeguimiento, string? notasAtencion, string? usuario);
}

public class AveriaService(AppDbContext context, ISeguimientoService seguimientoService) : IAveriaService
{
    public async Task<RegistroAveriaResponseDto> CrearAsync(CrearAveriaDto dto)
    {
        if (!DbSeeder.EsTipoAveriaValido(dto.Tipo))
        {
            throw new ArgumentException("El tipo de averia seleccionado no es valido.");
        }

        var numeroSeguimiento = await seguimientoService.GenerarNumeroAsync("AV");

        var averia = new Averia
        {
            NumeroSeguimiento = numeroSeguimiento,
            Nombre = dto.Nombre.Trim(),
            Telefono = dto.Telefono.Trim(),
            Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim(),
            Direccion = dto.Direccion.Trim(),
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim(),
            Estado = "Pendiente",
            Prioridad = "Media",
            FotoNombre = dto.FotoNombre,
            FotoBase64 = dto.FotoBase64
        };

        context.Averias.Add(averia);
        await context.SaveChangesAsync();
        await RegistrarHistorialAsync(averia.Id, "Creacion", null, "Pendiente", "Portal publico");
        await context.SaveChangesAsync();

        return new RegistroAveriaResponseDto
        {
            NumeroSeguimiento = numeroSeguimiento,
            Mensaje = $"Su reporte fue registrado correctamente. Numero de seguimiento: {numeroSeguimiento}. Estado: Pendiente."
        };
    }

    public async Task<IReadOnlyList<AveriaResponseDto>> ListarAsync()
    {
        var averias = await context.Averias
            .Include(a => a.FontaneroAsignado)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return averias.Select(Mappers.ToAveriaResponse).ToList();
    }

    public async Task<IReadOnlyList<AveriaResponseDto>> ListarParaGestionAsync()
    {
        var averias = await context.Averias
            .Include(a => a.FontaneroAsignado)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return averias.Select(Mappers.ToAveriaResponse).ToList();
    }

    public async Task<IReadOnlyList<AveriaResponseDto>> ListarAsignadasAsync(int fontaneroId)
    {
        var averias = await context.Averias
            .Include(a => a.FontaneroAsignado)
            .Where(a => a.FontaneroAsignadoId == fontaneroId)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return averias.Select(Mappers.ToAveriaResponse).ToList();
    }

    public async Task<IReadOnlyList<AveriaHistorialDto>> ObtenerHistorialAsync(string numeroSeguimiento)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return [];

        var historial = await context.AveriasHistorial
            .Where(h => h.AveriaId == averia.Id)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();

        return historial.Select(Mappers.ToHistorialResponse).ToList();
    }

    public async Task<AveriaResponseDto?> ObtenerPorNumeroAsync(string numeroSeguimiento)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        return averia is null ? null : Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> ActualizarEstadoAsync(string numeroSeguimiento, string estado, string? usuario)
    {
        var estadoNormalizado = EstadosAveria.Normalizar(estado);
        if (!EstadosAveria.EsValidoAdmin(estadoNormalizado))
        {
            throw new ArgumentException("El estado del reporte no es valido.");
        }

        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        var anterior = averia.Estado;
        averia.Estado = estadoNormalizado;
        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await RegistrarHistorialAsync(averia.Id, "Estado", anterior, estadoNormalizado, usuario);
        await context.SaveChangesAsync();

        await context.Entry(averia).Reference(a => a.FontaneroAsignado).LoadAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> AsignarFontaneroAsync(
        string numeroSeguimiento,
        int fontaneroId,
        string? usuario,
        bool forzarAsignacion)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        var fontanero = await context.Usuarios.FindAsync(fontaneroId);
        if (fontanero is null || fontanero.Rol != Roles.Fontanero)
        {
            throw new ArgumentException("El fontanero indicado no es valido.");
        }

        if (averia.FontaneroAsignadoId is not null && averia.FontaneroAsignadoId != fontaneroId && !forzarAsignacion)
        {
            throw new ArgumentException("Este reporte ya esta asignado a otro fontanero.");
        }

        var anterior = averia.FontaneroAsignado?.NombreUsuario;
        averia.FontaneroAsignadoId = fontaneroId;
        if (averia.Estado is "Pendiente" or "En revision")
        {
            averia.Estado = "Asignada";
        }

        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await RegistrarHistorialAsync(averia.Id, "Asignacion", anterior, fontanero.NombreUsuario, usuario);
        await context.SaveChangesAsync();
        await context.Entry(averia).Reference(a => a.FontaneroAsignado).LoadAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> ActualizarPrioridadAsync(string numeroSeguimiento, string prioridad, string? usuario)
    {
        if (!PrioridadesAveria.EsValida(prioridad))
        {
            throw new ArgumentException("La prioridad no es valida.");
        }

        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        var anterior = averia.Prioridad;
        averia.Prioridad = prioridad;
        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await RegistrarHistorialAsync(averia.Id, "Prioridad", anterior, prioridad, usuario);
        await context.SaveChangesAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> ActualizarObservacionesAdminAsync(
        string numeroSeguimiento,
        string? observaciones,
        string? usuario)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        var anterior = averia.ObservacionesAdmin;
        averia.ObservacionesAdmin = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await RegistrarHistorialAsync(averia.Id, "Observaciones admin", anterior, averia.ObservacionesAdmin, usuario);
        await context.SaveChangesAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> ActualizarAtencionFontaneroAsync(
        string numeroSeguimiento,
        AtencionFontaneroAveriaDto dto,
        string? usuario)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        if (!string.IsNullOrWhiteSpace(dto.DescripcionTrabajo))
        {
            averia.DescripcionTrabajo = dto.DescripcionTrabajo.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.MaterialesUtilizados))
        {
            averia.MaterialesUtilizados = dto.MaterialesUtilizados.Trim();
        }

        if (dto.NotasAtencion is not null)
        {
            averia.NotasAtencion = string.IsNullOrWhiteSpace(dto.NotasAtencion) ? null : dto.NotasAtencion.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.EvidenciaBase64))
        {
            averia.EvidenciaTrabajoNombre = dto.EvidenciaNombre;
            averia.EvidenciaTrabajoBase64 = dto.EvidenciaBase64;
        }

        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            var estado = EstadosAveria.Normalizar(dto.Estado);
            if (!EstadosAveria.EsValidoFontanero(estado))
            {
                throw new ArgumentException("El estado no es valido para el fontanero.");
            }

            var anterior = averia.Estado;
            averia.Estado = estado;
            await RegistrarHistorialAsync(averia.Id, "Estado fontanero", anterior, estado, usuario);
        }

        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    public async Task<AveriaResponseDto?> ActualizarNotasAsync(string numeroSeguimiento, string? notasAtencion, string? usuario)
    {
        var averia = await BuscarAveriaAsync(numeroSeguimiento);
        if (averia is null) return null;

        var anterior = averia.NotasAtencion;
        averia.NotasAtencion = string.IsNullOrWhiteSpace(notasAtencion) ? null : notasAtencion.Trim();
        averia.FechaUltimaActualizacion = DateTime.UtcNow;
        await RegistrarHistorialAsync(averia.Id, "Notas", anterior, averia.NotasAtencion, usuario);
        await context.SaveChangesAsync();
        return Mappers.ToAveriaResponse(averia);
    }

    private async Task RegistrarHistorialAsync(
        int averiaId,
        string accion,
        string? valorAnterior,
        string? valorNuevo,
        string? usuario)
    {
        context.AveriasHistorial.Add(new AveriaHistorial
        {
            AveriaId = averiaId,
            Accion = accion,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            Usuario = usuario,
            Fecha = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }

    private async Task<Averia?> BuscarAveriaAsync(string numeroSeguimiento) =>
        await context.Averias
            .Include(a => a.FontaneroAsignado)
            .FirstOrDefaultAsync(a => a.NumeroSeguimiento.ToUpper() == numeroSeguimiento.Trim().ToUpper());
}

public interface ISolicitudService
{
    Task<RegistroSolicitudResponseDto> CrearAsync(CrearSolicitudDto dto);
}

public class SolicitudService(AppDbContext context, ISeguimientoService seguimientoService) : ISolicitudService
{
    public async Task<RegistroSolicitudResponseDto> CrearAsync(CrearSolicitudDto dto)
    {
        if (!DbSeeder.EsTipoSolicitudValido(dto.Tipo))
        {
            throw new ArgumentException("El tipo de solicitud seleccionado no es valido.");
        }

        var numeroSeguimiento = await seguimientoService.GenerarNumeroAsync("SOL");

        var solicitud = new Solicitud
        {
            NumeroSeguimiento = numeroSeguimiento,
            Nombre = dto.Nombre.Trim(),
            Cedula = dto.Cedula.Trim(),
            Telefono = dto.Telefono.Trim(),
            Correo = dto.Correo.Trim(),
            Direccion = dto.Direccion.Trim(),
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim()
        };

        context.Solicitudes.Add(solicitud);
        await context.SaveChangesAsync();

        return new RegistroSolicitudResponseDto
        {
            NumeroSeguimiento = numeroSeguimiento,
            Mensaje = $"Solicitud registrada correctamente. Numero de seguimiento: {numeroSeguimiento}"
        };
    }
}

public interface IConsultaSeguimientoService
{
    Task<SeguimientoResponseDto?> ConsultarAsync(string numeroSeguimiento);
}

public class ConsultaSeguimientoService(AppDbContext context) : IConsultaSeguimientoService
{
    public async Task<SeguimientoResponseDto?> ConsultarAsync(string numeroSeguimiento)
    {
        var numero = numeroSeguimiento.Trim().ToUpper();

        if (numero.StartsWith("AV-"))
        {
            var averia = await context.Averias
                .Include(a => a.FontaneroAsignado)
                .FirstOrDefaultAsync(a => a.NumeroSeguimiento.ToUpper() == numero);

            if (averia is null) return null;

            var detalle = Mappers.ToAveriaResponse(averia);
            return new SeguimientoResponseDto
            {
                NumeroSeguimiento = averia.NumeroSeguimiento,
                Tipo = "AV",
                Estado = averia.Estado,
                MensajeEstado = EstadoMensajes.ParaAveria(averia.Estado),
                DetalleAveria = detalle
            };
        }

        if (numero.StartsWith("SOL-"))
        {
            var solicitud = await context.Solicitudes
                .FirstOrDefaultAsync(s => s.NumeroSeguimiento.ToUpper() == numero);

            if (solicitud is null) return null;

            return new SeguimientoResponseDto
            {
                NumeroSeguimiento = solicitud.NumeroSeguimiento,
                Tipo = "SOL",
                Estado = solicitud.Estado,
                MensajeEstado = EstadoMensajes.ParaSolicitud(solicitud.Estado)
            };
        }

        return null;
    }
}
