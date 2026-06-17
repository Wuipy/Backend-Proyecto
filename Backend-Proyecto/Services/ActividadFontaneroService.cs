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

        AplicarCamposEspecificos(actividad, dto);

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

        LimpiarCamposEspecificos(actividad);
        AplicarCamposEspecificos(actividad, dto);

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
        if (string.IsNullOrWhiteSpace(dto.Tipo))
        {
            throw new ArgumentException("Debe seleccionar un tipo de actividad.");
        }

        if (!DbSeeder.EsTipoActividadFontaneroValido(dto.Tipo))
        {
            throw new ArgumentException("El tipo de actividad no es valido.");
        }

        if (!DbSeeder.EsEstadoActividadFontaneroValido(dto.Estado))
        {
            throw new ArgumentException("El estado de actividad no es valido.");
        }

        if (string.IsNullOrWhiteSpace(dto.HoraInicio))
        {
            throw new ArgumentException("La hora de inicio es obligatoria.");
        }

        if (!string.IsNullOrWhiteSpace(dto.HoraFin) &&
            string.Compare(dto.HoraFin, dto.HoraInicio, StringComparison.Ordinal) <= 0)
        {
            throw new ArgumentException("La hora fin debe ser mayor que la hora inicio.");
        }

        ValidarCamposEspecificos(dto);
    }

    private static void ValidarCamposEspecificos(ActividadFontaneroDto dto)
    {
        if (dto.Tipo == DbSeeder.TipoVisitaCampo)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreAbonado))
                throw new ArgumentException("El nombre del abonado es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.LugarVisita))
                throw new ArgumentException("El lugar de la visita es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.MotivoVisita))
                throw new ArgumentException("El motivo de visita es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.EstadoMedidor))
                throw new ArgumentException("El estado del medidor es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.DetectoFuga))
                throw new ArgumentException("Debe indicar si se detecto fuga.");
            if (string.IsNullOrWhiteSpace(dto.ResultadoInspeccion))
                throw new ArgumentException("El resultado de la inspeccion es obligatorio.");

            if (dto.LecturaAnteriorM3.HasValue && dto.LecturaActualM3.HasValue &&
                dto.LecturaActualM3.Value < dto.LecturaAnteriorM3.Value)
            {
                throw new ArgumentException("La lectura actual no puede ser menor que la lectura anterior.");
            }

            return;
        }

        if (dto.Tipo == DbSeeder.TipoTomaPresion)
        {
            if (string.IsNullOrWhiteSpace(dto.AforoNumero))
                throw new ArgumentException("El aforo N° es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.LugarPrueba))
                throw new ArgumentException("El lugar de la prueba es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.HoraPrueba))
                throw new ArgumentException("La hora de la prueba es obligatoria.");
            if (!dto.ResultadoPsi.HasValue)
                throw new ArgumentException("El resultado en PSI es obligatorio.");
            if (dto.ResultadoPsi.Value < 0)
                throw new ArgumentException("El resultado en PSI no puede ser negativo.");
            if (string.IsNullOrWhiteSpace(dto.DiametroTuberia))
                throw new ArgumentException("El diametro de tuberia es obligatorio.");
            return;
        }

        if (dto.Tipo == DbSeeder.TipoControlOperativo)
        {
            if (string.IsNullOrWhiteSpace(dto.PruebaNumero))
                throw new ArgumentException("La prueba N° es obligatoria.");
            if (string.IsNullOrWhiteSpace(dto.LugarCasa))
                throw new ArgumentException("El lugar o casa es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.HoraControl))
                throw new ArgumentException("La hora del control es obligatoria.");
            if (string.IsNullOrWhiteSpace(dto.CloroResidual))
                throw new ArgumentException("El cloro residual es obligatorio.");
            if (dto.Ph.HasValue && (dto.Ph.Value < 0 || dto.Ph.Value > 14))
                throw new ArgumentException("El pH debe estar entre 0 y 14.");
            return;
        }

        if (dto.Tipo == DbSeeder.TipoActividadGeneral &&
            string.IsNullOrWhiteSpace(dto.DetalleTrabajoRealizado))
        {
            throw new ArgumentException("El detalle del trabajo realizado es obligatorio.");
        }
    }

    private static void LimpiarCamposEspecificos(ActividadFontanero actividad)
    {
        actividad.AbonadoNumero = null;
        actividad.NombreAbonado = null;
        actividad.LugarVisita = null;
        actividad.MotivoVisita = null;
        actividad.LecturaAnteriorM3 = null;
        actividad.LecturaActualM3 = null;
        actividad.ConsumoRegistradoM3 = null;
        actividad.EstadoMedidor = null;
        actividad.DetectoFuga = null;
        actividad.ResultadoInspeccion = null;
        actividad.AccionRecomendada = null;
        actividad.FotoMedidorNombre = null;
        actividad.FotoMedidorBase64 = null;
        actividad.AforoNumero = null;
        actividad.LugarPrueba = null;
        actividad.HoraPrueba = null;
        actividad.ResultadoPsi = null;
        actividad.DiametroTuberia = null;
        actividad.ObservacionesPresion = null;
        actividad.PruebaNumero = null;
        actividad.LugarCasa = null;
        actividad.HoraControl = null;
        actividad.CloroResidual = null;
        actividad.Turbiedad = null;
        actividad.Ph = null;
        actividad.Olor = null;
        actividad.Sabor = null;
        actividad.ObservacionesControlOperativo = null;
        actividad.DetalleTrabajoRealizado = null;
        actividad.ResultadoTrabajo = null;
        actividad.RequiereSeguimiento = null;
        actividad.PrioridadSeguimiento = null;
        actividad.FotoEvidenciaNombre = null;
        actividad.FotoEvidenciaBase64 = null;
    }

    private static void AplicarCamposEspecificos(ActividadFontanero actividad, ActividadFontaneroDto dto)
    {
        if (dto.Tipo == DbSeeder.TipoVisitaCampo)
        {
            actividad.AbonadoNumero = dto.AbonadoNumero?.Trim();
            actividad.NombreAbonado = dto.NombreAbonado?.Trim();
            actividad.LugarVisita = dto.LugarVisita?.Trim();
            actividad.MotivoVisita = dto.MotivoVisita?.Trim();
            actividad.LecturaAnteriorM3 = dto.LecturaAnteriorM3;
            actividad.LecturaActualM3 = dto.LecturaActualM3;
            actividad.ConsumoRegistradoM3 = dto.LecturaAnteriorM3.HasValue && dto.LecturaActualM3.HasValue
                ? dto.LecturaActualM3.Value - dto.LecturaAnteriorM3.Value
                : dto.ConsumoRegistradoM3;
            actividad.EstadoMedidor = dto.EstadoMedidor?.Trim();
            actividad.DetectoFuga = dto.DetectoFuga?.Trim();
            actividad.ResultadoInspeccion = dto.ResultadoInspeccion?.Trim();
            actividad.AccionRecomendada = dto.AccionRecomendada?.Trim();
            actividad.FotoMedidorNombre = dto.FotoMedidorNombre?.Trim();
            actividad.FotoMedidorBase64 = dto.FotoMedidorBase64;
            return;
        }

        if (dto.Tipo == DbSeeder.TipoTomaPresion)
        {
            actividad.AforoNumero = dto.AforoNumero?.Trim();
            actividad.LugarPrueba = dto.LugarPrueba?.Trim();
            actividad.HoraPrueba = dto.HoraPrueba?.Trim();
            actividad.ResultadoPsi = dto.ResultadoPsi;
            actividad.DiametroTuberia = dto.DiametroTuberia?.Trim();
            actividad.ObservacionesPresion = dto.ObservacionesPresion?.Trim();
            return;
        }

        if (dto.Tipo == DbSeeder.TipoControlOperativo)
        {
            actividad.PruebaNumero = dto.PruebaNumero?.Trim();
            actividad.LugarCasa = dto.LugarCasa?.Trim();
            actividad.HoraControl = dto.HoraControl?.Trim();
            actividad.CloroResidual = dto.CloroResidual?.Trim();
            actividad.Turbiedad = dto.Turbiedad?.Trim();
            actividad.Ph = dto.Ph;
            actividad.Olor = dto.Olor?.Trim();
            actividad.Sabor = dto.Sabor?.Trim();
            actividad.ObservacionesControlOperativo = dto.ObservacionesControlOperativo?.Trim();
            return;
        }

        if (dto.Tipo == DbSeeder.TipoActividadGeneral)
        {
            actividad.DetalleTrabajoRealizado = dto.DetalleTrabajoRealizado?.Trim();
            actividad.ResultadoTrabajo = dto.ResultadoTrabajo?.Trim();
            actividad.RequiereSeguimiento = dto.RequiereSeguimiento?.Trim();
            actividad.PrioridadSeguimiento = dto.RequiereSeguimiento == "Si"
                ? dto.PrioridadSeguimiento?.Trim()
                : null;
            actividad.FotoEvidenciaNombre = dto.FotoEvidenciaNombre?.Trim();
            actividad.FotoEvidenciaBase64 = dto.FotoEvidenciaBase64;
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
