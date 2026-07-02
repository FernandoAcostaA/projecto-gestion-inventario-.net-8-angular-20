using Microsoft.EntityFrameworkCore;
using PedidosApi.Models;
using PedidosApi.Data;
using PedidosApi.Repositories.Interfaces;

namespace PedidosApi.Repositories.Implementations
{
    public class TrabajadorRepository : ITrabajadorRepository
    {
        private readonly AppDbContext _context;

        public TrabajadorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trabajador>> GetAllAsync() =>
            await _context.Trabajadores.ToListAsync();

        public async Task<Trabajador?> GetByIdAsync(int id) =>
            await _context.Trabajadores.FindAsync(id);

        public async Task AddAsync(Trabajador entity) =>
            await _context.Trabajadores.AddAsync(entity);

        public void Update(Trabajador entity) =>
            _context.Trabajadores.Update(entity);

        public void Delete(Trabajador entity) =>
            _context.Trabajadores.Remove(entity);

        public async Task<bool> SaveChangesAsync() =>
            await _context.SaveChangesAsync() > 0;
    }
}
