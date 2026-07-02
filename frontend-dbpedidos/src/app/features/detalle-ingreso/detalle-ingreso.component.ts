// detalle-ingreso.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DetalleIngresoService } from '../../core/services/detalle-ingreso.service';
import { DetalleIngresoDto } from '../../core/models/detalle-ingreso.model';
import { ArticuloDto } from '../../core/services/articulo.service';
import { ArticuloService } from '../../core/services/articulo.service';

@Component({
  selector: 'app-detalle-ingreso',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  providers: [DatePipe],
  templateUrl: './detalle-ingreso.component.html',
  styleUrls: ['./detalle-ingreso.component.scss']
})
export class DetalleIngresoComponent implements OnInit {
  detalles: DetalleIngresoDto[] = [];
  detallesFiltrados: DetalleIngresoDto[] = [];
  detallesPaginados: DetalleIngresoDto[] = [];
  paginas: number[] = [];
  articulos: ArticuloDto[] = [];
  form!: FormGroup;
  editandoId: number | null = null;
  filtro = '';
  cantidadPorPagina = 5;
  paginaActual = 1;
  campoOrdenamiento: keyof DetalleIngresoDto = 'idDetalleIngreso';
  ascendente = true;
  mostrarFormulario = false;

  constructor(
    private detalleService: DetalleIngresoService,
    private articuloService: ArticuloService,
    private fb: FormBuilder
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      idDetalleIngreso: [null],
      idIngreso: [null, Validators.required],
      idArticulo: [null, Validators.required],
      precioCompra: [0, Validators.required],
      precioVenta: [0, Validators.required],
      stockInicial: [0, Validators.required],
      stockActual: [0, Validators.required],
      fechaProduccion: [new Date().toISOString().substring(0, 10), Validators.required],
      fechaVencimiento: [new Date().toISOString().substring(0, 10), Validators.required],
    });

    await Promise.all([
      this.cargarDetalles(),
      this.cargarArticulos()
    ]);
  }

  async cargarDetalles() {
    this.detalles = await this.detalleService.getAll();
    this.actualizarVista();
  }

  async cargarArticulos() {
    this.articulos = await this.articuloService.getAll();
  }

  actualizarVista() {
    const texto = this.filtro.toLowerCase();
    this.detallesFiltrados = this.detalles.filter(d =>
      String(d.idDetalleIngreso).includes(texto) ||
      String(d.idIngreso).includes(texto) ||
      (d.articuloNombre?.toLowerCase().includes(texto) || '') ||
      String(d.precioCompra).includes(texto) ||
      String(d.precioVenta).includes(texto) ||
      String(d.stockInicial).includes(texto) ||
      String(d.stockActual).includes(texto) ||
      d.fechaProduccion.includes(texto) ||
      d.fechaVencimiento.includes(texto)
    );

    this.detallesFiltrados.sort((a, b) => {
      const valA = a[this.campoOrdenamiento] ?? '';
      const valB = b[this.campoOrdenamiento] ?? '';
      return this.ascendente ? (valA > valB ? 1 : -1) : (valA < valB ? 1 : -1);
    });

    const inicio = (this.paginaActual - 1) * this.cantidadPorPagina;
    const fin = inicio + this.cantidadPorPagina;
    this.detallesPaginados = this.detallesFiltrados.slice(inicio, fin);

    const totalPaginas = Math.ceil(this.detallesFiltrados.length / this.cantidadPorPagina);
    this.paginas = Array.from({ length: totalPaginas }, (_, i) => i + 1);
  }

  cambiarOrden(campo: keyof DetalleIngresoDto) {
    if (this.campoOrdenamiento === campo) {
      this.ascendente = !this.ascendente;
    } else {
      this.campoOrdenamiento = campo;
      this.ascendente = true;
    }
    this.actualizarVista();
  }

  cambiarPagina(pagina: number) {
    this.paginaActual = pagina;
    this.actualizarVista();
  }

  async submit() {
    if (this.form.invalid) return;

    try {
      if (this.editandoId) {
        await this.detalleService.update(this.editandoId, this.form.value);
      } else {
        await this.detalleService.create(this.form.value);
      }

      await this.cargarDetalles();
      this.cancelar();
    } catch {
      alert('Error al guardar');
    }
  }

  editar(d: DetalleIngresoDto) {
    this.editandoId = d.idDetalleIngreso;
    this.form.patchValue({
      ...d,
      fechaProduccion: d.fechaProduccion.substring(0, 10),
      fechaVencimiento: d.fechaVencimiento.substring(0, 10),
    });
    this.mostrarFormulario = true;
  }

  cancelar() {
    this.editandoId = null;
    this.form.reset({
      precioCompra: 0,
      precioVenta: 0,
      stockInicial: 0,
      stockActual: 0,
      fechaProduccion: new Date().toISOString().substring(0, 10),
      fechaVencimiento: new Date().toISOString().substring(0, 10),
    });
    this.mostrarFormulario = false;
  }

  async eliminar(id: number) {
    if (!confirm('¿Eliminar este detalle?')) return;
    try {
      await this.detalleService.delete(id);
      await this.cargarDetalles();
    } catch {
      alert('Error al eliminar');
    }
  }
}
