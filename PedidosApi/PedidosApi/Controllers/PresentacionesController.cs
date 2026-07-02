using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.DTOs;
using PedidosApi.Models;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PresentacionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PresentacionesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PresentacionDto>>> GetAll()
        {
            var presentaciones = await _context.Presentaciones
                .Select(p => new PresentacionDto
                {
                    IdPresentacion = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion
                })
                .ToListAsync();

            return Ok(presentaciones);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PresentacionDto dto)
        {
            var presentacion = new Presentacion
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Presentaciones.Add(presentacion);
            await _context.SaveChangesAsync();

            dto.IdPresentacion = presentacion.Id;
            return CreatedAtAction(nameof(GetAll), new { id = presentacion.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, PresentacionDto dto)
        {
            var presentacion = await _context.Presentaciones.FindAsync(id);
            if (presentacion == null) return NotFound();

            presentacion.Nombre = dto.Nombre;
            presentacion.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var presentacion = await _context.Presentaciones.FindAsync(id);
            if (presentacion == null) return NotFound();

            _context.Presentaciones.Remove(presentacion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
