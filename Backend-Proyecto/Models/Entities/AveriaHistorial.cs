namespace Backend_Proyecto.Models.Entities;

public class AveriaHistorial
{
    public int Id { get; set; }
    public int AveriaId { get; set; }
    public Averia Averia { get; set; } = null!;
    public string Accion { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
