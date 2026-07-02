import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface AuditLogDto {
  id: number;
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userName?: string;
  ipAddress?: string;
  timestamp: string;
}

export interface AuditLogFilter {
  entityName?: string;
  action?: string;
  desde?: string;
  hasta?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class AuditoriaService {
  private readonly apiUrl = `${environment.apiUrl}/auditlogs`;

  async getAll(filtro?: AuditLogFilter): Promise<AuditLogDto[]> {
    const params: any = {};
    if (filtro) {
      if (filtro.entityName) params.entityName = filtro.entityName;
      if (filtro.action) params.action = filtro.action;
      if (filtro.desde) params.desde = filtro.desde;
      if (filtro.hasta) params.hasta = filtro.hasta;
      if (filtro.page) params.page = filtro.page;
      if (filtro.pageSize) params.pageSize = filtro.pageSize;
    }
    const response = await axios.get(this.apiUrl, { params });
    return response.data;
  }

  async getById(id: number): Promise<AuditLogDto> {
    const response = await axios.get(`${this.apiUrl}/${id}`);
    return response.data;
  }
}
