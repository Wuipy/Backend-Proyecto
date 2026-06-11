using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/solicitudes")]
public class SolicitudesController(ISolicitudService solicitudService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RegistroSolicitudResponseDto>> Crear([FromBody] CrearSolicitudDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CrearErrorValidacion());
        }

        var resultado = await solicitudService.CrearAsync(dto);
        return Ok(resultado);
    }

    private ApiErrorResponse CrearErrorValidacion() =>
        new()
        {
            Message = "Datos de entrada invalidos.",
            Errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                )
        };
}

[ApiController]
[Route("api/seguimiento")]
public class SeguimientoController(IConsultaSeguimientoService consultaService) : ControllerBase
{
    [HttpGet("{numeroSeguimiento}")]
    [AllowAnonymous]
    public async Task<ActionResult<SeguimientoResponseDto>> Consultar(string numeroSeguimiento)
    {
        if (string.IsNullOrWhiteSpace(numeroSeguimiento))
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Ingrese un numero de seguimiento para realizar la consulta."
            });
        }

        var resultado = await consultaService.ConsultarAsync(numeroSeguimiento);

        if (resultado is null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = $"No se encontro un tramite con el numero {numeroSeguimiento.Trim().ToUpper()}."
            });
        }

        return Ok(resultado);
    }
}
