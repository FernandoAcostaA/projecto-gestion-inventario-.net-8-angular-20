using Microsoft.AspNetCore.Mvc;
using PedidosApi.DTOs;
using PedidosApi.Services;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var token = await _auth.RegisterAsync(dto);
            if (token == null) return BadRequest("Usuario ya existe");
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var token = await _auth.LoginAsync(dto);
            if (token == null) return Unauthorized("Credenciales inválidas");
            return Ok(new { token });
        }
    }
}
