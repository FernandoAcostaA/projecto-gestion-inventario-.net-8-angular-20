using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("detalle_ingreso")]
    public class DetalleIngreso
    {
        [Key]
        [Column("iddetalle_ingreso")]
        public int IdDetalleIngreso { get; set; }

        [Column("idingreso")]
        public int IdIngreso { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdIngreso))]
        public Ingreso? Ingreso { get; set; }

        [Column("idarticulo")]
        public int IdArticulo { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(IdArticulo))]
        public Articulo? Articulo { get; set; }

        [Column("precio_compra")]
        public decimal PrecioCompra { get; set; }

        [Column("precio_venta")]
        public decimal PrecioVenta { get; set; }

        [Column("stock_inicial")]
        public int StockInicial { get; set; }

        [Column("stock_actual")]
        public int StockActual { get; set; }

        [Column("fecha_produccion")]
        public DateTime FechaProduccion { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
