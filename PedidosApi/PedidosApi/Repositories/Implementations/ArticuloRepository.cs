using PedidosApi.Data;
using PedidosApi.Models;
using Microsoft.EntityFrameworkCore;
using PedidosApi.Repositories.Interfaces;


public class ArticuloRepository : IArticuloRepository
{
    private readonly AppDbContext _context;

    public ArticuloRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Articulo>> GetAllAsync() =>
    await _context.Articulos
        .Include(a => a.Categoria)
        .Include(a => a.Presentacion) // ← AÑADIR ESTA LÍNEA
        .ToListAsync();

    public async Task<Articulo?> GetByIdAsync(int id) =>
    await _context.Articulos
        .Include(a => a.Categoria)
        .Include(a => a.Presentacion) // ← AÑADIR ESTA LÍNEA
        .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(Articulo articulo) =>
        await _context.Articulos.AddAsync(articulo);

    public void Update(Articulo articulo) =>
        _context.Articulos.Update(articulo);

    public void Delete(Articulo articulo) =>
        _context.Articulos.Remove(articulo);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
