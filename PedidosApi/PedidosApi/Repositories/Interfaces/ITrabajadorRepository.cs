using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface ITrabajadorRepository
    {
        Task<IEnumerable<Trabajador>> GetAllAsync();
        Task<Trabajador?> GetByIdAsync(int id);
        Task AddAsync(Trabajador entity);
        void Update(Trabajador entity);
        void Delete(Trabajador entity);
        Task<bool> SaveChangesAsync();
    }
}
