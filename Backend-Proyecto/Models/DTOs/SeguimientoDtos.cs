namespace Backend_Proyecto.Models.DTOs;

public class SeguimientoResponseDto
{
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string MensajeEstado { get; set; } = string.Empty;
    public AveriaResponseDto? DetalleAveria { get; set; }
}
