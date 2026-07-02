import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface ProveedorDto {
  idProveedor: number;
  razonSocial: string;
  sectorComercial: string;
  tipoDocumento: string;
  numDocumento: string;
  direccion?: string;
  telefono?: string;
  email?: string;
}


@Injectable({ providedIn: 'root' })
export class ProveedorService {
  private readonly apiUrl = `${environment.apiUrl}/proveedores`;

  async getAll(): Promise<ProveedorDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async create(data: Partial<ProveedorDto>): Promise<ProveedorDto> {
    const response = await axios.post(this.apiUrl, data);
    return response.data;
  }

  async update(id: number, data: Partial<ProveedorDto>): Promise<ProveedorDto> {
    const response = await axios.put(`${this.apiUrl}/${id}`, data);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
