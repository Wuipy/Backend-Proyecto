using System.ComponentModel.DataAnnotations;

namespace Backend_Proyecto.Models.DTOs;

public class ActividadPlomeriaDto
{
    [Required(ErrorMessage = "Seleccione el tipo de actividad.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [MaxLength(150)]
    public string Cliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicacion es obligatoria.")]
    [MaxLength(250)]
    public string Ubicacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatoria.")]
    [MaxLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public string Prioridad { get; set; } = "Media";

    [MaxLength(2000)]
    public string? NotasSeguimiento { get; set; }

    [MaxLength(20)]
    public string? NumeroAveriaVinculada { get; set; }
}

public class ActividadPlomeriaResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string? FechaActualizacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string? NotasSeguimiento { get; set; }
    public string? NumeroAveriaVinculada { get; set; }
}

public class CambiarEstadoActividadDto
{
    public string? Estado { get; set; }
}

public class NotasAveriaDto
{
    [MaxLength(2000)]
    public string? NotasAtencion { get; set; }
}
