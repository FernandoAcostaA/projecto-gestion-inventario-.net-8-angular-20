using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("proveedor")]
    public class Proveedor
    {
        [Key]
        [Column("idproveedor")]
        public int IdProveedor { get; set; }

        [Column("razon_social")]
        public string RazonSocial { get; set; } = string.Empty;

        [Column("sector_comercial")]
        public string SectorComercial { get; set; } = string.Empty;

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

        [Column("url")]
        public string? Url { get; set; }

        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}
