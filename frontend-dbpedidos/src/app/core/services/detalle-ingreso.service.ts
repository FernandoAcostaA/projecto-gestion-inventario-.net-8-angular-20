import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';
import { DetalleIngresoDto } from '../models/detalle-ingreso.model';

@Injectable({ providedIn: 'root' })
export class DetalleIngresoService {
  private apiUrl = `${environment.apiUrl}/detalleingreso`;

  async getAll(): Promise<DetalleIngresoDto[]> {
    const res = await axios.get(this.apiUrl);
    return res.data;
  }

  async getById(id: number): Promise<DetalleIngresoDto> {
    const res = await axios.get(`${this.apiUrl}/${id}`);
    return res.data;
  }

  async create(dto: DetalleIngresoDto): Promise<void> {
    await axios.post(this.apiUrl, dto);
  }

  async update(id: number, dto: DetalleIngresoDto): Promise<void> {
    await axios.put(`${this.apiUrl}/${id}`, dto);
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
