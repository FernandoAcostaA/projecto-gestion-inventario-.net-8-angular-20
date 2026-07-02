using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("trabajador")]
    public class Trabajador
    {
        [Key]
        [Column("idtrabajador")]
        public int IdTrabajador { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("sexo")]
        public string Sexo { get; set; } = string.Empty;

        [Column("fecha_nac")]
        public DateTime FechaNacimiento { get; set; }

        [Column("num_documento")]
        public string NumDocumento { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("acceso")]
        public string Acceso { get; set; } = string.Empty;

        [Column("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
