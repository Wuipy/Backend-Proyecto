namespace Backend_Proyecto.Models.Entities;

public class ActividadFontanero
{
    public string Id { get; set; } = string.Empty;
    public int FontaneroId { get; set; }
    public Usuario Fontanero { get; set; } = null!;
    public DateTime FechaActividad { get; set; }
    public string? HoraInicio { get; set; }
    public string? HoraFin { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string? NumeroAveriaVinculada { get; set; }
    public int? LecturaMedidorId { get; set; }
    public LecturaMedidor? LecturaMedidor { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string EstadoValidacion { get; set; } = "Pendiente";
    public string? ObservacionValidacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

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
