using Backend_Proyecto.Data;
using Backend_Proyecto.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Services;

public interface IContenidoService
{
    Task<IReadOnlyList<ComunicadoDto>> ListarComunicadosAsync();
    Task<IReadOnlyList<ProyectoDto>> ListarProyectosAsync();
}

public class ContenidoService(AppDbContext context) : IContenidoService
{
    public async Task<IReadOnlyList<ComunicadoDto>> ListarComunicadosAsync()
    {
        return await context.Comunicados
            .OrderByDescending(c => c.Id)
            .Select(c => new ComunicadoDto
            {
                Fecha = c.Fecha,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProyectoDto>> ListarProyectosAsync()
    {
        return await context.Proyectos
            .OrderBy(p => p.Id)
            .Select(p => new ProyectoDto
            {
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Estado = p.Estado
            })
            .ToListAsync();
    }
}
