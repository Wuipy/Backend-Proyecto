using System.ComponentModel.DataAnnotations;

namespace Backend_Proyecto.Models.DTOs;

public class ActividadFontaneroDto
{
    [Required]
    public string FechaActividad { get; set; } = string.Empty;

    [Required]
    public string? HoraInicio { get; set; }

    public string? HoraFin { get; set; }

    [Required]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Ubicacion { get; set; } = string.Empty;

    public string? NumeroAveriaVinculada { get; set; }
    public int? LecturaMedidorId { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";

    // Visita de campo
    public string? AbonadoNumero { get; set; }
    public string? NombreAbonado { get; set; }
    public string? LugarVisita { get; set; }
    public string? MotivoVisita { get; set; }
    public decimal? LecturaAnteriorM3 { get; set; }
    public decimal? LecturaActualM3 { get; set; }
    public decimal? ConsumoRegistradoM3 { get; set; }
    public string? EstadoMedidor { get; set; }
    public string? DetectoFuga { get; set; }
    public string? ResultadoInspeccion { get; set; }
    public string? AccionRecomendada { get; set; }
    public string? FotoMedidorNombre { get; set; }
    public string? FotoMedidorBase64 { get; set; }

    // Toma de presion
    public string? AforoNumero { get; set; }
    public string? LugarPrueba { get; set; }
    public string? HoraPrueba { get; set; }
    public decimal? ResultadoPsi { get; set; }
    public string? DiametroTuberia { get; set; }
    public string? ObservacionesPresion { get; set; }

    // Control operativo
    public string? PruebaNumero { get; set; }
    public string? LugarCasa { get; set; }
    public string? HoraControl { get; set; }
    public string? CloroResidual { get; set; }
    public string? Turbiedad { get; set; }
    public decimal? Ph { get; set; }
    public string? Olor { get; set; }
    public string? Sabor { get; set; }
    public string? ObservacionesControlOperativo { get; set; }

    // Actividad general
    public string? DetalleTrabajoRealizado { get; set; }
    public string? ResultadoTrabajo { get; set; }
    public string? RequiereSeguimiento { get; set; }
    public string? PrioridadSeguimiento { get; set; }
    public string? FotoEvidenciaNombre { get; set; }
    public string? FotoEvidenciaBase64 { get; set; }
}

public class ActividadFontaneroResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Fontanero { get; set; } = string.Empty;
    public string FechaActividad { get; set; } = string.Empty;
    public string FechaActividadIso { get; set; } = string.Empty;
    public string? HoraInicio { get; set; }
    public string? HoraFin { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string? NumeroAveriaVinculada { get; set; }
    public int? LecturaMedidorId { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string EstadoValidacion { get; set; } = string.Empty;
    public string? ObservacionValidacion { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
    public string? FechaActualizacion { get; set; }

    public string? AbonadoNumero { get; set; }
    public string? NombreAbonado { get; set; }
    public string? LugarVisita { get; set; }
    public string? MotivoVisita { get; set; }
    public decimal? LecturaAnteriorM3 { get; set; }
    public decimal? LecturaActualM3 { get; set; }
    public decimal? ConsumoRegistradoM3 { get; set; }
    public string? EstadoMedidor { get; set; }
    public string? DetectoFuga { get; set; }
    public string? ResultadoInspeccion { get; set; }
    public string? AccionRecomendada { get; set; }
    public string? FotoMedidorNombre { get; set; }
    public string? FotoMedidorBase64 { get; set; }

    public string? AforoNumero { get; set; }
    public string? LugarPrueba { get; set; }
    public string? HoraPrueba { get; set; }
    public decimal? ResultadoPsi { get; set; }
    public string? DiametroTuberia { get; set; }
    public string? ObservacionesPresion { get; set; }

    public string? PruebaNumero { get; set; }
    public string? LugarCasa { get; set; }
    public string? HoraControl { get; set; }
    public string? CloroResidual { get; set; }
    public string? Turbiedad { get; set; }
    public decimal? Ph { get; set; }
    public string? Olor { get; set; }
    public string? Sabor { get; set; }
    public string? ObservacionesControlOperativo { get; set; }

    public string? DetalleTrabajoRealizado { get; set; }
    public string? ResultadoTrabajo { get; set; }
    public string? RequiereSeguimiento { get; set; }
    public string? PrioridadSeguimiento { get; set; }
    public string? FotoEvidenciaNombre { get; set; }
    public string? FotoEvidenciaBase64 { get; set; }
}

public class ValidarActividadFontaneroDto
{
    [Required]
    public string EstadoValidacion { get; set; } = string.Empty;

    public string? ObservacionValidacion { get; set; }
}

public class CrearLecturaMedidorDto
{
    [Required]
    [MaxLength(150)]
    public string NombreAbonado { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NumeroAbonado { get; set; }

    [Required]
    [MaxLength(50)]
    public string NumeroMedidor { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? CedulaAbonado { get; set; }

    [MaxLength(250)]
    public string? Ubicacion { get; set; }

    [Required]
    public decimal LecturaAnterior { get; set; }

    [Required]
    public decimal LecturaActual { get; set; }

    [Required]
    public string FechaLectura { get; set; } = string.Empty;

    public string? HoraLectura { get; set; }

    public string? Observaciones { get; set; }

    public string? EstadoMedidor { get; set; }

    public string? MotivoVisita { get; set; }

    public string? ResultadoInspeccion { get; set; }

    public string? EvidenciaNombre { get; set; }

    public string? EvidenciaBase64 { get; set; }
}

public class AsignarLecturaMedidorDto
{
    [Required]
    [MaxLength(150)]
    public string NombreAbonado { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NumeroAbonado { get; set; }

    [Required]
    [MaxLength(50)]
    public string NumeroMedidor { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Ubicacion { get; set; }

    [Required]
    public decimal LecturaAnterior { get; set; }

    [Required]
    public string FechaLectura { get; set; } = string.Empty;

    [Required]
    public int FontaneroId { get; set; }

    public string? Observaciones { get; set; }
}

public class ActualizarLecturaMedidorDto
{
    public decimal? LecturaActual { get; set; }
    public string? Observaciones { get; set; }
    public string? ObservacionAdmin { get; set; }
    public string? Estado { get; set; }
    public string? EstadoMedidor { get; set; }
    public string? HoraLectura { get; set; }
    public string? MotivoVisita { get; set; }
    public string? ResultadoInspeccion { get; set; }
    public string? EvidenciaNombre { get; set; }
    public string? EvidenciaBase64 { get; set; }
}

public class LecturaMedidorResponseDto
{
    public int Id { get; set; }
    public string NombreAbonado { get; set; } = string.Empty;
    public string? NumeroAbonado { get; set; }
    public string NumeroMedidor { get; set; } = string.Empty;
    public string? CedulaAbonado { get; set; }
    public string? Ubicacion { get; set; }
    public decimal LecturaAnterior { get; set; }
    public decimal LecturaActual { get; set; }
    public decimal Consumo { get; set; }
    public decimal? ConsumoMesAnterior { get; set; }
    public bool AlertaConsumoAlto { get; set; }
    public bool ConsumoAlto { get; set; }
    public string FechaLectura { get; set; } = string.Empty;
    public string? HoraLectura { get; set; }
    public string? Observaciones { get; set; }
    public string? ObservacionAdmin { get; set; }
    public string? MotivoVisita { get; set; }
    public string? ResultadoInspeccion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? EstadoMedidor { get; set; }
    public string? EvidenciaNombre { get; set; }
    public string? EvidenciaBase64 { get; set; }
    public string Fontanero { get; set; } = string.Empty;
    public string? RevisadaPorAdmin { get; set; }
    public string FechaRegistro { get; set; } = string.Empty;
    public string? FechaActualizacion { get; set; }
}

public class ResumenLecturasMedidorDto
{
    public int TotalMes { get; set; }
    public int Pendientes { get; set; }
    public int RegistradasHoy { get; set; }
    public int Validadas { get; set; }
    public int ConInconsistencia { get; set; }
    public int ConsumosAltos { get; set; }
}

public class HistorialLecturaDto
{
    public int Id { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public string? Observacion { get; set; }
    public string? Usuario { get; set; }
    public string Fecha { get; set; } = string.Empty;
}

public class ReporteLecturasMedidorDto
{
    public string Tipo { get; set; } = string.Empty;
    public int TotalRegistros { get; set; }
    public IReadOnlyList<LecturaMedidorResponseDto> Registros { get; set; } = [];
}
