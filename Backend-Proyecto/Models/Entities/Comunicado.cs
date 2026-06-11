namespace Backend_Proyecto.Models.Entities;

public class Comunicado
{
    public int Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
