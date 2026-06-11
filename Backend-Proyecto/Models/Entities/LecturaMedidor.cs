namespace Backend_Proyecto.Models.Entities;

public class LecturaMedidor
{
    public int Id { get; set; }
    public string NombreAbonado { get; set; } = string.Empty;
    public string NumeroMedidor { get; set; } = string.Empty;
    public string? CedulaAbonado { get; set; }
    public decimal LecturaAnterior { get; set; }
    public decimal LecturaActual { get; set; }
    public decimal Consumo { get; set; }
    public decimal? ConsumoMesAnterior { get; set; }
    public DateTime FechaLectura { get; set; }
    public string? Observaciones { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public int FontaneroId { get; set; }
    public Usuario Fontanero { get; set; } = null!;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
