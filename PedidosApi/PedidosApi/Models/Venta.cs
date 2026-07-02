using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("venta")]
    public class Venta
    {
        [Key]
        [Column("idventa")]
        public int IdVenta { get; set; }

        [Column("idcliente")]
        public int IdCliente { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdCliente))]
        public Cliente? Cliente { get; set; }

        [Column("idtrabajador")]
        public int IdTrabajador { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdTrabajador))]
        public Trabajador? Trabajador { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("tipo_comprobante")]
        public string TipoComprobante { get; set; } = string.Empty;

        [Column("serie")]
        public string Serie { get; set; } = string.Empty;

        [Column("correlativo")]
        public string Correlativo { get; set; } = string.Empty;

        [Column("igv")]
        public decimal Igv { get; set; }

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
