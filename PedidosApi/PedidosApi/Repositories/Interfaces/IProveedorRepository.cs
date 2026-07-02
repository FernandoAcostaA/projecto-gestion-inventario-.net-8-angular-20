using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IProveedorRepository
    {
        Task<IEnumerable<Proveedor>> GetAllAsync();
        Task<Proveedor?> GetByIdAsync(int id);
        Task AddAsync(Proveedor entity);
        void Update(Proveedor entity);
        void Delete(Proveedor entity);
        Task<bool> SaveChangesAsync();
    }
}
