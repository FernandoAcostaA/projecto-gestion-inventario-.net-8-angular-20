import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface PresentacionDto {
  idPresentacion: number;
  nombre: string;
  descripcion: string;
}

@Injectable({ providedIn: 'root' })
export class PresentacionService {
  private readonly apiUrl = `${environment.apiUrl}/presentaciones`;

  async getAll(): Promise<PresentacionDto[]> {
    const response = await axios.get(this.apiUrl);
    return response.data;
  }

  async create(data: Partial<PresentacionDto>): Promise<PresentacionDto> {
    const response = await axios.post(this.apiUrl, data);
    return response.data;
  }

  async update(id: number, data: Partial<PresentacionDto>): Promise<PresentacionDto> {
    const response = await axios.put(`${this.apiUrl}/${id}`, data);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
