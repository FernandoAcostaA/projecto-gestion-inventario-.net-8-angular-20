using Microsoft.EntityFrameworkCore;
using PedidosApi.Models;
using PedidosApi.Data;
using PedidosApi.Repositories.Interfaces;

namespace PedidosApi.Repositories.Implementations
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AppDbContext _context;

        public VentaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venta>> GetAllAsync() =>
            await _context.Ventas.ToListAsync();

        public async Task<Venta?> GetByIdAsync(int id) =>
            await _context.Ventas.FindAsync(id);

        public async Task AddAsync(Venta entity) =>
            await _context.Ventas.AddAsync(entity);

        public void Update(Venta entity) =>
            _context.Ventas.Update(entity);

        public void Delete(Venta entity) =>
            _context.Ventas.Remove(entity);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}
