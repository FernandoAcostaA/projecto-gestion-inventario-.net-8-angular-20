import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface ResumenDashboardDto {
  totalClientes: number;
  totalProveedores: number;
  totalArticulos: number;
  ventasHoy: number;
  totalVentasHoy: number;
  ingresosHoy: number;
  totalIngresosHoy: number;
  articulosStockBajo: number;
  ventasMesActual: number;
  ingresosMesActual: number;
}

export interface VentasPorPeriodoDto {
  periodo: string;
  cantidad: number;
  total: number;
}

export interface TopArticuloDto {
  idArticulo: number;
  nombre: string;
  categoria?: string;
  totalVendido: number;
  totalIngresos: number;
}

export interface IngresosPorProveedorDto {
  idProveedor: number;
  proveedor: string;
  cantidadCompras: number;
  totalGastado: number;
}

@Injectable({ providedIn: 'root' })
export class ReporteService {
  private readonly apiUrl = `${environment.apiUrl}/reportes`;

  async getResumen(): Promise<ResumenDashboardDto> {
    const response = await axios.get(`${this.apiUrl}/resumen`);
    return response.data;
  }

  async getVentasPorPeriodo(fechaInicio?: string, fechaFin?: string, agrupacion = 'dia'): Promise<VentasPorPeriodoDto[]> {
    const params: any = { agrupacion };
    if (fechaInicio) params.fechaInicio = fechaInicio;
    if (fechaFin) params.fechaFin = fechaFin;
    const response = await axios.get(`${this.apiUrl}/ventas-por-periodo`, { params });
    return response.data;
  }

  async getTopArticulos(fechaInicio?: string, fechaFin?: string, top = 10): Promise<TopArticuloDto[]> {
    const params: any = { top };
    if (fechaInicio) params.fechaInicio = fechaInicio;
    if (fechaFin) params.fechaFin = fechaFin;
    const response = await axios.get(`${this.apiUrl}/top-articulos`, { params });
    return response.data;
  }

  async getIngresosPorProveedor(fechaInicio?: string, fechaFin?: string): Promise<IngresosPorProveedorDto[]> {
    const params: any = {};
    if (fechaInicio) params.fechaInicio = fechaInicio;
    if (fechaFin) params.fechaFin = fechaFin;
    const response = await axios.get(`${this.apiUrl}/ingresos-por-proveedor`, { params });
    return response.data;
  }
}
