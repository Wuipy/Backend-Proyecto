using Backend_Proyecto.Models.DTOs;
using Backend_Proyecto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Proyecto.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CrearErrorValidacion());
        }

        var resultado = await authService.LoginAsync(dto);

        if (resultado is null)
        {
            return Unauthorized(new ApiErrorResponse
            {
                Message = "Usuario o contrasena incorrectos."
            });
        }

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
