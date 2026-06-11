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
}
