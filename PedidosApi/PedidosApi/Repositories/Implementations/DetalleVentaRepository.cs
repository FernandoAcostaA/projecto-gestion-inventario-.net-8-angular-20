using PedidosApi.Data;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


public class DetalleVentaRepository : IDetalleVentaRepository
{
    private readonly AppDbContext _context;

    public DetalleVentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DetalleVenta>> GetAllAsync() =>
        await _context.DetallesVenta.ToListAsync(); // ← nombre correcto

    public async Task<DetalleVenta?> GetByIdAsync(int id) =>
        await _context.DetallesVenta.FindAsync(id);

    public async Task AddAsync(DetalleVenta entity) =>
        await _context.DetallesVenta.AddAsync(entity);

    public void Update(DetalleVenta entity) =>
        _context.DetallesVenta.Update(entity);

    public void Delete(DetalleVenta entity) =>
        _context.DetallesVenta.Remove(entity);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
