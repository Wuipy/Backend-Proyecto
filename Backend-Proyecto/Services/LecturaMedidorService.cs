using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Models.Entities;
using Backend_Proyecto.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Services;

public interface ILecturaMedidorService
{
    Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarTodasAsync();
    Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarPorFontaneroAsync(int fontaneroId);
    Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorMedidorAsync(string numeroMedidor);
    Task<LecturaMedidorResponseDto> CrearAsync(CrearLecturaMedidorDto dto, int fontaneroId);
    Task<LecturaMedidorResponseDto?> ActualizarAsync(int id, ActualizarLecturaMedidorDto dto, string rol, int usuarioId);
}

public class LecturaMedidorService(AppDbContext context) : ILecturaMedidorService
{
    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarTodasAsync()
    {
        var lecturas = await context.LecturasMedidor
            .Include(l => l.Fontanero)
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarPorFontaneroAsync(int fontaneroId)
    {
        var lecturas = await context.LecturasMedidor
            .Include(l => l.Fontanero)
            .Where(l => l.FontaneroId == fontaneroId)
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorMedidorAsync(string numeroMedidor)
    {
        var normalizado = numeroMedidor.Trim().ToUpperInvariant();
        var lecturas = await context.LecturasMedidor
            .Include(l => l.Fontanero)
            .Where(l => l.NumeroMedidor.ToUpper() == normalizado)
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<LecturaMedidorResponseDto> CrearAsync(CrearLecturaMedidorDto dto, int fontaneroId)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreAbonado) || string.IsNullOrWhiteSpace(dto.NumeroMedidor))
        {
            throw new ArgumentException("Abonado y numero de medidor son obligatorios.");
        }

        var consumo = dto.LecturaActual - dto.LecturaAnterior;
        var estado = consumo < 0 ? "Con inconsistencia" : "Registrada";

        var consumoMesAnterior = await ObtenerConsumoMesAnteriorAsync(dto.NumeroMedidor);

        var lectura = new LecturaMedidor
        {
            NombreAbonado = dto.NombreAbonado.Trim(),
            NumeroMedidor = dto.NumeroMedidor.Trim(),
            CedulaAbonado = string.IsNullOrWhiteSpace(dto.CedulaAbonado) ? null : dto.CedulaAbonado.Trim(),
            LecturaAnterior = dto.LecturaAnterior,
            LecturaActual = dto.LecturaActual,
            Consumo = consumo,
            ConsumoMesAnterior = consumoMesAnterior,
            FechaLectura = ParseFecha(dto.FechaLectura),
            Observaciones = dto.Observaciones?.Trim(),
            Estado = estado,
            FontaneroId = fontaneroId
        };

        context.LecturasMedidor.Add(lectura);
        await context.SaveChangesAsync();
        await context.Entry(lectura).Reference(l => l.Fontanero).LoadAsync();
        return Mappers.ToLecturaResponse(lectura);
    }

    public async Task<LecturaMedidorResponseDto?> ActualizarAsync(
        int id,
        ActualizarLecturaMedidorDto dto,
        string rol,
        int usuarioId)
    {
        var lectura = await context.LecturasMedidor
            .Include(l => l.Fontanero)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lectura is null) return null;

        if (rol == Roles.Fontanero && lectura.FontaneroId != usuarioId)
        {
            throw new UnauthorizedAccessException("No puede modificar lecturas de otro fontanero.");
        }

        if (dto.LecturaActual.HasValue)
        {
            ValidarLecturas(lectura.LecturaAnterior, dto.LecturaActual.Value);
            lectura.LecturaActual = dto.LecturaActual.Value;
            lectura.Consumo = lectura.LecturaActual - lectura.LecturaAnterior;
            if (lectura.Consumo < 0)
            {
                lectura.Estado = "Con inconsistencia";
            }
        }

        if (dto.Observaciones is not null)
        {
            lectura.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            if (!EstadosLecturaMedidor.EsValido(dto.Estado))
            {
                throw new ArgumentException("El estado de lectura no es valido.");
            }

            if (rol == Roles.Fontanero && dto.Estado is not ("Registrada" or "Con inconsistencia"))
            {
                throw new ArgumentException("El fontanero no puede aplicar ese estado.");
            }

            lectura.Estado = dto.Estado;
        }

        await context.SaveChangesAsync();
        return Mappers.ToLecturaResponse(lectura);
    }

    private static void ValidarLecturas(decimal anterior, decimal actual)
    {
        if (actual < anterior)
        {
            throw new ArgumentException("La lectura actual no puede ser menor que la lectura anterior.");
        }
    }

    private async Task<decimal?> ObtenerConsumoMesAnteriorAsync(string numeroMedidor)
    {
        var ultima = await context.LecturasMedidor
            .Where(l => l.NumeroMedidor.ToUpper() == numeroMedidor.Trim().ToUpper())
            .OrderByDescending(l => l.FechaLectura)
            .FirstOrDefaultAsync();

        return ultima?.Consumo;
    }

    private static DateTime ParseFecha(string fecha)
    {
        if (DateTime.TryParse(fecha, out var resultado))
        {
            return DateTime.SpecifyKind(resultado, DateTimeKind.Utc);
        }

        throw new ArgumentException("La fecha de lectura no es valida.");
    }
}
