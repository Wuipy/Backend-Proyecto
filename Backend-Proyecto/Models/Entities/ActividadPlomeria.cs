namespace Backend_Proyecto.Models.Entities;

public class ActividadPlomeria
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public string Prioridad { get; set; } = "Media";
    public string? NotasSeguimiento { get; set; }
    public string? NumeroAveriaVinculada { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
}
