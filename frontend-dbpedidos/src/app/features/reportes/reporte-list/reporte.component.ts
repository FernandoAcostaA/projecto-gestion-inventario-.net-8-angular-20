import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReporteService, ResumenDashboardDto, VentasPorPeriodoDto, TopArticuloDto, IngresosPorProveedorDto } from '../../../core/services/reporte.service';

@Component({
  selector: 'app-reporte',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reporte.component.html',
  styleUrls: ['./reporte.component.scss']
})
export class ReporteComponent implements OnInit {
  cargando = true;
  error = '';
  pestanaActiva = 'resumen';

  resumen: ResumenDashboardDto | null = null;

  ventasFiltro = { fechaInicio: '', fechaFin: '', agrupacion: 'dia' };
  ventas: VentasPorPeriodoDto[] = [];

  topFiltro = { fechaInicio: '', fechaFin: '', top: 10 };
  topArticulos: TopArticuloDto[] = [];

  ingresosFiltro = { fechaInicio: '', fechaFin: '' };
  ingresos: IngresosPorProveedorDto[] = [];

  constructor(private reporteService: ReporteService) {}

  async ngOnInit() {
    await this.cargarResumen();
  }

  async cargarResumen() {
    this.cargando = true;
    this.error = '';
    try {
      this.resumen = await this.reporteService.getResumen();
    } catch {
      this.error = 'Error al cargar el resumen';
    } finally {
      this.cargando = false;
    }
  }

  async cargarVentas() {
    this.cargando = true;
    this.error = '';
    try {
      this.ventas = await this.reporteService.getVentasPorPeriodo(
        this.ventasFiltro.fechaInicio || undefined,
        this.ventasFiltro.fechaFin || undefined,
        this.ventasFiltro.agrupacion
      );
    } catch {
      this.error = 'Error al cargar ventas por período';
    } finally {
      this.cargando = false;
    }
  }

  async cargarTopArticulos() {
    this.cargando = true;
    this.error = '';
    try {
      this.topArticulos = await this.reporteService.getTopArticulos(
        this.topFiltro.fechaInicio || undefined,
        this.topFiltro.fechaFin || undefined,
        this.topFiltro.top
      );
    } catch {
      this.error = 'Error al cargar top artículos';
    } finally {
      this.cargando = false;
    }
  }

  async cargarIngresos() {
    this.cargando = true;
    this.error = '';
    try {
      this.ingresos = await this.reporteService.getIngresosPorProveedor(
        this.ingresosFiltro.fechaInicio || undefined,
        this.ingresosFiltro.fechaFin || undefined
      );
    } catch {
      this.error = 'Error al cargar ingresos por proveedor';
    } finally {
      this.cargando = false;
    }
  }

  cambiarPestana(pestana: string) {
    this.pestanaActiva = pestana;
    this.error = '';
    if (pestana === 'resumen' && !this.resumen) this.cargarResumen();
    if (pestana === 'ventas' && this.ventas.length === 0) this.cargarVentas();
    if (pestana === 'top' && this.topArticulos.length === 0) this.cargarTopArticulos();
    if (pestana === 'proveedores' && this.ingresos.length === 0) this.cargarIngresos();
  }
}
