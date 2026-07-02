import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';

import { ProveedorService, ProveedorDto } from '../../../core/services/proveedor.service';
import { TrabajadorService, TrabajadorDto } from '../../../core/services/trabajador.service';
import { ArticuloService, ArticuloDto } from '../../../core/services/articulo.service';
import { IngresoService } from '../../../core/services/ingreso.service';
import { IngresoConDetallesDto, IngresoDto } from '../../../core/models/ingreso.model';
import { DetalleIngresoDto } from '../../../core/models/detalle-ingreso.model';

import Swal from 'sweetalert2';

@Component({
  selector: 'app-ingreso',
  standalone: true,
  templateUrl: './ingreso.component.html',
  styleUrls: ['./ingreso.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, FormsModule]
})
export class IngresoComponent implements OnInit {
  form!: FormGroup;
  proveedores: ProveedorDto[] = [];
  trabajadores: TrabajadorDto[] = [];
  articulos: ArticuloDto[] = [];
  ingresos: IngresoDto[] = [];
  detalles: DetalleIngresoDto[] = [];

  mostrarFormulario: boolean = false;
  idIngresoEditando: number | null = null;

  busqueda: string = '';
  cantidadPorPagina = 10;
  paginaActual = 1;
  campoOrdenamiento: keyof IngresoDto | '' = '';
  ordenAscendente: boolean = true;
  Math = Math;

  ingresoSeleccionadoConDetalles: { [id: number]: DetalleIngresoDto[] } = {};
  filaExpandida: number | null = null;
  detalleEditandoIndex: number | null = null;

  constructor(
    private fb: FormBuilder,
    private proveedorService: ProveedorService,
    private trabajadorService: TrabajadorService,
    private articuloService: ArticuloService,
    private ingresoService: IngresoService
  ) {}

  fechaActual(): string {
    const hoy = new Date();
    return hoy.toISOString().split('T')[0];
  }

  async ngOnInit() {
    this.form = this.fb.group({
      idProveedor: [null, Validators.required],
      idTrabajador: [null, Validators.required],
      fecha: [this.fechaActual(), Validators.required],
      tipoComprobante: ['Boleta', Validators.required],
      serie: ['0001'],
      correlativo: ['0000001'],
      igv: [18, Validators.required],
      estado: ['Aceptado']
    });

    await this.cargarDatos();
  }

  async cargarDatos() {
  [this.proveedores, this.trabajadores, this.articulos, this.ingresos] = await Promise.all([
    this.proveedorService.getAll(),
    this.trabajadorService.getAll(),
    this.articuloService.getAll(),
    this.ingresoService.getAll()
  ]);
  this.ingresoSeleccionadoConDetalles = {}; // Limpia cache
}


  agregarDetalle() {
    const hoy = this.fechaActual();
    this.detalles.push({
      idDetalleIngreso: 0,
      idIngreso: 0,
      idArticulo: 0,
      precioCompra: 0,
      precioVenta: 0,
      stockInicial: 0,
      stockActual: 0,
      fechaProduccion: hoy,
      fechaVencimiento: hoy,
      articuloNombre: ''
    });
  }

  eliminarDetalle(index: number) {
    this.detalles.splice(index, 1);
  }

  obtenerNombreArticulo(idArticulo: number): string {
    const articulo = this.articulos.find(a => a.idArticulo === idArticulo);
    return articulo ? articulo.nombre : '—';
  }

  async toggleDetallesIngreso(idIngreso: number) {
    if (this.filaExpandida === idIngreso) {
      this.filaExpandida = null;
      this.detalleEditandoIndex = null;
    } else {
      this.filaExpandida = idIngreso;
      if (!this.ingresoSeleccionadoConDetalles[idIngreso]) {
        const ingresoCompleto = await this.ingresoService.getByIdCompleto(idIngreso);
        this.ingresoSeleccionadoConDetalles[idIngreso] = ingresoCompleto.detalles;
      }
    }
  }

  editarDetalleExpandido(index: number) {
    this.detalleEditandoIndex = index;
  }

  async guardarDetalleExpandido(idIngreso: number, index: number) {
  const detalles = this.ingresoSeleccionadoConDetalles[idIngreso];
  const detalle = detalles[index];

  const ingreso = this.ingresos.find(i => i.idIngreso === idIngreso);
  if (!ingreso) return;

  const ingresoActualizado: IngresoConDetallesDto = {
    idProveedor: ingreso.idProveedor,
    idTrabajador: ingreso.idTrabajador,
    fecha: ingreso.fecha,
    tipoComprobante: ingreso.tipoComprobante,
    serie: ingreso.serie,
    correlativo: ingreso.correlativo,
    igv: ingreso.igv,
    estado: ingreso.estado,
   detalles: detalles.map(d => ({
  idDetalleIngreso: d.idDetalleIngreso,
  idIngreso: d.idIngreso,
  idArticulo: d.idArticulo,
  precioCompra: d.precioCompra,
  precioVenta: d.precioVenta,
  stockInicial: d.stockInicial,
  stockActual: d.stockActual,
  fechaProduccion: new Date(d.fechaProduccion).toISOString(),
  fechaVencimiento: new Date(d.fechaVencimiento).toISOString()
}))

  };

  try {
    await this.ingresoService.update(idIngreso, ingresoActualizado);
    this.detalleEditandoIndex = null;
    Swal.fire('Guardado', 'Detalle actualizado correctamente en el servidor.', 'success');
  } catch (e) {
    console.error('❌ Error al actualizar detalle:', e);
    Swal.fire('Error', 'No se pudo actualizar el detalle.', 'error');
  }
}


  cancelarEdicionDetalle() {
    this.detalleEditandoIndex = null;
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      Swal.fire('Formulario inválido', 'Completa todos los campos requeridos.', 'warning');
      return;
    }

    if (this.detalles.length === 0) {
      Swal.fire('Sin detalles', 'Debes agregar al menos un detalle de ingreso.', 'warning');
      return;
    }

    for (const d of this.detalles) {
      if (!d.idArticulo || d.idArticulo === 0) {
        Swal.fire('Detalle inválido', 'Todos los detalles deben tener un artículo válido.', 'warning');
        return;
      }
    }

    const ingreso: IngresoConDetallesDto = {
      ...this.form.value,
      detalles: this.detalles.map(d => ({
        idArticulo: d.idArticulo,
        precioCompra: d.precioCompra,
        precioVenta: d.precioVenta,
        stockInicial: d.stockInicial,
        stockActual: d.stockActual,
        fechaProduccion: new Date(d.fechaProduccion).toISOString(),
        fechaVencimiento: new Date(d.fechaVencimiento).toISOString()
      }))
    };

    try {
      if (this.idIngresoEditando) {
        await this.ingresoService.update(this.idIngresoEditando, ingreso);
        Swal.fire('Actualizado', 'Ingreso editado correctamente.', 'success');
      } else {
        await this.ingresoService.create(ingreso);
        Swal.fire('Registrado', 'Ingreso registrado correctamente.', 'success');
      }

      this.cancelar();
      await this.cargarDatos();
    } catch (e) {
      console.error('❌ Error al guardar ingreso:', e);
      Swal.fire('Error', 'Ocurrió un error al guardar el ingreso.', 'error');
    }
  }

  async editarIngreso(ingreso: IngresoDto) {
    this.mostrarFormulario = true;
    this.idIngresoEditando = ingreso.idIngreso;

    const ingresoCompleto = await this.ingresoService.getByIdCompleto(ingreso.idIngreso);

    this.form.patchValue({
      idProveedor: ingresoCompleto.idProveedor,
      idTrabajador: ingresoCompleto.idTrabajador,
      fecha: ingresoCompleto.fecha.substring(0, 10),
      tipoComprobante: ingresoCompleto.tipoComprobante,
      serie: ingresoCompleto.serie,
      correlativo: ingresoCompleto.correlativo,
      igv: ingresoCompleto.igv,
      estado: ingresoCompleto.estado
    });

    this.detalles = ingresoCompleto.detalles.map(d => ({
      idDetalleIngreso: 0,
      idIngreso: 0,
      idArticulo: d.idArticulo,
      precioCompra: d.precioCompra,
      precioVenta: d.precioVenta,
      stockInicial: d.stockInicial,
      stockActual: d.stockActual,
      fechaProduccion: d.fechaProduccion.substring(0, 10),
      fechaVencimiento: d.fechaVencimiento.substring(0, 10),
      articuloNombre: ''
    }));
  }

  async eliminarIngreso(id: number) {
    const result = await Swal.fire({
      title: '¿Eliminar ingreso?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    });

    if (!result.isConfirmed) return;

    try {
      await this.ingresoService.delete(id);
      this.ingresos = this.ingresos.filter(i => i.idIngreso !== id);
      Swal.fire('Eliminado', 'Ingreso eliminado correctamente.', 'success');
    } catch {
      Swal.fire('Error', 'No se pudo eliminar el ingreso.', 'error');
    }
  }

  ordenarPor(campo: keyof IngresoDto) {
    if (this.campoOrdenamiento === campo) {
      this.ordenAscendente = !this.ordenAscendente;
    } else {
      this.campoOrdenamiento = campo;
      this.ordenAscendente = true;
    }
  }

  get ingresosFiltradosPaginados(): IngresoDto[] {
    let filtrados = this.ingresos.filter(i =>
      Object.values(i).some(v => v?.toString().toLowerCase().includes(this.busqueda.toLowerCase()))
    );

    if (this.campoOrdenamiento !== '') {
      filtrados = filtrados.sort((a, b) => {
        const campo = this.campoOrdenamiento as keyof IngresoDto;
        const valorA = a[campo];
        const valorB = b[campo];

        if (typeof valorA === 'string' && typeof valorB === 'string') {
          return this.ordenAscendente
            ? valorA.localeCompare(valorB)
            : valorB.localeCompare(valorA);
        }

        if (typeof valorA === 'number' && typeof valorB === 'number') {
          return this.ordenAscendente
            ? valorA - valorB
            : valorB - valorA;
        }

        return 0;
      });
    }

    const inicio = (this.paginaActual - 1) * this.cantidadPorPagina;
    return filtrados.slice(inicio, inicio + this.cantidadPorPagina);
  }

  cambiarPagina(offset: number) {
    this.paginaActual += offset;
  }

  get totalPaginas(): number {
    return Math.ceil(
      this.ingresos.filter(i =>
        Object.values(i).some(v => v?.toString().toLowerCase().includes(this.busqueda.toLowerCase()))
      ).length / this.cantidadPorPagina
    );
  }

  cancelar() {
    this.form.reset({
      idProveedor: null,
      idTrabajador: null,
      fecha: this.fechaActual(),
      tipoComprobante: 'Boleta',
      serie: '0001',
      correlativo: '0000001',
      igv: 18,
      estado: 'Aceptado'
    });
    this.detalles = [];
    this.idIngresoEditando = null;
    this.mostrarFormulario = false;
  }
}
