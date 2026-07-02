using PedidosApi.DTOs;
using PedidosApi.Models;

namespace PedidosApi.Services
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(UserRegisterDto dto);
        Task<string?> LoginAsync(UserLoginDto dto);
    }
}
