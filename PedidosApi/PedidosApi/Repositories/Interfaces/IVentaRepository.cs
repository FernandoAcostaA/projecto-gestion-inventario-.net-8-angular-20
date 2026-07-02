using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IVentaRepository
    {
        Task<IEnumerable<Venta>> GetAllAsync();
        Task<Venta?> GetByIdAsync(int id);
        Task AddAsync(Venta entity);
        void Update(Venta entity);
        void Delete(Venta entity);
        Task<bool> SaveChangesAsync();
    }
}
