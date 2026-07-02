import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditoriaService, AuditLogDto } from '../../../core/services/auditoria.service';

@Component({
  selector: 'app-auditoria',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auditoria.component.html',
  styleUrls: ['./auditoria.component.scss']
})
export class AuditoriaComponent implements OnInit {
  logs: AuditLogDto[] = [];
  cargando = true;
  error = '';

  entityNames = [
    'Categoria', 'Presentacion', 'Articulo', 'Cliente',
    'Proveedor', 'Trabajador', 'Ingreso', 'DetalleIngreso',
    'Venta', 'DetalleVenta', 'User'
  ];

  filtro = {
    entityName: '',
    action: '',
    desde: '',
    hasta: ''
  };

  paginaActual = 1;
  paginaSize = 20;
  expandedId: number | null = null;

  constructor(private auditoriaService: AuditoriaService) {}

  async ngOnInit() {
    await this.cargarLogs();
  }

  async cargarLogs() {
    this.cargando = true;
    this.error = '';
    try {
      this.logs = await this.auditoriaService.getAll({
        entityName: this.filtro.entityName || undefined,
        action: this.filtro.action || undefined,
        desde: this.filtro.desde || undefined,
        hasta: this.filtro.hasta || undefined,
        page: this.paginaActual,
        pageSize: this.paginaSize
      });
    } catch {
      this.error = 'Error al cargar los logs de auditoría';
    } finally {
      this.cargando = false;
    }
  }

  async buscar() {
    this.paginaActual = 1;
    await this.cargarLogs();
  }

  async cambiarPagina(delta: number) {
    this.paginaActual += delta;
    await this.cargarLogs();
  }

  toggleExpand(id: number) {
    this.expandedId = this.expandedId === id ? null : id;
  }

  formatearJson(json?: string): string {
    if (!json) return '—';
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }

  textoAccion(action: string): string {
    switch (action) {
      case 'Added': return 'Creación';
      case 'Modified': return 'Modificación';
      case 'Deleted': return 'Eliminación';
      default: return action;
    }
  }

  claseAccion(action: string): string {
    switch (action) {
      case 'Added': return 'badge-added';
      case 'Modified': return 'badge-modified';
      case 'Deleted': return 'badge-deleted';
      default: return '';
    }
  }
}
