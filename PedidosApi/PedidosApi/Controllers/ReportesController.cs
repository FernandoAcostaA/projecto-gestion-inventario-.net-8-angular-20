using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.DTOs.Reportes;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("resumen")]
        public async Task<ActionResult<ResumenDashboardDto>> GetResumen()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var totalClientes = await _context.Clientes.CountAsync();
            var totalProveedores = await _context.Proveedores.CountAsync();
            var totalArticulos = await _context.Articulos.CountAsync();

            var ventasHoy = await _context.Ventas.CountAsync(v => v.Fecha.Date == hoy);
            var ingresosHoy = await _context.Ingresos.CountAsync(i => i.Fecha.Date == hoy);

            var totalVentasHoy = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha.Date == hoy)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var totalIngresosHoy = await _context.DetallesIngreso
                .Where(di => di.Ingreso!.Fecha.Date == hoy)
                .SumAsync(di => di.StockInicial * di.PrecioCompra);

            var articulosStockBajo = await _context.DetallesIngreso
                .Where(di => di.StockActual <= 5)
                .Select(di => di.IdArticulo)
                .Distinct()
                .CountAsync();

            var ventasMes = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicioMes)
                .SumAsync(dv => dv.Cantidad * dv.PrecioVenta - dv.Descuento);

            var ingresosMes = await _context.DetallesIngreso
                .Where(di => di.Ingreso!.Fecha >= inicioMes)
                .SumAsync(di => di.StockInicial * di.PrecioCompra);

            return Ok(new ResumenDashboardDto
            {
                TotalClientes = totalClientes,
                TotalProveedores = totalProveedores,
                TotalArticulos = totalArticulos,
                VentasHoy = ventasHoy,
                TotalVentasHoy = totalVentasHoy,
                IngresosHoy = ingresosHoy,
                TotalIngresosHoy = totalIngresosHoy,
                ArticulosStockBajo = articulosStockBajo,
                VentasMesActual = ventasMes,
                IngresosMesActual = ingresosMes
            });
        }

        [HttpGet("ventas-por-periodo")]
        public async Task<ActionResult<List<VentasPorPeriodoDto>>> GetVentasPorPeriodo(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string agrupacion = "dia")
        {
            var inicio = fechaInicio ?? DateTime.Today.AddMonths(-1);
            var fin = fechaFin ?? DateTime.Today;

            var query = await _context.Ventas
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .Join(_context.DetallesVenta,
                    v => v.IdVenta,
                    dv => dv.IdVenta,
                    (v, dv) => new { v.Fecha, Total = dv.Cantidad * dv.PrecioVenta - dv.Descuento })
                .ToListAsync();

            List<VentasPorPeriodoDto> resultado;

            if (agrupacion == "mes")
            {
                resultado = query
                    .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                    .Select(g => new VentasPorPeriodoDto
                    {
                        Periodo = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Cantidad = g.Count(),
                        Total = g.Sum(x => x.Total)
                    })
                    .OrderBy(r => r.Periodo)
                    .ToList();
            }
            else if (agrupacion == "semana")
            {
                resultado = query
                    .GroupBy(x => new {
                        x.Fecha.Year,
                        Semana = System.Globalization.CultureInfo.InvariantCulture.Calendar
                            .GetWeekOfYear(x.Fecha, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                    })
                    .Select(g => new VentasPorPeriodoDto
                    {
                        Periodo = $"{g.Key.Year}-Sem{g.Key.Semana:D2}",
                        Cantidad = g.Count(),
                        Total = g.Sum(x => x.Total)
                    })
                    .OrderBy(r => r.Periodo)
                    .ToList();
            }
            else
            {
                resultado = query
                    .GroupBy(x => x.Fecha.Date)
                    .Select(g => new VentasPorPeriodoDto
                    {
                        Periodo = g.Key.ToString("yyyy-MM-dd"),
                        Cantidad = g.Count(),
                        Total = g.Sum(x => x.Total)
                    })
                    .OrderBy(r => r.Periodo)
                    .ToList();
            }

            return Ok(resultado);
        }

        [HttpGet("top-articulos")]
        public async Task<ActionResult<List<TopArticuloDto>>> GetTopArticulos(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int top = 10)
        {
            var inicio = fechaInicio ?? DateTime.Today.AddMonths(-3);
            var fin = fechaFin ?? DateTime.Today;

            var resultado = await _context.DetallesVenta
                .Where(dv => dv.Venta!.Fecha >= inicio
                          && dv.Venta!.Fecha <= fin)
                .GroupBy(dv => new { dv.DetalleIngreso!.IdArticulo, dv.DetalleIngreso!.Articulo!.Nombre, ArticuloCategoria = dv.DetalleIngreso.Articulo.Categoria.Nombre })
                .Select(g => new TopArticuloDto
                {
                    IdArticulo = g.Key.IdArticulo,
                    Nombre = g.Key.Nombre,
                    Categoria = g.Key.ArticuloCategoria,
                    TotalVendido = g.Sum(x => x.Cantidad),
                    TotalIngresos = g.Sum(x => x.Cantidad * x.PrecioVenta - x.Descuento)
                })
                .OrderByDescending(r => r.TotalVendido)
                .Take(top)
                .ToListAsync();

            return Ok(resultado);
        }

        [HttpGet("ingresos-por-proveedor")]
        public async Task<ActionResult<List<IngresosPorProveedorDto>>> GetIngresosPorProveedor(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            var inicio = fechaInicio ?? DateTime.Today.AddMonths(-3);
            var fin = fechaFin ?? DateTime.Today;

            var resultado = await _context.Ingresos
                .Where(i => i.Fecha >= inicio && i.Fecha <= fin)
                .GroupBy(i => new { i.IdProveedor, i.Proveedor!.RazonSocial })
                .Select(g => new IngresosPorProveedorDto
                {
                    IdProveedor = g.Key.IdProveedor,
                    Proveedor = g.Key.RazonSocial,
                    CantidadCompras = g.Count(),
                    TotalGastado = g.Sum(i => i.DetallesIngreso
                        .Sum(d => d.StockInicial * d.PrecioCompra))
                })
                .OrderByDescending(r => r.TotalGastado)
                .ToListAsync();

            return Ok(resultado);
        }
    }
}
