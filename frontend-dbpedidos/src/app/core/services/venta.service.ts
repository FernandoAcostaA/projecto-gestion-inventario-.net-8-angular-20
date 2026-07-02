import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import { VentaDto, VentaConDetallesDto } from '../models/venta.model';

@Injectable({ providedIn: 'root' })
export class VentaService {
  private readonly apiUrl = `${environment.apiUrl}/ventas`;

  async getAll(): Promise<VentaDto[]> {
    const res = await axios.get(this.apiUrl);
    return res.data;
  }

  async createVentaConDetalles(data: VentaConDetallesDto): Promise<any> {
    const payload = {
      ...data,
      fecha: new Date(data.fecha),
      idCliente: Number(data.idCliente),
      idTrabajador: data.idTrabajador != null ? Number(data.idTrabajador) : null,
      detalles: data.detalles.map(d => ({
        ...d,
        idDetalleIngreso: Number(d.idDetalleIngreso)
      }))
    };
    const res = await axios.post(`${this.apiUrl}/completo`, payload);
    return res.data;
  }

  async update(id: number, data: VentaConDetallesDto): Promise<void> {
    const payload = {
      ...data,
      fecha: new Date(data.fecha),
      idCliente: Number(data.idCliente),
      idTrabajador: data.idTrabajador != null ? Number(data.idTrabajador) : null,
      detalles: data.detalles.map(d => ({
        ...d,
        idDetalleIngreso: Number(d.idDetalleIngreso)
      }))
    };
    await axios.put(`${this.apiUrl}/completo/${id}`, payload);
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
