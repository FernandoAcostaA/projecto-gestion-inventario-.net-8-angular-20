using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetAllAsync();
        Task<Categoria?> GetByIdAsync(int id);
        Task AddAsync(Categoria entity);
        void Update(Categoria entity);
        void Delete(Categoria entity);
        Task<bool> SaveChangesAsync();
    }
}
