using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        Task<IEnumerable<Cliente>> GetAllAsync();
        Task<Cliente?> GetByIdAsync(int id);
        Task AddAsync(Cliente entity);
        void Update(Cliente entity);
        void Delete(Cliente entity);
        Task<bool> SaveChangesAsync();
    }
}
