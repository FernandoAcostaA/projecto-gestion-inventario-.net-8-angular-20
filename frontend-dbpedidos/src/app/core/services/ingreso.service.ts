import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import {
  IngresoDto,
  IngresoConDetallesDto,
  IngresoDtoConDetalles
} from '../models/ingreso.model';
import { ReposicionStockDto } from '../models/venta.model';

@Injectable({ providedIn: 'root' })
export class IngresoService {
  private readonly apiUrl = `${environment.apiUrl}/ingresos`;

  async getAll(): Promise<IngresoDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async getAllConDetalles(): Promise<IngresoDtoConDetalles[]> {
    const res = await axios.get(this.apiUrl);
    return res.data;
  }

  async create(data: IngresoConDetallesDto): Promise<{ idIngreso: number }> {
    const response = await axios.post(`${this.apiUrl}/completo`, data);
    return response.data;
  }
  async getByIdCompleto(id: number): Promise<IngresoConDetallesDto> {
  const response = await axios.get(`${this.apiUrl}/completo/${id}`);
  return response.data;
}

  async reposicionRapida(data: ReposicionStockDto): Promise<{ idIngreso: number; idDetalleIngreso: number }> {
    const response = await axios.post(`${this.apiUrl}/reposicion-rapida`, data);
    return response.data;
  }

  delete(id: number): Promise<void> {
    return axios.delete(`${this.apiUrl}/${id}`);
  }

  async update(id: number, data: IngresoConDetallesDto): Promise<void> {
  await axios.put(`${this.apiUrl}/completo/${id}`, data);
}

}
