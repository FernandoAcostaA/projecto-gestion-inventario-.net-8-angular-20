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
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Trabajador)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.DetalleIngreso)
                        .ThenInclude(di => di.Articulo)
                .ToListAsync();

            var result = ventas.Select(v => new VentaDto
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                IdTrabajador = v.IdTrabajador,
                Fecha = v.Fecha,
                TipoComprobante = v.TipoComprobante,
                Serie = v.Serie,
                Correlativo = v.Correlativo,
                Igv = v.Igv,
                ClienteNombre = v.Cliente?.Nombre,
                TrabajadorNombre = v.Trabajador?.Nombre,
                Detalles = v.DetallesVenta.Select(dv => new DetalleVentaDto
                {
                    IdDetalleVenta = dv.IdDetalleVenta,
                    IdVenta = dv.IdVenta,
                    IdDetalleIngreso = dv.IdDetalleIngreso,
                    Cantidad = dv.Cantidad,
                    PrecioVenta = dv.PrecioVenta,
                    Descuento = dv.Descuento,
                    ArticuloNombre = dv.DetalleIngreso?.Articulo?.Nombre
                }).ToList()
            });

            return Ok(result);
        }

        // GET: api/ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetById(int id)
        {
            var v = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Trabajador)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.DetalleIngreso)
                        .ThenInclude(di => di.Articulo)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (v == null) return NotFound();

            var dto = new VentaDto
            {
                IdVenta = v.IdVenta,
                IdCliente = v.IdCliente,
                IdTrabajador = v.IdTrabajador,
                Fecha = v.Fecha,
                TipoComprobante = v.TipoComprobante,
                Serie = v.Serie,
                Correlativo = v.Correlativo,
                Igv = v.Igv,
                ClienteNombre = v.Cliente?.Nombre,
                TrabajadorNombre = v.Trabajador?.Nombre,
                Detalles = v.DetallesVenta.Select(dv => new DetalleVentaDto
                {
                    IdDetalleVenta = dv.IdDetalleVenta,
                    IdVenta = dv.IdVenta,
                    IdDetalleIngreso = dv.IdDetalleIngreso,
                    Cantidad = dv.Cantidad,
                    PrecioVenta = dv.PrecioVenta,
                    Descuento = dv.Descuento,
                    ArticuloNombre = dv.DetalleIngreso?.Articulo?.Nombre
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/ventas/completo
        [HttpPost("completo")]
        public async Task<IActionResult> CrearConDetalles([FromBody] VentaConDetallesDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var trabajadorId = dto.IdTrabajador ?? await _context.Trabajadores
                    .Select(t => (int?)t.IdTrabajador)
                    .FirstOrDefaultAsync() ?? throw new InvalidOperationException("No hay trabajadores registrados.");

                var venta = new Venta
                {
                    IdCliente = dto.IdCliente,
                    IdTrabajador = trabajadorId,
                    Fecha = dto.Fecha,
                    TipoComprobante = dto.TipoComprobante,
                    Serie = dto.Serie,
                    Correlativo = dto.Correlativo,
                    Igv = dto.Igv
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync(); // genera IdVenta

                foreach (var d in dto.Detalles)
                {
                    var detalleIngreso = await _context.DetallesIngreso.FindAsync(d.IdDetalleIngreso);
                    if (detalleIngreso == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"El detalle de ingreso con ID {d.IdDetalleIngreso} no existe." });
                    }

                    if (detalleIngreso.StockActual < d.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"Stock insuficiente para el artículo. Disponible: {detalleIngreso.StockActual}, solicitado: {d.Cantidad}" });
                    }

                    detalleIngreso.StockActual -= d.Cantidad;

                    var detalle = new DetalleVenta
                    {
                        IdVenta = venta.IdVenta,
                        IdDetalleIngreso = d.IdDetalleIngreso,
                        Cantidad = d.Cantidad,
                        PrecioVenta = d.PrecioVenta,
                        Descuento = d.Descuento
                    };

                    _context.DetallesVenta.Add(detalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { venta.IdVenta });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<IActionResult> Create(Venta venta)
        {
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = venta.IdVenta }, venta);
        }

        // PUT: api/ventas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Venta venta)
        {
            if (id != venta.IdVenta) return BadRequest();
            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.DetallesVenta)
                    .FirstOrDefaultAsync(v => v.IdVenta == id);

                if (venta == null) return NotFound();

                // Restaura el stock antes de eliminar
                foreach (var detalle in venta.DetallesVenta)
                {
                    var ingreso = await _context.DetallesIngreso.FindAsync(detalle.IdDetalleIngreso);
                    if (ingreso != null)
                    {
                        ingreso.StockActual += detalle.Cantidad;
                    }
                }

                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }
        // PUT: api/ventas/completo/5
        [HttpPut("completo/{id}")]
        public async Task<IActionResult> ActualizarVentaCompleta(int id, [FromBody] VentaConDetallesDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ventaExistente = await _context.Ventas
                    .Include(v => v.DetallesVenta)
                    .FirstOrDefaultAsync(v => v.IdVenta == id);

                if (ventaExistente == null)
                    return NotFound();

                var trabajadorId = dto.IdTrabajador ?? await _context.Trabajadores
                    .Select(t => (int?)t.IdTrabajador)
                    .FirstOrDefaultAsync() ?? throw new InvalidOperationException("No hay trabajadores registrados.");

                // Actualiza los datos de la venta
                ventaExistente.IdCliente = dto.IdCliente;
                ventaExistente.IdTrabajador = trabajadorId;
                ventaExistente.Fecha = dto.Fecha;
                ventaExistente.TipoComprobante = dto.TipoComprobante;
                ventaExistente.Serie = dto.Serie;
                ventaExistente.Correlativo = dto.Correlativo;
                ventaExistente.Igv = dto.Igv;

                // Restaura el stock de los detalles antiguos
                foreach (var oldDetalle in ventaExistente.DetallesVenta)
                {
                    var oldIngreso = await _context.DetallesIngreso.FindAsync(oldDetalle.IdDetalleIngreso);
                    if (oldIngreso != null)
                    {
                        oldIngreso.StockActual += oldDetalle.Cantidad;
                    }
                }

                // Elimina los detalles existentes
                _context.DetallesVenta.RemoveRange(ventaExistente.DetallesVenta);

                // Agrega los nuevos detalles y descuenta stock
                foreach (var d in dto.Detalles)
                {
                    var detalleIngreso = await _context.DetallesIngreso.FindAsync(d.IdDetalleIngreso);
                    if (detalleIngreso == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"El detalle de ingreso con ID {d.IdDetalleIngreso} no existe." });
                    }

                    if (detalleIngreso.StockActual < d.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { error = $"Stock insuficiente para el artículo. Disponible: {detalleIngreso.StockActual}, solicitado: {d.Cantidad}" });
                    }

                    detalleIngreso.StockActual -= d.Cantidad;

                    var nuevoDetalle = new DetalleVenta
                    {
                        IdVenta = ventaExistente.IdVenta,
                        IdDetalleIngreso = d.IdDetalleIngreso,
                        Cantidad = d.Cantidad,
                        PrecioVenta = d.PrecioVenta,
                        Descuento = d.Descuento
                    };

                    _context.DetallesVenta.Add(nuevoDetalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
