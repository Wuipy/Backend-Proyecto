namespace Backend_Proyecto.Models.Entities;

public class Solicitud
{
    public int Id { get; set; }
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "En revision";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
