using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Models.Entities;
using Backend_Proyecto.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Services;

public interface IActividadFontaneroService
{
    Task<IReadOnlyList<ActividadFontaneroResponseDto>> ListarTodasAsync();
    Task<IReadOnlyList<ActividadFontaneroResponseDto>> ListarPorFontaneroAsync(int fontaneroId);
    Task<ActividadFontaneroResponseDto> CrearAsync(ActividadFontaneroDto dto, int fontaneroId);
    Task<ActividadFontaneroResponseDto?> ActualizarAsync(string id, ActividadFontaneroDto dto, int fontaneroId);
    Task<ActividadFontaneroResponseDto?> ValidarAsync(string id, ValidarActividadFontaneroDto dto);
}

public class ActividadFontaneroService(AppDbContext context) : IActividadFontaneroService
{
    public async Task<IReadOnlyList<ActividadFontaneroResponseDto>> ListarTodasAsync()
    {
        var actividades = await context.ActividadesFontanero
            .Include(a => a.Fontanero)
            .OrderByDescending(a => a.FechaActividad)
            .ToListAsync();

        return actividades.Select(Mappers.ToActividadFontaneroResponse).ToList();
    }

    public async Task<IReadOnlyList<ActividadFontaneroResponseDto>> ListarPorFontaneroAsync(int fontaneroId)
    {
        var actividades = await context.ActividadesFontanero
            .Include(a => a.Fontanero)
            .Where(a => a.FontaneroId == fontaneroId)
            .OrderByDescending(a => a.FechaActividad)
            .ToListAsync();

        return actividades.Select(Mappers.ToActividadFontaneroResponse).ToList();
    }

    public async Task<ActividadFontaneroResponseDto> CrearAsync(ActividadFontaneroDto dto, int fontaneroId)
    {
        ValidarDto(dto);

        var actividad = new ActividadFontanero
        {
            Id = GenerarId(),
            FontaneroId = fontaneroId,
            FechaActividad = ParseFecha(dto.FechaActividad),
            HoraInicio = dto.HoraInicio,
            HoraFin = dto.HoraFin,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion.Trim(),
            Ubicacion = dto.Ubicacion.Trim(),
            NumeroAveriaVinculada = NormalizarAveria(dto.NumeroAveriaVinculada),
            LecturaMedidorId = dto.LecturaMedidorId,
            MaterialesUtilizados = dto.MaterialesUtilizados?.Trim(),
            Observaciones = dto.Observaciones?.Trim(),
            Estado = dto.Estado
        };

        context.ActividadesFontanero.Add(actividad);
        await context.SaveChangesAsync();
        await context.Entry(actividad).Reference(a => a.Fontanero).LoadAsync();
        return Mappers.ToActividadFontaneroResponse(actividad);
    }

    public async Task<ActividadFontaneroResponseDto?> ActualizarAsync(string id, ActividadFontaneroDto dto, int fontaneroId)
    {
        ValidarDto(dto);

        var actividad = await context.ActividadesFontanero
            .Include(a => a.Fontanero)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actividad is null || actividad.FontaneroId != fontaneroId) return null;

        if (actividad.EstadoValidacion != "Pendiente")
        {
            throw new ArgumentException("No puede editar una actividad ya validada o rechazada.");
        }

        actividad.FechaActividad = ParseFecha(dto.FechaActividad);
        actividad.HoraInicio = dto.HoraInicio;
        actividad.HoraFin = dto.HoraFin;
        actividad.Tipo = dto.Tipo;
        actividad.Descripcion = dto.Descripcion.Trim();
        actividad.Ubicacion = dto.Ubicacion.Trim();
        actividad.NumeroAveriaVinculada = NormalizarAveria(dto.NumeroAveriaVinculada);
        actividad.LecturaMedidorId = dto.LecturaMedidorId;
        actividad.MaterialesUtilizados = dto.MaterialesUtilizados?.Trim();
        actividad.Observaciones = dto.Observaciones?.Trim();
        actividad.Estado = dto.Estado;
        actividad.FechaActualizacion = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Mappers.ToActividadFontaneroResponse(actividad);
    }

    public async Task<ActividadFontaneroResponseDto?> ValidarAsync(string id, ValidarActividadFontaneroDto dto)
    {
        if (!EstadosValidacionActividad.EsValido(dto.EstadoValidacion))
        {
            throw new ArgumentException("El estado de validacion no es valido.");
        }

        if (dto.EstadoValidacion == "Rechazada" && string.IsNullOrWhiteSpace(dto.ObservacionValidacion))
        {
            throw new ArgumentException("Debe indicar una observacion al rechazar la actividad.");
        }

        var actividad = await context.ActividadesFontanero
            .Include(a => a.Fontanero)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actividad is null) return null;

        actividad.EstadoValidacion = dto.EstadoValidacion;
        actividad.ObservacionValidacion = dto.ObservacionValidacion?.Trim();
        actividad.FechaActualizacion = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Mappers.ToActividadFontaneroResponse(actividad);
    }

    private static void ValidarDto(ActividadFontaneroDto dto)
    {
        if (!DbSeeder.EsTipoActividadFontaneroValido(dto.Tipo))
        {
            throw new ArgumentException("El tipo de actividad no es valido.");
        }

        if (!DbSeeder.EsEstadoActividadFontaneroValido(dto.Estado))
        {
            throw new ArgumentException("El estado de actividad no es valido.");
        }
    }

    private static DateTime ParseFecha(string fecha)
    {
        if (DateTime.TryParse(fecha, out var resultado))
        {
            return DateTime.SpecifyKind(resultado, DateTimeKind.Utc);
        }

        throw new ArgumentException("La fecha de actividad no es valida.");
    }

    private static string? NormalizarAveria(string? numero) =>
        string.IsNullOrWhiteSpace(numero) ? null : numero.Trim().ToUpperInvariant();

    private static string GenerarId() =>
        $"AF-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():x}-{Guid.NewGuid().ToString("N")[..4]}";
}
