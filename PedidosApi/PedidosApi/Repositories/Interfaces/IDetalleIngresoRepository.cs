using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IDetalleIngresoRepository
    {
        Task<IEnumerable<DetalleIngreso>> GetAllAsync();
        Task<DetalleIngreso?> GetByIdAsync(int id);
        Task AddAsync(DetalleIngreso entity);
        void Update(DetalleIngreso entity);
        void Delete(DetalleIngreso entity);
        Task<bool> SaveChangesAsync();
    }
}
