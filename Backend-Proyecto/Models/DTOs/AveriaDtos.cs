using System.ComponentModel.DataAnnotations;

namespace Backend_Proyecto.Models.DTOs;

public class CrearAveriaDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    [MaxLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingrese un correo electronico valido.")]
    [MaxLength(150)]
    public string? Correo { get; set; }

    [Required(ErrorMessage = "La ubicacion de la averia es obligatoria.")]
    [MaxLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el tipo de averia.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion del problema es obligatoria.")]
    [MaxLength(2000)]
    public string Descripcion { get; set; } = string.Empty;

    public string? FotoNombre { get; set; }
    public string? FotoBase64 { get; set; }
}

public class AveriaResponseDto
{
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string MensajeEstado { get; set; } = string.Empty;
    public string? FontaneroAsignado { get; set; }
    public string? NotasAtencion { get; set; }
    public string? ObservacionesAdmin { get; set; }
    public string? DescripcionTrabajo { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? FechaUltimaActualizacion { get; set; }
    public FotoAveriaDto? Foto { get; set; }
    public FotoAveriaDto? EvidenciaTrabajo { get; set; }
}

public class CambiarEstadoAveriaDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}

public class AsignarFontaneroAveriaDto
{
    [Required(ErrorMessage = "El fontanero es obligatorio.")]
    public string Fontanero { get; set; } = string.Empty;
}

public class ActualizarPrioridadAveriaDto
{
    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public string Prioridad { get; set; } = string.Empty;
}

public class ObservacionesAdminAveriaDto
{
    [MaxLength(2000)]
    public string? ObservacionesAdmin { get; set; }
}

public class AtencionFontaneroAveriaDto
{
    public string? DescripcionTrabajo { get; set; }
    public string? MaterialesUtilizados { get; set; }
    public string? NotasAtencion { get; set; }
    public string? Estado { get; set; }
    public string? EvidenciaNombre { get; set; }
    public string? EvidenciaBase64 { get; set; }
}

public class AveriaHistorialDto
{
    public string Accion { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string? Usuario { get; set; }
    public string Fecha { get; set; } = string.Empty;
}

public class FotoAveriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string VistaPrevia { get; set; } = string.Empty;
}

public class RegistroAveriaResponseDto
{
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class FontaneroResumenDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
}
