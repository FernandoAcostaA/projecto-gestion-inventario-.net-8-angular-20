using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [Column("idcliente")]
        public int IdCliente { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("apellidos")]
        public string? Apellidos { get; set; }

        [Column("sexo")]
        public string? Sexo { get; set; }

        [Column("fecha_nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("tipo_documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Column("num_documento")]
        public string NumDocumento { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
