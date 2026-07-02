using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApi.DTOs;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrabajadoresController : ControllerBase
    {
        private readonly ITrabajadorRepository _repo;
        private readonly IUserRepository _userRepo;

        public TrabajadoresController(ITrabajadorRepository repo, IUserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrabajadorDto>>> GetAll()
        {
            var trabajadores = await _repo.GetAllAsync();

            var dtoList = trabajadores.Select(t => new TrabajadorDto
            {
                IdTrabajador = t.IdTrabajador,
                Nombre = t.Nombre,
                Apellidos = t.Apellidos,
                Sexo = t.Sexo,
                FechaNacimiento = t.FechaNacimiento,
                NumDocumento = t.NumDocumento,
                Direccion = t.Direccion,
                Telefono = t.Telefono,
                Email = t.Email,
                Acceso = t.Acceso,
                Usuario = t.Usuario,
                Password = "" // nunca enviar password
            });

            return Ok(dtoList);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Trabajador trabajador)
        {
            // Validar si el usuario ya existe en la tabla de autenticación
            if (!string.IsNullOrWhiteSpace(trabajador.Usuario) &&
                !string.IsNullOrWhiteSpace(trabajador.Password))
            {
                var existente = await _userRepo.GetByUsernameAsync(trabajador.Usuario);
                if (existente != null)
                    return BadRequest("El nombre de usuario ya está registrado.");
            }

            await _repo.AddAsync(trabajador);
            await _repo.SaveChangesAsync();

            // Si se proporcionó usuario y password, registrar también en tabla User
            if (!string.IsNullOrWhiteSpace(trabajador.Usuario) &&
                !string.IsNullOrWhiteSpace(trabajador.Password))
            {
                CreatePasswordHash(trabajador.Password, out byte[] hash, out byte[] salt);

                var user = new User
                {
                    UserName = trabajador.Usuario,
                    PasswordHash = hash,
                    PasswordSalt = salt
                };

                await _userRepo.AddAsync(user);
                await _userRepo.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetAll), new { id = trabajador.IdTrabajador }, trabajador);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Trabajador trabajador)
        {
            if (id != trabajador.IdTrabajador)
                return BadRequest();

            _repo.Update(trabajador);
            await _repo.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trabajador = await _repo.GetByIdAsync(id);
            if (trabajador == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(trabajador.Usuario))
            {
                var usuario = await _userRepo.GetByUsernameAsync(trabajador.Usuario);
                if (usuario != null)
                {
                    _userRepo.Delete(usuario); // 👈 llama al nuevo método
                    await _userRepo.SaveChangesAsync();
                }
            }

            _repo.Delete(trabajador);
            await _repo.SaveChangesAsync();

            return NoContent();
        }



        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
