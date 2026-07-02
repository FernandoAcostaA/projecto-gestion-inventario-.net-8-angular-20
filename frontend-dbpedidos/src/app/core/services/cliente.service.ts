import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface ClienteDto {
  idCliente: number;
  nombre: string;
  apellidos?: string;
  sexo?: string;
  fechaNacimiento?: string;
  tipoDocumento: string;
  numDocumento: string;
  direccion?: string;
  telefono?: string;
  email?: string;
}

@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly apiUrl = `${environment.apiUrl}/clientes`;

  async getAll(): Promise<ClienteDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async create(data: Partial<ClienteDto>): Promise<ClienteDto> {
    const response = await axios.post(this.apiUrl, data);
    return response.data;
  }

  async update(id: number, data: Partial<ClienteDto>): Promise<ClienteDto> {
    const response = await axios.put(`${this.apiUrl}/${id}`, data);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
