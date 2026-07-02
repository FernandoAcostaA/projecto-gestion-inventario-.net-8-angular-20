using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("detalle_venta")]
    public class DetalleVenta
    {
        [Key]
        [Column("iddetalle_venta")]
        public int IdDetalleVenta { get; set; }

        [Column("idventa")]
        public int IdVenta { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdVenta))]
        public Venta? Venta { get; set; }

        [Column("iddetalle_ingreso")]
        public int IdDetalleIngreso { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdDetalleIngreso))]
        public DetalleIngreso? DetalleIngreso { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("descuento")]
        public decimal Descuento { get; set; }
    }
}
