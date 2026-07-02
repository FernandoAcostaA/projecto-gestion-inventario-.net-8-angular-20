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
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetAll()
        {
            var proveedores = await _context.Proveedores.ToListAsync();

            var dtoList = proveedores.Select(p => new ProveedorDto
            {
                IdProveedor = p.IdProveedor,
                RazonSocial = p.RazonSocial,
                SectorComercial = p.SectorComercial,
                TipoDocumento = p.TipoDocumento,
                NumDocumento = p.NumDocumento,
                Direccion = p.Direccion, // ✅ Se añadió
                Telefono = p.Telefono,
                Email = p.Email
            });

            return Ok(dtoList);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = proveedor.IdProveedor }, proveedor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor) return BadRequest();
            _context.Proveedores.Update(proveedor);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
