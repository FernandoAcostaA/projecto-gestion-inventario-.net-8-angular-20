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
    public class IngresosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngresosController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/ingresos/reposicion-rapida
        [HttpPost("reposicion-rapida")]
        public async Task<IActionResult> ReposicionRapida([FromBody] ReposicionStockDto dto)
        {
            try
            {
                var articulo = await _context.Articulos.FindAsync(dto.IdArticulo);
                if (articulo == null)
                    return NotFound(new { error = "Artículo no encontrado." });

                var trabajadorId = await _context.Trabajadores
                    .Select(t => (int?)t.IdTrabajador)
                    .FirstOrDefaultAsync() ?? throw new InvalidOperationException("No hay trabajadores registrados.");

                var proveedorId = await _context.Proveedores
                    .Select(p => (int?)p.IdProveedor)
                    .FirstOrDefaultAsync() ?? 1;

                var ingreso = new Ingreso
                {
                    IdProveedor = proveedorId,
                    IdTrabajador = trabajadorId,
                    Fecha = DateTime.Now,
                    TipoComprobante = "REPO",
                    Serie = "R",
                    Correlativo = "0000001",
                    Igv = 0,
                    Estado = "REPOSICIÓN"
                };

                _context.Ingresos.Add(ingreso);
                await _context.SaveChangesAsync();

                var detalle = new DetalleIngreso
                {
                    IdIngreso = ingreso.IdIngreso,
                    IdArticulo = dto.IdArticulo,
                    PrecioCompra = dto.PrecioCompra,
                    PrecioVenta = dto.PrecioVenta,
                    StockInicial = dto.Cantidad,
                    StockActual = dto.Cantidad,
                    FechaProduccion = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddYears(1)
                };

                _context.DetallesIngreso.Add(detalle);
                await _context.SaveChangesAsync();

                return Ok(new { idIngreso = ingreso.IdIngreso, idDetalleIngreso = detalle.IdDetalleIngreso });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngresoDto>>> GetAll()
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.Proveedor)
                .Include(i => i.Trabajador)
                .ToListAsync();

            var dtoList = ingresos.Select(i => new IngresoDto
            {
                IdIngreso = i.IdIngreso,
                IdProveedor = i.IdProveedor,
                IdTrabajador = i.IdTrabajador,
                Fecha = i.Fecha,
                TipoComprobante = i.TipoComprobante,
                Serie = i.Serie,
                Correlativo = i.Correlativo,
                Igv = i.Igv,
                Estado = i.Estado,
                NombreProveedor = i.Proveedor?.RazonSocial,
                NombreTrabajador = i.Trabajador?.Nombre + " " + i.Trabajador?.Apellidos
            });

            return Ok(dtoList);
        }

        [HttpGet("completo/{id}")]
        public async Task<ActionResult<IngresoConDetallesDto>> ObtenerIngresoCompleto(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.DetallesIngreso)
                .FirstOrDefaultAsync(i => i.IdIngreso == id);

            if (ingreso == null)
                return NotFound();

            var ingresoDto = new IngresoConDetallesDto
            {
                IdProveedor = ingreso.IdProveedor,
                IdTrabajador = ingreso.IdTrabajador,
                Fecha = ingreso.Fecha,
                TipoComprobante = ingreso.TipoComprobante,
                Serie = ingreso.Serie,
                Correlativo = ingreso.Correlativo,
                Igv = ingreso.Igv,
                Estado = ingreso.Estado,
                Detalles = ingreso.DetallesIngreso.Select(d => new DetalleIngresoSimpleDto
                {
                    IdArticulo = d.IdArticulo,
                    PrecioCompra = d.PrecioCompra,
                    PrecioVenta = d.PrecioVenta,
                    StockInicial = d.StockInicial,
                    StockActual = d.StockActual,
                    FechaProduccion = d.FechaProduccion,
                    FechaVencimiento = d.FechaVencimiento
                }).ToList()
            };

            return Ok(ingresoDto);
        }


        [HttpPost]
        public async Task<IActionResult> Create(Ingreso ingreso)
        {
            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = ingreso.IdIngreso }, ingreso);
        }

        [HttpPost("completo")]
        public async Task<IActionResult> CrearIngresoCompleto([FromBody] IngresoConDetallesDto ingresoDto)
        {
            try
            {
                var ingreso = new Ingreso
                {
                    IdProveedor = ingresoDto.IdProveedor,
                    IdTrabajador = ingresoDto.IdTrabajador,
                    Fecha = ingresoDto.Fecha,
                    TipoComprobante = ingresoDto.TipoComprobante,
                    Serie = ingresoDto.Serie,
                    Correlativo = ingresoDto.Correlativo,
                    Igv = ingresoDto.Igv,
                    Estado = ingresoDto.Estado,
                    DetallesIngreso = ingresoDto.Detalles.Select(d => new DetalleIngreso
                    {
                        IdArticulo = d.IdArticulo,
                        PrecioCompra = d.PrecioCompra,
                        PrecioVenta = d.PrecioVenta,
                        StockInicial = d.StockInicial,
                        StockActual = d.StockActual,
                        FechaProduccion = d.FechaProduccion,
                        FechaVencimiento = d.FechaVencimiento
                    }).ToList()
                };

                _context.Ingresos.Add(ingreso);
                await _context.SaveChangesAsync();

                return Ok(new { idIngreso = ingreso.IdIngreso });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message,
                    stack = ex.InnerException?.StackTrace ?? ex.StackTrace
                });
            }
        }

        [HttpPut("completo/{id}")]
        public async Task<IActionResult> ActualizarIngresoCompleto(int id, [FromBody] IngresoConDetallesDto ingresoDto)
        {
            var ingresoExistente = await _context.Ingresos
                .Include(i => i.DetallesIngreso)
                .FirstOrDefaultAsync(i => i.IdIngreso == id);

            if (ingresoExistente == null)
                return NotFound();

            ingresoExistente.IdProveedor = ingresoDto.IdProveedor;
            ingresoExistente.IdTrabajador = ingresoDto.IdTrabajador;
            ingresoExistente.Fecha = ingresoDto.Fecha;
            ingresoExistente.TipoComprobante = ingresoDto.TipoComprobante;
            ingresoExistente.Serie = ingresoDto.Serie;
            ingresoExistente.Correlativo = ingresoDto.Correlativo;
            ingresoExistente.Igv = ingresoDto.Igv;
            ingresoExistente.Estado = ingresoDto.Estado;

            _context.DetallesIngreso.RemoveRange(ingresoExistente.DetallesIngreso);
            ingresoExistente.DetallesIngreso = ingresoDto.Detalles.Select(d => new DetalleIngreso
            {
                IdArticulo = d.IdArticulo,
                PrecioCompra = d.PrecioCompra,
                PrecioVenta = d.PrecioVenta,
                StockInicial = d.StockInicial,
                StockActual = d.StockActual,
                FechaProduccion = d.FechaProduccion,
                FechaVencimiento = d.FechaVencimiento
            }).ToList();

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.InnerException?.Message ?? ex.Message,
                    stack = ex.InnerException?.StackTrace ?? ex.StackTrace
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ingreso = await _context.Ingresos.FindAsync(id);
            if (ingreso == null)
                return NotFound();

            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
