using PedidosApi.Models;

namespace PedidosApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task<bool> SaveChangesAsync();
        void Delete(User user); // ✅ nuevo método
    }
}
