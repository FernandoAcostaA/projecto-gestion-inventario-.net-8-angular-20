using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IDetalleVentaRepository
    {
        Task<IEnumerable<DetalleVenta>> GetAllAsync();
        Task<DetalleVenta?> GetByIdAsync(int id);
        Task AddAsync(DetalleVenta entity);
        void Update(DetalleVenta entity);
        void Delete(DetalleVenta entity);
        Task<bool> SaveChangesAsync();
    }
}
