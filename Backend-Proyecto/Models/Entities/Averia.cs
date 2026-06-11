namespace Backend_Proyecto.Models.Entities;

public class Averia
{
    public int Id { get; set; }
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public string Prioridad { get; set; } = "Media";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? FotoNombre { get; set; }
    public string? FotoBase64 { get; set; }
    public int? FontaneroAsignadoId { get; set; }
    public Usuario? FontaneroAsignado { get; set; }
    public string? NotasAtencion { get; set; }
    public string? ObservacionesAdmin { get; set; }
    public string? DescripcionTrabajo { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? EvidenciaTrabajoNombre { get; set; }
    public string? EvidenciaTrabajoBase64 { get; set; }
    public DateTime? FechaUltimaActualizacion { get; set; }
}
