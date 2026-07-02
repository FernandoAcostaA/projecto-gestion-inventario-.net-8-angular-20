import { DetalleIngresoDto } from './detalle-ingreso.model';

export interface IngresoDto {
  idIngreso: number;
  idProveedor: number;
  idTrabajador: number;
  fecha: string;
  tipoComprobante: string;
  serie: string;
  correlativo: string;
  igv: number;
  estado: string;
  nombreTrabajador: string;
  nombreProveedor: string;
}

export interface IngresoConDetallesDto {
  idTrabajador: number;
  idProveedor: number;
  fecha: string;
  tipoComprobante: string;
  serie: string;
  correlativo: string;
  igv: number;
  estado: string;
  detalles: DetalleIngresoDto[];
}

export interface IngresoDtoConDetalles extends IngresoDto {
  detalles?: DetalleIngresoDto[];
}
