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
    Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarPendientesPorFontaneroAsync(int fontaneroId);
    Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorMedidorAsync(string numeroMedidor);
    Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorAbonadoAsync(string numeroAbonado);
    Task<IReadOnlyList<HistorialLecturaDto>> HistorialCambiosAsync(int lecturaId);
    Task<ResumenLecturasMedidorDto> ResumenAdminAsync();
    Task<ResumenLecturasMedidorDto> ResumenFontaneroAsync(int fontaneroId);
    Task<ReporteLecturasMedidorDto> GenerarReporteAsync(string tipo);
    Task<LecturaMedidorResponseDto> CrearAsync(CrearLecturaMedidorDto dto, int fontaneroId);
    Task<LecturaMedidorResponseDto> AsignarAsync(AsignarLecturaMedidorDto dto, int adminId);
    Task<LecturaMedidorResponseDto?> ActualizarAsync(int id, ActualizarLecturaMedidorDto dto, string rol, int usuarioId, string nombreUsuario);
}

public class LecturaMedidorService(AppDbContext context) : ILecturaMedidorService
{
    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarTodasAsync()
    {
        var lecturas = await ConsultaBase()
            .OrderByDescending(l => l.FechaLectura)
            .ThenByDescending(l => l.FechaRegistro)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarPorFontaneroAsync(int fontaneroId)
    {
        var lecturas = await ConsultaBase()
            .Where(l => l.FontaneroId == fontaneroId && l.Estado != "Pendiente")
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> ListarPendientesPorFontaneroAsync(int fontaneroId)
    {
        var lecturas = await ConsultaBase()
            .Where(l => l.FontaneroId == fontaneroId && l.Estado == "Pendiente")
            .OrderBy(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorMedidorAsync(string numeroMedidor)
    {
        var normalizado = numeroMedidor.Trim().ToUpperInvariant();
        var lecturas = await ConsultaBase()
            .Where(l => l.NumeroMedidor.ToUpper() == normalizado && l.Estado != "Pendiente")
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<LecturaMedidorResponseDto>> HistorialPorAbonadoAsync(string numeroAbonado)
    {
        var normalizado = numeroAbonado.Trim().ToUpperInvariant();
        var lecturas = await ConsultaBase()
            .Where(l =>
                l.Estado != "Pendiente" &&
                (
                    (l.NumeroAbonado != null && l.NumeroAbonado.ToUpper() == normalizado) ||
                    l.NombreAbonado.ToUpper().Contains(normalizado)
                ))
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        return lecturas.Select(Mappers.ToLecturaResponse).ToList();
    }

    public async Task<IReadOnlyList<HistorialLecturaDto>> HistorialCambiosAsync(int lecturaId)
    {
        var items = await context.HistorialLecturas
            .Where(h => h.LecturaMedidorId == lecturaId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();

        return items.Select(Mappers.ToHistorialLecturaDto).ToList();
    }

    public async Task<ResumenLecturasMedidorDto> ResumenAdminAsync()
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hoy = DateTime.UtcNow.Date;

        var lecturas = await context.LecturasMedidor
            .Where(l => l.Estado != "Pendiente")
            .ToListAsync();

        return CrearResumen(lecturas, inicioMes, hoy);
    }

    public async Task<ResumenLecturasMedidorDto> ResumenFontaneroAsync(int fontaneroId)
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var hoy = DateTime.UtcNow.Date;

        var lecturas = await context.LecturasMedidor
            .Where(l => l.FontaneroId == fontaneroId)
            .ToListAsync();

        return CrearResumen(lecturas, inicioMes, hoy, incluirPendientes: true);
    }

    public async Task<ReporteLecturasMedidorDto> GenerarReporteAsync(string tipo)
    {
        var normalizado = tipo.Trim().ToLowerInvariant();
        var lecturas = await ConsultaBase()
            .Where(l => l.Estado != "Pendiente")
            .OrderByDescending(l => l.FechaLectura)
            .ToListAsync();

        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var filtradas = normalizado switch
        {
            "mes" => lecturas.Where(l => l.FechaLectura >= inicioMes).ToList(),
            "consumo-alto" => lecturas.Where(l => l.ConsumoAlto || Mappers.EsAlertaConsumoAlto(l)).ToList(),
            "inconsistencia" => lecturas.Where(l => l.Estado == "Con inconsistencia").ToList(),
            "fontanero" => lecturas.OrderBy(l => l.Fontanero.NombreUsuario).ToList(),
            _ => lecturas
        };

        return new ReporteLecturasMedidorDto
        {
            Tipo = normalizado,
            TotalRegistros = filtradas.Count,
            Registros = filtradas.Select(Mappers.ToLecturaResponse).ToList()
        };
    }

    public async Task<LecturaMedidorResponseDto> CrearAsync(CrearLecturaMedidorDto dto, int fontaneroId)
    {
        ValidarCamposObligatorios(dto.NombreAbonado, dto.NumeroMedidor);
        ValidarEstadoMedidor(dto.EstadoMedidor);

        var consumo = dto.LecturaActual - dto.LecturaAnterior;
        ValidarConsumoCero(consumo, dto.Observaciones);

        var consumoMesAnterior = await ObtenerConsumoMesAnteriorAsync(dto.NumeroMedidor);
        var consumoAlto = EsConsumoAlto(consumo, consumoMesAnterior);
        var estado = DeterminarEstado(consumo, consumoAlto);

        var lectura = new LecturaMedidor
        {
            NombreAbonado = dto.NombreAbonado.Trim(),
            NumeroAbonado = NormalizarOpcional(dto.NumeroAbonado),
            NumeroMedidor = dto.NumeroMedidor.Trim(),
            CedulaAbonado = NormalizarOpcional(dto.CedulaAbonado),
            Ubicacion = NormalizarOpcional(dto.Ubicacion),
            LecturaAnterior = dto.LecturaAnterior,
            LecturaActual = dto.LecturaActual,
            Consumo = consumo,
            ConsumoMesAnterior = consumoMesAnterior,
            ConsumoAlto = consumoAlto,
            FechaLectura = ParseFecha(dto.FechaLectura),
            HoraLectura = NormalizarOpcional(dto.HoraLectura),
            Observaciones = NormalizarOpcional(dto.Observaciones),
            MotivoVisita = NormalizarOpcional(dto.MotivoVisita),
            ResultadoInspeccion = NormalizarOpcional(dto.ResultadoInspeccion),
            EstadoMedidor = NormalizarOpcional(dto.EstadoMedidor),
            EvidenciaNombre = NormalizarOpcional(dto.EvidenciaNombre),
            EvidenciaBase64 = NormalizarOpcional(dto.EvidenciaBase64),
            Estado = estado,
            FontaneroId = fontaneroId
        };

        context.LecturasMedidor.Add(lectura);
        await context.SaveChangesAsync();

        await RegistrarHistorialAsync(lectura.Id, fontaneroId, null, "Registro creado", null, estado, dto.Observaciones);
        await context.Entry(lectura).Reference(l => l.Fontanero).LoadAsync();

        return Mappers.ToLecturaResponse(lectura);
    }

    public async Task<LecturaMedidorResponseDto> AsignarAsync(AsignarLecturaMedidorDto dto, int adminId)
    {
        ValidarCamposObligatorios(dto.NombreAbonado, dto.NumeroMedidor);

        var fontanero = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == dto.FontaneroId && u.Rol == Roles.Fontanero);
        if (fontanero is null)
        {
            throw new ArgumentException("El fontanero seleccionado no es valido.");
        }

        var lectura = new LecturaMedidor
        {
            NombreAbonado = dto.NombreAbonado.Trim(),
            NumeroAbonado = NormalizarOpcional(dto.NumeroAbonado),
            NumeroMedidor = dto.NumeroMedidor.Trim(),
            Ubicacion = NormalizarOpcional(dto.Ubicacion),
            LecturaAnterior = dto.LecturaAnterior,
            LecturaActual = dto.LecturaAnterior,
            Consumo = 0,
            FechaLectura = ParseFecha(dto.FechaLectura),
            Observaciones = NormalizarOpcional(dto.Observaciones),
            Estado = "Pendiente",
            FontaneroId = dto.FontaneroId
        };

        context.LecturasMedidor.Add(lectura);
        await context.SaveChangesAsync();

        await RegistrarHistorialAsync(lectura.Id, adminId, "Administradora", "Lectura asignada", null, "Pendiente", dto.Observaciones);
        await context.Entry(lectura).Reference(l => l.Fontanero).LoadAsync();

        return Mappers.ToLecturaResponse(lectura);
    }

    public async Task<LecturaMedidorResponseDto?> ActualizarAsync(
        int id,
        ActualizarLecturaMedidorDto dto,
        string rol,
        int usuarioId,
        string nombreUsuario)
    {
        var lectura = await ConsultaBase().FirstOrDefaultAsync(l => l.Id == id);
        if (lectura is null) return null;

        if (rol == Roles.Fontanero && lectura.FontaneroId != usuarioId)
        {
            throw new UnauthorizedAccessException("No puede modificar lecturas de otro fontanero.");
        }

        var estadoAnterior = lectura.Estado;

        if (dto.LecturaActual.HasValue)
        {
            if (rol == Roles.Fontanero && lectura.Estado == "Pendiente")
            {
                lectura.LecturaActual = dto.LecturaActual.Value;
                lectura.Consumo = lectura.LecturaActual - lectura.LecturaAnterior;
                ValidarConsumoCero(lectura.Consumo, dto.Observaciones ?? lectura.Observaciones);
                lectura.ConsumoMesAnterior ??= await ObtenerConsumoMesAnteriorAsync(lectura.NumeroMedidor, lectura.Id);
                lectura.ConsumoAlto = EsConsumoAlto(lectura.Consumo, lectura.ConsumoMesAnterior);
                lectura.Estado = DeterminarEstado(lectura.Consumo, lectura.ConsumoAlto);
            }
            else if (rol == Roles.Admin)
            {
                ValidarLecturasAdmin(lectura.LecturaAnterior, dto.LecturaActual.Value);
                lectura.LecturaActual = dto.LecturaActual.Value;
                lectura.Consumo = lectura.LecturaActual - lectura.LecturaAnterior;
                lectura.ConsumoAlto = EsConsumoAlto(lectura.Consumo, lectura.ConsumoMesAnterior);
                if (lectura.Consumo < 0)
                {
                    lectura.Estado = "Con inconsistencia";
                }
            }
            else
            {
                throw new ArgumentException("No tiene permiso para corregir la lectura actual.");
            }
        }

        if (dto.Observaciones is not null)
        {
            lectura.Observaciones = NormalizarOpcional(dto.Observaciones);
        }

        if (dto.ObservacionAdmin is not null)
        {
            if (rol != Roles.Admin)
            {
                throw new UnauthorizedAccessException("Solo la administradora puede agregar observaciones administrativas.");
            }

            lectura.ObservacionAdmin = NormalizarOpcional(dto.ObservacionAdmin);
        }

        if (dto.HoraLectura is not null)
        {
            lectura.HoraLectura = NormalizarOpcional(dto.HoraLectura);
        }

        if (dto.MotivoVisita is not null)
        {
            lectura.MotivoVisita = NormalizarOpcional(dto.MotivoVisita);
        }

        if (dto.ResultadoInspeccion is not null)
        {
            lectura.ResultadoInspeccion = NormalizarOpcional(dto.ResultadoInspeccion);
        }

        if (dto.EstadoMedidor is not null)
        {
            ValidarEstadoMedidor(dto.EstadoMedidor);
            lectura.EstadoMedidor = NormalizarOpcional(dto.EstadoMedidor);
        }

        if (dto.EvidenciaBase64 is not null)
        {
            lectura.EvidenciaNombre = NormalizarOpcional(dto.EvidenciaNombre);
            lectura.EvidenciaBase64 = NormalizarOpcional(dto.EvidenciaBase64);
        }

        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            ValidarCambioEstado(dto.Estado, rol, dto.ObservacionAdmin ?? lectura.ObservacionAdmin);

            if (dto.Estado == "Rechazada" && string.IsNullOrWhiteSpace(dto.ObservacionAdmin ?? lectura.ObservacionAdmin))
            {
                throw new ArgumentException("Debe indicar una observacion administrativa para rechazar la lectura.");
            }

            lectura.Estado = dto.Estado;

            if (rol == Roles.Admin && dto.Estado is "Revisada" or "Validada" or "Rechazada")
            {
                lectura.RevisadaPorAdminId = usuarioId;
            }
        }

        lectura.FechaActualizacion = DateTime.UtcNow;
        await context.SaveChangesAsync();

        if (estadoAnterior != lectura.Estado)
        {
            await RegistrarHistorialAsync(
                lectura.Id,
                usuarioId,
                nombreUsuario,
                "Cambio de estado",
                estadoAnterior,
                lectura.Estado,
                dto.ObservacionAdmin ?? dto.Observaciones);
        }

        return Mappers.ToLecturaResponse(lectura);
    }

    private IQueryable<LecturaMedidor> ConsultaBase() =>
        context.LecturasMedidor
            .Include(l => l.Fontanero)
            .Include(l => l.RevisadaPorAdmin);

    private static ResumenLecturasMedidorDto CrearResumen(
        IReadOnlyList<LecturaMedidor> lecturas,
        DateTime inicioMes,
        DateTime hoy,
        bool incluirPendientes = false)
    {
        var registradas = incluirPendientes
            ? lecturas.Where(l => l.Estado != "Pendiente").ToList()
            : lecturas;

        return new ResumenLecturasMedidorDto
        {
            TotalMes = registradas.Count(l => l.FechaLectura >= inicioMes),
            Pendientes = lecturas.Count(l => l.Estado == "Pendiente"),
            RegistradasHoy = registradas.Count(l => l.FechaRegistro.Date == hoy || l.FechaLectura.Date == hoy),
            Validadas = registradas.Count(l => l.Estado == "Validada"),
            ConInconsistencia = registradas.Count(l => l.Estado == "Con inconsistencia"),
            ConsumosAltos = registradas.Count(l => l.ConsumoAlto || Mappers.EsAlertaConsumoAlto(l))
        };
    }

    private static void ValidarCamposObligatorios(string nombreAbonado, string numeroMedidor)
    {
        if (string.IsNullOrWhiteSpace(nombreAbonado) || string.IsNullOrWhiteSpace(numeroMedidor))
        {
            throw new ArgumentException("Abonado y numero de medidor son obligatorios.");
        }
    }

    private static void ValidarEstadoMedidor(string? estadoMedidor)
    {
        if (!string.IsNullOrWhiteSpace(estadoMedidor) && !EstadosMedidorLectura.EsValido(estadoMedidor))
        {
            throw new ArgumentException("El estado del medidor no es valido.");
        }
    }

    private static void ValidarConsumoCero(decimal consumo, string? observaciones)
    {
        if (consumo == 0 && string.IsNullOrWhiteSpace(observaciones))
        {
            throw new ArgumentException("Debe registrar una observacion cuando el consumo es cero.");
        }
    }

    private static void ValidarLecturasAdmin(decimal anterior, decimal actual)
    {
        if (actual < anterior)
        {
            throw new ArgumentException("La lectura actual no puede ser menor que la lectura anterior.");
        }
    }

    private static void ValidarCambioEstado(string estado, string rol, string? observacionAdmin)
    {
        if (!EstadosLecturaMedidor.EsValido(estado))
        {
            throw new ArgumentException("El estado de lectura no es valido.");
        }

        if (rol == Roles.Fontanero && !EstadosLecturaMedidor.Fontanero.Contains(estado))
        {
            throw new ArgumentException("El fontanero no puede aplicar ese estado.");
        }
    }

    private static bool EsConsumoAlto(decimal consumo, decimal? consumoMesAnterior) =>
        consumo > LecturaMedidorConstantes.LimiteConsumoAltoM3 ||
        (consumoMesAnterior.HasValue && consumoMesAnterior.Value > 0 && consumo > consumoMesAnterior.Value * 2);

    private static string DeterminarEstado(decimal consumo, bool consumoAlto) =>
        consumo < 0 || consumoAlto ? "Con inconsistencia" : "Registrada";

    private async Task<decimal?> ObtenerConsumoMesAnteriorAsync(string numeroMedidor, int? excluirId = null)
    {
        var consulta = context.LecturasMedidor
            .Where(l => l.NumeroMedidor.ToUpper() == numeroMedidor.Trim().ToUpper() && l.Estado != "Pendiente");

        if (excluirId.HasValue)
        {
            consulta = consulta.Where(l => l.Id != excluirId.Value);
        }

        var ultima = await consulta
            .OrderByDescending(l => l.FechaLectura)
            .FirstOrDefaultAsync();

        return ultima?.Consumo;
    }

    private async Task RegistrarHistorialAsync(
        int lecturaId,
        int? usuarioId,
        string? usuarioNombre,
        string accion,
        string? estadoAnterior,
        string? estadoNuevo,
        string? observacion)
    {
        context.HistorialLecturas.Add(new HistorialLectura
        {
            LecturaMedidorId = lecturaId,
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            Accion = accion,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            Observacion = NormalizarOpcional(observacion)
        });

        await context.SaveChangesAsync();
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static DateTime ParseFecha(string fecha)
    {
        if (DateTime.TryParse(fecha, out var resultado))
        {
            return DateTime.SpecifyKind(resultado, DateTimeKind.Utc);
        }

        throw new ArgumentException("La fecha de lectura no es valida.");
    }
}
