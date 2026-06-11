using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Backend_Proyecto.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend_Proyecto.Data;
using System.Security.Claims;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/averias")]
public class AveriasController(IAveriaService averiaService, AppDbContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<AveriaResponseDto>>> Listar()
    {
        var averias = await averiaService.ListarAsync();
        return Ok(averias);
    }

    [HttpGet("gestion")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<AveriaResponseDto>>> ListarParaGestion()
    {
        return Ok(await averiaService.ListarParaGestionAsync());
    }

    [HttpGet("asignadas")]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<IReadOnlyList<AveriaResponseDto>>> ListarAsignadas()
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        return Ok(await averiaService.ListarAsignadasAsync(usuario.Id));
    }

    [HttpGet("{numeroSeguimiento}/historial")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<AveriaHistorialDto>>> ObtenerHistorial(string numeroSeguimiento)
    {
        var historial = await averiaService.ObtenerHistorialAsync(numeroSeguimiento);
        return Ok(historial);
    }

    [HttpGet("{numeroSeguimiento}")]
    [AllowAnonymous]
    public async Task<ActionResult<AveriaResponseDto>> ObtenerPorNumero(string numeroSeguimiento)
    {
        var averia = await averiaService.ObtenerPorNumeroAsync(numeroSeguimiento);
        if (averia is null)
        {
            return NotFound(new ApiErrorResponse { Message = $"No se encontro el reporte {numeroSeguimiento}." });
        }

        return Ok(averia);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RegistroAveriaResponseDto>> Crear([FromBody] CrearAveriaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(CrearErrorValidacion());

        var resultado = await averiaService.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorNumero), new { numeroSeguimiento = resultado.NumeroSeguimiento }, resultado);
    }

    [HttpPatch("{numeroSeguimiento}/estado")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AveriaResponseDto>> CambiarEstado(
        string numeroSeguimiento,
        [FromBody] CambiarEstadoAveriaDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        var resultado = await averiaService.ActualizarEstadoAsync(numeroSeguimiento, dto.Estado, usuario.NombreUsuario);
        if (resultado is null) return NotFound(new ApiErrorResponse { Message = "Reporte no encontrado." });

        return Ok(resultado);
    }

    [HttpPatch("{numeroSeguimiento}/asignar-fontanero")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AveriaResponseDto>> AsignarFontanero(
        string numeroSeguimiento,
        [FromBody] AsignarFontaneroAveriaDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        var fontanero = await context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == dto.Fontanero && u.Rol == Roles.Fontanero);

        if (fontanero is null)
        {
            return BadRequest(new ApiErrorResponse { Message = "Fontanero no encontrado." });
        }

        var resultado = await averiaService.AsignarFontaneroAsync(
            numeroSeguimiento,
            fontanero.Id,
            usuario.NombreUsuario,
            forzarAsignacion: true);

        if (resultado is null) return NotFound(new ApiErrorResponse { Message = "Reporte no encontrado." });

        return Ok(resultado);
    }

    [HttpPatch("{numeroSeguimiento}/prioridad")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AveriaResponseDto>> ActualizarPrioridad(
        string numeroSeguimiento,
        [FromBody] ActualizarPrioridadAveriaDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        var resultado = await averiaService.ActualizarPrioridadAsync(
            numeroSeguimiento,
            dto.Prioridad,
            usuario.NombreUsuario);

        if (resultado is null) return NotFound(new ApiErrorResponse { Message = "Reporte no encontrado." });

        return Ok(resultado);
    }

    [HttpPatch("{numeroSeguimiento}/observaciones-admin")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AveriaResponseDto>> ActualizarObservacionesAdmin(
        string numeroSeguimiento,
        [FromBody] ObservacionesAdminAveriaDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        var resultado = await averiaService.ActualizarObservacionesAdminAsync(
            numeroSeguimiento,
            dto.ObservacionesAdmin,
            usuario.NombreUsuario);

        if (resultado is null) return NotFound(new ApiErrorResponse { Message = "Reporte no encontrado." });

        return Ok(resultado);
    }

    [HttpPatch("{numeroSeguimiento}/atencion-fontanero")]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<AveriaResponseDto>> ActualizarAtencionFontanero(
        string numeroSeguimiento,
        [FromBody] AtencionFontaneroAveriaDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized(new ApiErrorResponse { Message = "Sesion no valida." });

        var reporte = await context.Averias
            .FirstOrDefaultAsync(a => a.NumeroSeguimiento.ToUpper() == numeroSeguimiento.Trim().ToUpper());

        if (reporte is null) return NotFound(new ApiErrorResponse { Message = "Reporte no encontrado." });

        if (reporte.FontaneroAsignadoId != usuario.Id)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
            {
                Message = "Solo puede atender averias asignadas a usted."
            });
        }

        var resultado = await averiaService.ActualizarAtencionFontaneroAsync(
            numeroSeguimiento,
            dto,
            usuario.NombreUsuario);

        return Ok(resultado);
    }

    private async Task<Models.Entities.Usuario?> ObtenerUsuarioActualAsync()
    {
        var nombreUsuario = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nombreUsuario)) return null;

        return await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    private ApiErrorResponse CrearErrorValidacion() =>
        new()
        {
            Message = "Datos de entrada invalidos.",
            Errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
        };
}
