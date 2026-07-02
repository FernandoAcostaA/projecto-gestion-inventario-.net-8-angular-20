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
    public class DetalleIngresoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleIngresoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/detalleingreso
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleIngresoDto>>> GetAll()
        {
            var detalles = await _context.DetallesIngreso
                .Include(d => d.Articulo)
                .Include(d => d.Ingreso)
                .ToListAsync();

            var dtoList = detalles.Select(d => new DetalleIngresoDto
            {
                IdDetalleIngreso = d.IdDetalleIngreso,
                IdArticulo = d.IdArticulo,
                PrecioCompra = d.PrecioCompra,
                PrecioVenta = d.PrecioVenta,
                StockInicial = d.StockInicial,
                StockActual = d.StockActual,
                FechaProduccion = d.FechaProduccion,
                FechaVencimiento = d.FechaVencimiento,
                ArticuloNombre = d.Articulo?.Nombre,
                IdIngreso = d.IdIngreso
            }).ToList();

            return Ok(dtoList);
        }

        // GET: api/detalleingreso/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleIngresoDto>> GetById(int id)
        {
            var d = await _context.DetallesIngreso
                .Include(d => d.Articulo)
                .Include(d => d.Ingreso)
                .FirstOrDefaultAsync(d => d.IdDetalleIngreso == id);

            if (d == null) return NotFound();

            var dto = new DetalleIngresoDto
            {
                IdDetalleIngreso = d.IdDetalleIngreso,
                IdArticulo = d.IdArticulo,
                PrecioCompra = d.PrecioCompra,
                PrecioVenta = d.PrecioVenta,
                StockInicial = d.StockInicial,
                StockActual = d.StockActual,
                FechaProduccion = d.FechaProduccion,
                FechaVencimiento = d.FechaVencimiento,
                ArticuloNombre = d.Articulo?.Nombre,
                IdIngreso = d.IdIngreso
            };

            return Ok(dto);
        }

        // POST: api/detalleingreso
        [HttpPost]
        public async Task<IActionResult> Create(DetalleIngreso detalle)
        {
            _context.DetallesIngreso.Add(detalle);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = detalle.IdDetalleIngreso }, detalle);
        }

        // PUT: api/detalleingreso/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, DetalleIngresoDto dto)
        {
            if (id != dto.IdDetalleIngreso)
                return BadRequest("El ID no coincide con el cuerpo de la solicitud.");

            var detalle = await _context.DetallesIngreso.FindAsync(id);
            if (detalle == null)
                return NotFound();

            detalle.IdArticulo = dto.IdArticulo;
            detalle.IdIngreso = dto.IdIngreso;
            detalle.PrecioCompra = dto.PrecioCompra;
            detalle.PrecioVenta = dto.PrecioVenta;
            detalle.StockInicial = dto.StockInicial;
            detalle.StockActual = dto.StockActual;
            detalle.FechaProduccion = dto.FechaProduccion;
            detalle.FechaVencimiento = dto.FechaVencimiento;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/detalleingreso/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _context.DetallesIngreso.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _context.DetallesIngreso.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
