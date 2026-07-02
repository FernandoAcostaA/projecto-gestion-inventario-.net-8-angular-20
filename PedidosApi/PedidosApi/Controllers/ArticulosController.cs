using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.DTOs;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ArticulosController : ControllerBase
    {
        private readonly IArticuloRepository _repo;
        private readonly AppDbContext _context;

        public ArticulosController(IArticuloRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticuloDto>>> GetAll()
        {
            var articulos = await _repo.GetAllAsync();

            var stocks = await _context.DetallesIngreso
                .GroupBy(di => di.IdArticulo)
                .Select(g => new { IdArticulo = g.Key, StockTotal = g.Sum(di => di.StockActual) })
                .ToDictionaryAsync(x => x.IdArticulo, x => x.StockTotal);

            var dtoList = articulos.Select(a =>
            {
                stocks.TryGetValue(a.Id, out var stock);
                return new ArticuloDto
                {
                    IdArticulo = a.Id,
                    Codigo = a.Codigo,
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    Imagen = a.Imagen,
                    IdCategoria = a.IdCategoria,
                    CategoriaNombre = a.Categoria?.Nombre,
                    IdPresentacion = a.IdPresentacion,
                    PresentacionNombre = a.Presentacion?.Nombre,
                    StockTotal = stock
                };
            });

            return Ok(dtoList);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ArticuloDto>> GetById(int id)
        {
            var a = await _repo.GetByIdAsync(id);
            if (a == null) return NotFound();

            var stock = await _context.DetallesIngreso
                .Where(di => di.IdArticulo == id)
                .SumAsync(di => di.StockActual);

            var dto = new ArticuloDto
            {
                IdArticulo = a.Id,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Imagen = a.Imagen,
                IdCategoria = a.IdCategoria,
                CategoriaNombre = a.Categoria?.Nombre,
                IdPresentacion = a.IdPresentacion,
                PresentacionNombre = a.Presentacion?.Nombre,
                StockTotal = stock
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ArticuloCreateDto dto)
        {
            var articulo = new Articulo
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Imagen = dto.Imagen,
                IdCategoria = dto.IdCategoria,
                IdPresentacion = dto.IdPresentacion
            };

            await _repo.AddAsync(articulo);
            await _repo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = articulo.Id }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ArticuloCreateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Codigo = dto.Codigo;
            existing.Nombre = dto.Nombre;
            existing.Descripcion = dto.Descripcion;
            existing.Imagen = dto.Imagen;
            existing.IdCategoria = dto.IdCategoria;
            existing.IdPresentacion = dto.IdPresentacion;

            _repo.Update(existing);
            await _repo.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var articulo = await _repo.GetByIdAsync(id);
            if (articulo == null) return NotFound();

            _repo.Delete(articulo);
            await _repo.SaveChangesAsync();

            return NoContent();
        }
    }
}
