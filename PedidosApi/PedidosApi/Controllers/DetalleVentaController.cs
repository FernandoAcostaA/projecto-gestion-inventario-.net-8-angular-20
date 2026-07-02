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
    public class DetalleVentaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleVentaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/detalleventa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleVentaDto>>> GetAll()
        {
            var detalles = await _context.DetallesVenta
                .Include(d => d.DetalleIngreso)
                    .ThenInclude(di => di.Articulo)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .ToListAsync();

            var result = detalles.Select(d => new DetalleVentaDto
            {
                IdDetalleVenta = d.IdDetalleVenta,
                IdVenta = d.IdVenta,
                IdDetalleIngreso = d.IdDetalleIngreso,
                Cantidad = d.Cantidad,
                PrecioVenta = d.PrecioVenta,
                Descuento = d.Descuento,
                ArticuloNombre = d.DetalleIngreso?.Articulo?.Nombre,
                CodigoVenta = $"{d.Venta?.Serie}-{d.Venta?.Correlativo}",
                ClienteNombre = d.Venta?.Cliente?.Nombre
            });

            return Ok(result);
        }

        // GET: api/detalleventa/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleVentaDto>> GetById(int id)
        {
            var d = await _context.DetallesVenta
                .Include(d => d.DetalleIngreso)
                    .ThenInclude(di => di.Articulo)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(d => d.IdDetalleVenta == id);

            if (d == null) return NotFound();

            var dto = new DetalleVentaDto
            {
                IdDetalleVenta = d.IdDetalleVenta,
                IdVenta = d.IdVenta,
                IdDetalleIngreso = d.IdDetalleIngreso,
                Cantidad = d.Cantidad,
                PrecioVenta = d.PrecioVenta,
                Descuento = d.Descuento,
                ArticuloNombre = d.DetalleIngreso?.Articulo?.Nombre,
                CodigoVenta = $"{d.Venta?.Serie}-{d.Venta?.Correlativo}",
                ClienteNombre = d.Venta?.Cliente?.Nombre
            };

            return Ok(dto);
        }

        // POST: api/detalleventa
        [HttpPost]
        public async Task<IActionResult> Create(DetalleVenta detalle)
        {
            _context.DetallesVenta.Add(detalle);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = detalle.IdDetalleVenta }, detalle);
        }

        // PUT: api/detalleventa/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DetalleVenta detalle)
        {
            if (id != detalle.IdDetalleVenta) return BadRequest();
            _context.DetallesVenta.Update(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/detalleventa/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _context.DetallesVenta.FindAsync(id);
            if (detalle == null) return NotFound();
            _context.DetallesVenta.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
