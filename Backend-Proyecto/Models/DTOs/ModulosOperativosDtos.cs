using System.ComponentModel.DataAnnotations;



namespace Backend_Proyecto.Models.DTOs;



public class ActividadFontaneroDto

{

    [Required]

    public string FechaActividad { get; set; } = string.Empty;



    public string? HoraInicio { get; set; }

    public string? HoraFin { get; set; }



    [Required]

    public string Tipo { get; set; } = string.Empty;



    [Required]

    [MaxLength(2000)]

    public string Descripcion { get; set; } = string.Empty;



    [Required]

    [MaxLength(250)]

    public string Ubicacion { get; set; } = string.Empty;



    public string? NumeroAveriaVinculada { get; set; }

    public int? LecturaMedidorId { get; set; }

    public string? MaterialesUtilizados { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = "Pendiente";

}



public class ActividadFontaneroResponseDto

{

    public string Id { get; set; } = string.Empty;

    public string Fontanero { get; set; } = string.Empty;

    public string FechaActividad { get; set; } = string.Empty;

    public string FechaActividadIso { get; set; } = string.Empty;

    public string? HoraInicio { get; set; }

    public string? HoraFin { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Ubicacion { get; set; } = string.Empty;

    public string? NumeroAveriaVinculada { get; set; }

    public int? LecturaMedidorId { get; set; }

    public string? MaterialesUtilizados { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string EstadoValidacion { get; set; } = string.Empty;

    public string? ObservacionValidacion { get; set; }

    public string FechaCreacion { get; set; } = string.Empty;

    public string? FechaActualizacion { get; set; }

}



public class ValidarActividadFontaneroDto

{

    [Required]

    public string EstadoValidacion { get; set; } = string.Empty;



    public string? ObservacionValidacion { get; set; }

}



public class CrearLecturaMedidorDto

{

    [Required]

    [MaxLength(150)]

    public string NombreAbonado { get; set; } = string.Empty;



    [Required]

    [MaxLength(50)]

    public string NumeroMedidor { get; set; } = string.Empty;



    [MaxLength(30)]

    public string? CedulaAbonado { get; set; }



    [Required]

    public decimal LecturaAnterior { get; set; }



    [Required]

    public decimal LecturaActual { get; set; }



    [Required]

    public string FechaLectura { get; set; } = string.Empty;



    public string? Observaciones { get; set; }

}



public class ActualizarLecturaMedidorDto

{

    public decimal? LecturaActual { get; set; }

    public string? Observaciones { get; set; }

    public string? Estado { get; set; }

}



public class LecturaMedidorResponseDto

{

    public int Id { get; set; }

    public string NombreAbonado { get; set; } = string.Empty;

    public string NumeroMedidor { get; set; } = string.Empty;

    public string? CedulaAbonado { get; set; }

    public decimal LecturaAnterior { get; set; }

    public decimal LecturaActual { get; set; }

    public decimal Consumo { get; set; }

    public decimal? ConsumoMesAnterior { get; set; }

    public bool AlertaConsumoAlto { get; set; }

    public string FechaLectura { get; set; } = string.Empty;

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Fontanero { get; set; } = string.Empty;

    public string FechaRegistro { get; set; } = string.Empty;

}


