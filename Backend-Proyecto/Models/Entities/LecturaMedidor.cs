namespace Backend_Proyecto.Models.Entities;

public class LecturaMedidor
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
    public bool ConsumoAlto { get; set; }
    public DateTime FechaLectura { get; set; }
    public string? HoraLectura { get; set; }
    public string? Observaciones { get; set; }
    public string? ObservacionAdmin { get; set; }
    public string? MotivoVisita { get; set; }
    public string? ResultadoInspeccion { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? EstadoMedidor { get; set; }
    public string? EvidenciaNombre { get; set; }
    public string? EvidenciaBase64 { get; set; }
    public int FontaneroId { get; set; }
    public Usuario Fontanero { get; set; } = null!;
    public int? RevisadaPorAdminId { get; set; }
    public Usuario? RevisadaPorAdmin { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
}
