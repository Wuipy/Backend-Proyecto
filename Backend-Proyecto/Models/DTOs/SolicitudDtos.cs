using System.ComponentModel.DataAnnotations;

namespace Backend_Proyecto.Models.DTOs;

public class CrearSolicitudDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cedula es obligatoria.")]
    [MaxLength(30)]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    [MaxLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electronico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electronico valido.")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La direccion es obligatoria.")]
    [MaxLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el tipo de solicitud.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agregue el detalle de la solicitud.")]
    [MaxLength(2000)]
    public string Descripcion { get; set; } = string.Empty;
}

public class RegistroSolicitudResponseDto
{
    public string NumeroSeguimiento { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}
