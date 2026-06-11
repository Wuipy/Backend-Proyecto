using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Backend_Proyecto.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/actividades-plomeria")]
[Authorize(Roles = Roles.Admin)]
public class ActividadesPlomeriaController(IActividadPlomeriaService actividadService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActividadPlomeriaResponseDto>>> Listar()
    {
        var actividades = await actividadService.ListarAsync();
        return Ok(actividades);
    }

    [HttpPost]
    public async Task<ActionResult<ActividadPlomeriaResponseDto>> Crear([FromBody] ActividadPlomeriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CrearErrorValidacion());
        }

        var actividad = await actividadService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = actividad.Id }, actividad);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActividadPlomeriaResponseDto>> ObtenerPorId(string id)
    {
        var actividades = await actividadService.ListarAsync();
        var actividad = actividades.FirstOrDefault(a => a.Id == id);

        if (actividad is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return Ok(actividad);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ActividadPlomeriaResponseDto>> Actualizar(string id, [FromBody] ActividadPlomeriaDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CrearErrorValidacion());
        }

        var actividad = await actividadService.ActualizarAsync(id, dto);

        if (actividad is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return Ok(actividad);
    }

    [HttpPatch("{id}/estado")]
    public async Task<ActionResult<ActividadPlomeriaResponseDto>> CambiarEstado(string id, [FromBody] CambiarEstadoActividadDto? dto)
    {
        var actividad = await actividadService.CambiarEstadoAsync(id, dto);

        if (actividad is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return Ok(actividad);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Eliminar(string id)
    {
        var eliminado = await actividadService.EliminarAsync(id);

        if (!eliminado)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return NoContent();
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
