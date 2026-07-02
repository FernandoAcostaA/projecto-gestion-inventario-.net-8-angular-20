using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;

public class ProveedorRepository : IProveedorRepository
{
    private readonly AppDbContext _context;

    public ProveedorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Proveedor>> GetAllAsync() =>
        await _context.Proveedores.ToListAsync();

    public async Task<Proveedor?> GetByIdAsync(int id) =>
        await _context.Proveedores.FindAsync(id);

    public async Task AddAsync(Proveedor entity) =>
        await _context.Proveedores.AddAsync(entity);

    public void Update(Proveedor entity) =>
        _context.Proveedores.Update(entity);

    public void Delete(Proveedor entity) =>
        _context.Proveedores.Remove(entity);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
