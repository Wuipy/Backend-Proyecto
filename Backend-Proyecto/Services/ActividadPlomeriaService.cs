using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Services;

public interface IActividadPlomeriaService
{
    Task<IReadOnlyList<ActividadPlomeriaResponseDto>> ListarAsync();
    Task<ActividadPlomeriaResponseDto> CrearAsync(ActividadPlomeriaDto dto);
    Task<ActividadPlomeriaResponseDto?> ActualizarAsync(string id, ActividadPlomeriaDto dto);
    Task<ActividadPlomeriaResponseDto?> CambiarEstadoAsync(string id, CambiarEstadoActividadDto? dto = null);
    Task<bool> EliminarAsync(string id);
}

public class ActividadPlomeriaService(AppDbContext context) : IActividadPlomeriaService
{
    public async Task<IReadOnlyList<ActividadPlomeriaResponseDto>> ListarAsync()
    {
        var actividades = await context.ActividadesPlomeria
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

        return actividades.Select(Mappers.ToActividadResponse).ToList();
    }

    public async Task<ActividadPlomeriaResponseDto> CrearAsync(ActividadPlomeriaDto dto)
    {
        ValidarActividad(dto);

        var actividad = new ActividadPlomeria
        {
            Id = GenerarId(),
            Tipo = dto.Tipo,
            Cliente = dto.Cliente.Trim(),
            Ubicacion = dto.Ubicacion.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Estado = dto.Estado,
            Prioridad = dto.Prioridad,
            NotasSeguimiento = string.IsNullOrWhiteSpace(dto.NotasSeguimiento) ? null : dto.NotasSeguimiento.Trim(),
            NumeroAveriaVinculada = NormalizarNumeroAveria(dto.NumeroAveriaVinculada)
        };

        context.ActividadesPlomeria.Add(actividad);
        await context.SaveChangesAsync();

        return Mappers.ToActividadResponse(actividad);
    }

    public async Task<ActividadPlomeriaResponseDto?> ActualizarAsync(string id, ActividadPlomeriaDto dto)
    {
        ValidarActividad(dto);

        var actividad = await context.ActividadesPlomeria.FindAsync(id);
        if (actividad is null) return null;

        actividad.Tipo = dto.Tipo;
        actividad.Cliente = dto.Cliente.Trim();
        actividad.Ubicacion = dto.Ubicacion.Trim();
        actividad.Descripcion = dto.Descripcion.Trim();
        actividad.Estado = dto.Estado;
        actividad.Prioridad = dto.Prioridad;
        actividad.NotasSeguimiento = string.IsNullOrWhiteSpace(dto.NotasSeguimiento) ? null : dto.NotasSeguimiento.Trim();
        actividad.NumeroAveriaVinculada = NormalizarNumeroAveria(dto.NumeroAveriaVinculada);
        actividad.FechaActualizacion = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Mappers.ToActividadResponse(actividad);
    }

    public async Task<ActividadPlomeriaResponseDto?> CambiarEstadoAsync(string id, CambiarEstadoActividadDto? dto = null)
    {
        var actividad = await context.ActividadesPlomeria.FindAsync(id);
        if (actividad is null) return null;

        if (dto is not null && !string.IsNullOrWhiteSpace(dto.Estado))
        {
            if (!DbSeeder.EsEstadoActividadValido(dto.Estado))
            {
                throw new ArgumentException("El estado de actividad no es valido.");
            }

            actividad.Estado = dto.Estado;
        }
        else
        {
            actividad.Estado = DbSeeder.SiguienteEstadoActividad(actividad.Estado);
        }

        actividad.FechaActualizacion = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Mappers.ToActividadResponse(actividad);
    }

    public async Task<bool> EliminarAsync(string id)
    {
        var actividad = await context.ActividadesPlomeria.FindAsync(id);
        if (actividad is null) return false;

        context.ActividadesPlomeria.Remove(actividad);
        await context.SaveChangesAsync();
        return true;
    }

    private static void ValidarActividad(ActividadPlomeriaDto dto)
    {
        if (!DbSeeder.EsTipoActividadValido(dto.Tipo))
        {
            throw new ArgumentException("El tipo de actividad no es valido.");
        }

        if (!DbSeeder.EsEstadoActividadValido(dto.Estado))
        {
            throw new ArgumentException("El estado de actividad no es valido.");
        }

        if (!DbSeeder.EsPrioridadActividadValida(dto.Prioridad))
        {
            throw new ArgumentException("La prioridad de actividad no es valida.");
        }
    }

    private static string? NormalizarNumeroAveria(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return null;
        return numero.Trim().ToUpperInvariant();
    }

    private static string GenerarId() =>
        $"ACT-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds():x}-{Guid.NewGuid().ToString("N")[..4]}";
}

public interface IContenidoService
{
    Task<IReadOnlyList<ComunicadoDto>> ListarComunicadosAsync();
    Task<IReadOnlyList<ProyectoDto>> ListarProyectosAsync();
}

public class ContenidoService(AppDbContext context) : IContenidoService
{
    public async Task<IReadOnlyList<ComunicadoDto>> ListarComunicadosAsync()
    {
        return await context.Comunicados
            .OrderByDescending(c => c.Id)
            .Select(c => new ComunicadoDto
            {
                Fecha = c.Fecha,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProyectoDto>> ListarProyectosAsync()
    {
        return await context.Proyectos
            .OrderBy(p => p.Id)
            .Select(p => new ProyectoDto
            {
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Estado = p.Estado
            })
            .ToListAsync();
    }
}
