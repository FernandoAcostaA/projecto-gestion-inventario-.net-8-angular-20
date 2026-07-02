export interface DetalleVentaDto {
  idDetalleVenta?: number;
  idVenta?: number;
  idDetalleIngreso: number;
  cantidad: number;
  precioVenta: number;
  descuento: number;
  articuloNombre?: string;
}

export interface VentaConDetallesDto {
  idCliente: number;
  idTrabajador?: number | null;
  fecha: string;
  tipoComprobante: string;
  serie: string;
  correlativo: string;
  igv: number;
  detalles: DetalleVentaDto[];
}

export interface VentaDto {
  idVenta: number;
  idCliente: number;
  idTrabajador: number;
  fecha: string;
  tipoComprobante: string;
  serie: string;
  correlativo: string;
  igv: number;
  clienteNombre?: string;
  trabajadorNombre?: string;
  detalles: DetalleVentaDto[];

  expanded?: boolean;
}

export interface ReposicionStockDto {
  idArticulo: number;
  cantidad: number;
  precioCompra: number;
  precioVenta: number;
}
