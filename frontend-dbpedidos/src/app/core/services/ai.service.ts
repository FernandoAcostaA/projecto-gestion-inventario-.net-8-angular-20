import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface AiMessage {
  role: string;
  content: string;
}

export interface AiSuggestedAction {
  label: string;
  route: string;
  action?: string;
}

export interface AiDataChange {
  entity: string;
  action: string;
}

export interface AiChatResponse {
  reply: string;
  suggestedActions?: AiSuggestedAction[];
  dataChanges?: AiDataChange[];
}

export interface AiAutocompleteItem {
  id: number;
  nombre: string;
  codigo?: string;
  documento?: string;
  categoria?: string;
  presentacion?: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/ai`;

  async chat(message: string, history?: AiMessage[]): Promise<AiChatResponse> {
    const response = await axios.post(`${this.apiUrl}/chat`, { message, history });
    return response.data;
  }

  async recommend(type: string): Promise<string> {
    const response = await axios.post(`${this.apiUrl}/recommend`, { type });
    return response.data.recommendation;
  }

  async autocomplete(entityType: string, query: string, field?: string): Promise<AiAutocompleteItem[]> {
    const response = await axios.post(`${this.apiUrl}/autocomplete`, { entityType, query, field });
    return response.data;
  }

  async generateReport(prompt: string, period?: string): Promise<string> {
    const response = await axios.post(`${this.apiUrl}/report`, { prompt, period });
    return response.data.report;
  }

  async getClientInsights(idCliente: number): Promise<string> {
    const response = await axios.post(`${this.apiUrl}/cliente-insights`, { idCliente });
    return response.data.insights;
  }

  async searchArticlesByAI(query: string): Promise<AiAutocompleteItem[]> {
    const response = await axios.post(`${this.apiUrl}/buscar-articulos`, { query });
    return response.data;
  }

  async getDashboardSummary(period?: string): Promise<string> {
    const response = await axios.post(`${this.apiUrl}/dashboard-summary`, { period });
    return response.data.summary;
  }
}
