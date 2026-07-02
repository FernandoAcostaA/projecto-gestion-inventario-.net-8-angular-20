using Microsoft.EntityFrameworkCore;
using PedidosApi.Models;
using PedidosApi.Data;
using PedidosApi.Repositories.Interfaces;

namespace PedidosApi.Repositories.Implementations
{
    public class IngresoRepository : IIngresoRepository
    {
        private readonly AppDbContext _context;

        public IngresoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ingreso>> GetAllAsync() =>
            await _context.Ingresos.ToListAsync();

        public async Task<Ingreso?> GetByIdAsync(int id) =>
            await _context.Ingresos.FindAsync(id);

        public async Task AddAsync(Ingreso entity) =>
            await _context.Ingresos.AddAsync(entity);

        public void Update(Ingreso entity) =>
            _context.Ingresos.Update(entity);

        public void Delete(Ingreso entity) =>
            _context.Ingresos.Remove(entity);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}
