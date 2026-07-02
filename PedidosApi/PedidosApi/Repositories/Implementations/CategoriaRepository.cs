using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> GetAllAsync() =>
        await _context.Categorias.ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await _context.Categorias.FindAsync(id);

    public async Task AddAsync(Categoria entity) =>
        await _context.Categorias.AddAsync(entity);

    public void Update(Categoria entity) =>
        _context.Categorias.Update(entity);

    public void Delete(Categoria entity) =>
        _context.Categorias.Remove(entity);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
