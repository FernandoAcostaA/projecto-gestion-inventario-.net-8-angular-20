using Microsoft.EntityFrameworkCore;
using PedidosApi.Models;

namespace PedidosApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Presentacion> Presentaciones { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Trabajador> Trabajadores { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<DetalleIngreso> DetallesIngreso { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Para respetar los nombres de tabla en minúscula
            modelBuilder.Entity<Categoria>().ToTable("categoria");
            modelBuilder.Entity<Presentacion>().ToTable("presentacion");
            modelBuilder.Entity<Articulo>().ToTable("articulo");
            modelBuilder.Entity<Cliente>().ToTable("cliente");
            modelBuilder.Entity<Proveedor>().ToTable("proveedor");
            modelBuilder.Entity<Trabajador>().ToTable("trabajador");
            modelBuilder.Entity<Ingreso>().ToTable("ingreso");
            modelBuilder.Entity<DetalleIngreso>().ToTable("detalle_ingreso");
            modelBuilder.Entity<Venta>().ToTable("venta");
            modelBuilder.Entity<DetalleVenta>().ToTable("detalle_venta");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<AuditLog>().ToTable("audit_log");
        }
    }
}
