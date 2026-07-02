using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IArticuloRepository
    {
        Task<IEnumerable<Articulo>> GetAllAsync();
        Task<Articulo?> GetByIdAsync(int id);
        Task AddAsync(Articulo articulo);
        void Update(Articulo articulo);
        void Delete(Articulo articulo);
        Task<bool> SaveChangesAsync();
    }
}
