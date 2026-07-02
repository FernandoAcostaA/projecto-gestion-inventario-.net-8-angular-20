using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IIngresoRepository
    {
        Task<IEnumerable<Ingreso>> GetAllAsync();
        Task<Ingreso?> GetByIdAsync(int id);
        Task AddAsync(Ingreso entity);
        void Update(Ingreso entity);
        void Delete(Ingreso entity);
        Task<bool> SaveChangesAsync();
    }
}
