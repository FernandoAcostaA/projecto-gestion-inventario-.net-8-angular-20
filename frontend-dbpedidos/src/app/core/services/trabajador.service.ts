import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface TrabajadorDto {
  idTrabajador: number;
  nombre: string;
  apellidos: string;
  sexo: string;
  fechaNacimiento: string;
  numDocumento: string;
  direccion: string;
  telefono: string;
  email: string;
  acceso: string;
  usuario: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class TrabajadorService {
  private readonly apiUrl = `${environment.apiUrl}/trabajadores`;

  async getAll(): Promise<TrabajadorDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async create(data: Partial<TrabajadorDto>): Promise<TrabajadorDto> {
    const response = await axios.post(this.apiUrl, data);
    return response.data;
  }

  async update(id: number, data: Partial<TrabajadorDto>): Promise<TrabajadorDto> {
    const response = await axios.put(`${this.apiUrl}/${id}`, data);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
