using Microsoft.EntityFrameworkCore;
using PedidosApi.Data;
using PedidosApi.Models;
using PedidosApi.Repositories.Interfaces;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Cliente>> GetAllAsync() =>
        await _context.Clientes.ToListAsync();

    public async Task<Cliente?> GetByIdAsync(int id) =>
        await _context.Clientes.FindAsync(id);

    public async Task AddAsync(Cliente entity) =>
        await _context.Clientes.AddAsync(entity);

    public void Update(Cliente entity) =>
        _context.Clientes.Update(entity);

    public void Delete(Cliente entity) =>
        _context.Clientes.Remove(entity);

    public async Task<bool> SaveChangesAsync() =>
        await _context.SaveChangesAsync() > 0;
}
