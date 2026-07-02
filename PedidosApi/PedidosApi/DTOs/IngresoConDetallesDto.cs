using System;
using System.Collections.Generic;

namespace PedidosApi.DTOs
{
    public class DetalleIngresoSimpleDto
    {
        public int IdArticulo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockInicial { get; set; }
        public int StockActual { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }

    public class IngresoConDetallesDto
    {
        public int IdProveedor { get; set; }
        public int IdTrabajador { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public decimal Igv { get; set; }
        public string Estado { get; set; } = "Emitido";

        public List<DetalleIngresoSimpleDto> Detalles { get; set; } = new();
    }
}
