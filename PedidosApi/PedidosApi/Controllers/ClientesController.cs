using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.DTOs;
using PedidosApi.Models;

namespace PedidosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
        {
            var clientes = await _context.Clientes.ToListAsync();

            var dtoList = clientes.Select(c => new ClienteDto
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Apellidos = c.Apellidos,
                Sexo = c.Sexo,
                FechaNacimiento = c.FechaNacimiento,
                TipoDocumento = c.TipoDocumento,
                NumDocumento = c.NumDocumento,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email
            });

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetById(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null) return NotFound();

            var dto = new ClienteDto
            {
                IdCliente = c.IdCliente,
                Nombre = c.Nombre,
                Apellidos = c.Apellidos,
                Sexo = c.Sexo,
                FechaNacimiento = c.FechaNacimiento,
                TipoDocumento = c.TipoDocumento,
                NumDocumento = c.NumDocumento,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ClienteDto dto)
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre,
                Apellidos = dto.Apellidos,
                Sexo = dto.Sexo,
                FechaNacimiento = dto.FechaNacimiento,
                TipoDocumento = dto.TipoDocumento,
                NumDocumento = dto.NumDocumento,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Email = dto.Email
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            dto.IdCliente = cliente.IdCliente;
            return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClienteDto dto)
        {
            if (id != dto.IdCliente) return BadRequest();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            cliente.Nombre = dto.Nombre;
            cliente.Apellidos = dto.Apellidos;
            cliente.Sexo = dto.Sexo;
            cliente.FechaNacimiento = dto.FechaNacimiento;
            cliente.TipoDocumento = dto.TipoDocumento;
            cliente.NumDocumento = dto.NumDocumento;
            cliente.Direccion = dto.Direccion;
            cliente.Telefono = dto.Telefono;
            cliente.Email = dto.Email;

            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
