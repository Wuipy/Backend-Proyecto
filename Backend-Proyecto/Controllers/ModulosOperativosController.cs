using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Backend_Proyecto.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Roles.Admin)]
public class UsuariosController(AppDbContext context) : ControllerBase
{
    [HttpGet("fontaneros")]
    public async Task<ActionResult<IReadOnlyList<FontaneroResumenDto>>> ListarFontaneros()
    {
        var fontaneros = await context.Usuarios
            .Where(u => u.Rol == Roles.Fontanero)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new FontaneroResumenDto { Usuario = u.NombreUsuario })
            .ToListAsync();

        return Ok(fontaneros);
    }
}

[ApiController]
[Route("api/actividades-fontanero")]
public class ActividadesFontaneroController(
    IActividadFontaneroService actividadService,
    AppDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<ActividadFontaneroResponseDto>>> ListarTodas()
    {
        return Ok(await actividadService.ListarTodasAsync());
    }

    [HttpGet("mis-actividades")]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<IReadOnlyList<ActividadFontaneroResponseDto>>> ListarMisActividades()
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        return Ok(await actividadService.ListarPorFontaneroAsync(usuario.Id));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<ActividadFontaneroResponseDto>> Crear([FromBody] ActividadFontaneroDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(CrearErrorValidacion());

        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        var actividad = await actividadService.CrearAsync(dto, usuario.Id);
        return CreatedAtAction(nameof(ListarMisActividades), actividad);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<ActividadFontaneroResponseDto>> Actualizar(string id, [FromBody] ActividadFontaneroDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(CrearErrorValidacion());

        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        var actividad = await actividadService.ActualizarAsync(id, dto, usuario.Id);
        if (actividad is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return Ok(actividad);
    }

    [HttpPatch("{id}/validar")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ActividadFontaneroResponseDto>> Validar(
        string id,
        [FromBody] ValidarActividadFontaneroDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(CrearErrorValidacion());

        var actividad = await actividadService.ValidarAsync(id, dto);
        if (actividad is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Actividad no encontrada." });
        }

        return Ok(actividad);
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

[ApiController]
[Route("api/lecturas-medidor")]
public class LecturasMedidorController(
    ILecturaMedidorService lecturaService,
    AppDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<LecturaMedidorResponseDto>>> ListarTodas()
    {
        return Ok(await lecturaService.ListarTodasAsync());
    }

    [HttpGet("mis-lecturas")]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<IReadOnlyList<LecturaMedidorResponseDto>>> ListarMisLecturas()
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        return Ok(await lecturaService.ListarPorFontaneroAsync(usuario.Id));
    }

    [HttpGet("historial/{numeroMedidor}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<LecturaMedidorResponseDto>>> Historial(string numeroMedidor)
    {
        return Ok(await lecturaService.HistorialPorMedidorAsync(numeroMedidor));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Fontanero)]
    public async Task<ActionResult<LecturaMedidorResponseDto>> Crear([FromBody] CrearLecturaMedidorDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(CrearErrorValidacion());

        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        var lectura = await lecturaService.CrearAsync(dto, usuario.Id);
        return CreatedAtAction(nameof(ListarMisLecturas), lectura);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Fontanero}")]
    public async Task<ActionResult<LecturaMedidorResponseDto>> Actualizar(
        int id,
        [FromBody] ActualizarLecturaMedidorDto dto)
    {
        var usuario = await ObtenerUsuarioActualAsync();
        if (usuario is null) return Unauthorized();

        var lectura = await lecturaService.ActualizarAsync(id, dto, usuario.Rol, usuario.Id);
        if (lectura is null)
        {
            return NotFound(new ApiErrorResponse { Message = "Lectura no encontrada." });
        }

        return Ok(lectura);
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
