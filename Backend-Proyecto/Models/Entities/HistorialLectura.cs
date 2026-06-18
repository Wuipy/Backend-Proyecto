namespace Backend_Proyecto.Models.Entities;

public class HistorialLectura
{
    public int Id { get; set; }
    public int LecturaMedidorId { get; set; }
    public LecturaMedidor LecturaMedidor { get; set; } = null!;
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public string? Observacion { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
