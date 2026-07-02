export interface DetalleIngresoDto {
  idDetalleIngreso: number; // ← hacerlo opcional
  idIngreso: number;        // ← hacerlo opcional
  idArticulo: number;
  precioCompra: number;
  precioVenta: number;
  stockInicial: number;
  stockActual: number;
  fechaProduccion: string;
  fechaVencimiento: string;
  articuloNombre?: string;   // usado solo en frontend
}
