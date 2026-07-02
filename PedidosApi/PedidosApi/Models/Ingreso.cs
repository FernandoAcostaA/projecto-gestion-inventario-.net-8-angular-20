using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("ingreso")]
    public class Ingreso
    {
        [Key]
        [Column("idingreso")]
        public int IdIngreso { get; set; }

        [Column("idtrabajador")]
        public int IdTrabajador { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdTrabajador))]
        public Trabajador? Trabajador { get; set; }

        [Column("idproveedor")]
        public int IdProveedor { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdProveedor))]
        public Proveedor? Proveedor { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("tipo_comprobante")]
        public string TipoComprobante { get; set; } = string.Empty;

        [Column("serie")]
        [StringLength(10)]
        public string Serie { get; set; } = string.Empty;

        [Column("correlativo")]
        [StringLength(7)]
        public string Correlativo { get; set; } = string.Empty;

        [Column("igv")]
        public decimal Igv { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        public ICollection<DetalleIngreso> DetallesIngreso { get; set; } = new List<DetalleIngreso>();
    }
}
