using PedidosApi.Data;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


public class DetalleIngresoRepository : IDetalleIngresoRepository
{
    private readonly AppDbContext _context;

    public DetalleIngresoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DetalleIngreso>> GetAllAsync() =>
        await _context.DetallesIngreso.ToListAsync(); // ← nombre correcto

    public async Task<DetalleIngreso?> GetByIdAsync(int id) =>
        await _context.DetallesIngreso.FindAsync(id);

    public async Task AddAsync(DetalleIngreso entity) =>
        await _context.DetallesIngreso.AddAsync(entity);

    public void Update(DetalleIngreso entity) =>
        _context.DetallesIngreso.Update(entity);

    public void Delete(DetalleIngreso entity) =>
        _context.DetallesIngreso.Remove(entity);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
