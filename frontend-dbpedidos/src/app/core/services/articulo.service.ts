import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface ArticuloDto {
  idArticulo: number;
  codigo: string;
  nombre: string;
  descripcion?: string;
  imagen?: string; // base64 para el frontend
  idCategoria: number;
  categoriaNombre?: string;
  idPresentacion: number;
  presentacionNombre?: string;
  stockTotal: number;
}



@Injectable({ providedIn: 'root' })
export class ArticuloService {
  private readonly apiUrl = `${environment.apiUrl}/articulos`;

  async getAll(): Promise<ArticuloDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async create(data: Partial<ArticuloDto>): Promise<ArticuloDto> {
    const response = await axios.post(this.apiUrl, data);
    return response.data;
  }

  async update(id: number, data: Partial<ArticuloDto>): Promise<ArticuloDto> {
    const response = await axios.put(`${this.apiUrl}/${id}`, data);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
