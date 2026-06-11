using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/comunicados")]
public class ComunicadosController(IContenidoService contenidoService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ComunicadoDto>>> Listar()
    {
        var comunicados = await contenidoService.ListarComunicadosAsync();
        return Ok(comunicados);
    }
}

[ApiController]
[Route("api/proyectos")]
public class ProyectosController(IContenidoService contenidoService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ProyectoDto>>> Listar()
    {
        var proyectos = await contenidoService.ListarProyectosAsync();
        return Ok(proyectos);
    }
}

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() =>
        Ok(new { status = "ok", servicio = "SIGASJ API", fecha = DateTime.UtcNow });
}
