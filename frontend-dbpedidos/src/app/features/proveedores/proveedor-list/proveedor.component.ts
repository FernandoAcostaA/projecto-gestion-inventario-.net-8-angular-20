import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { ProveedorService, ProveedorDto } from '../../../core/services/proveedor.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-proveedor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './proveedor.component.html',
  styleUrls: ['./proveedor.component.scss']
})
export class ProveedorComponent implements OnInit {
  proveedores: ProveedorDto[] = [];
  form!: FormGroup;
  cargando = true;
  error = '';
  editandoId: number | null = null;

  filtro = '';
  mostrarFormulario = false;

  // Paginación
  proveedoresPorPagina = 5;
  paginaActual = 1;

  // Ordenamiento
  ordenColumna: keyof ProveedorDto | '' = '';
  ordenAscendente = true;

  constructor(
    private proveedorService: ProveedorService,
    private fb: FormBuilder
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      razonSocial: ['', Validators.required],
      sectorComercial: [''],
      tipoDocumento: ['RUC', Validators.required],
      numDocumento: [''],
      direccion: [''],
      telefono: [''],
      email: ['', [Validators.email]]
    });

    await this.cargarProveedores();
  }

  async cargarProveedores() {
    this.cargando = true;
    try {
      this.proveedores = await this.proveedorService.getAll();
    } catch {
      this.error = 'Error al cargar los proveedores';
    } finally {
      this.cargando = false;
    }
  }

  async submit() {
  if (this.form.invalid) return;

  const datos = this.form.value;

  try {
    if (this.editandoId === null) {
      await this.proveedorService.create(datos);
      Swal.fire('¡Registrado!', 'Proveedor registrado exitosamente.', 'success');
    } else {
      await this.proveedorService.update(this.editandoId, { ...datos, idProveedor: this.editandoId });
      Swal.fire('¡Actualizado!', 'Proveedor actualizado correctamente.', 'success');
    }

    await this.cargarProveedores();
    this.cancelar();
  } catch {
    Swal.fire('Error', 'No se pudo guardar el proveedor.', 'error');
  }
}


  editar(proveedor: ProveedorDto) {
    this.editandoId = proveedor.idProveedor;
    this.form.patchValue(proveedor);
    this.mostrarFormulario = true;
  }

  cancelar() {
    this.form.reset({ tipoDocumento: 'RUC' });
    this.editandoId = null;
    this.mostrarFormulario = false;
  }

  async eliminar(id: number) {
  const result = await Swal.fire({
    title: '¿Estás seguro?',
    text: 'Esta acción no se puede deshacer',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, eliminar',
    cancelButtonText: 'Cancelar'
  });

  if (!result.isConfirmed) return;

  try {
    await this.proveedorService.delete(id);
    await this.cargarProveedores();
    Swal.fire('¡Eliminado!', 'Proveedor eliminado correctamente.', 'success');
  } catch {
    Swal.fire('Error', 'No se pudo eliminar el proveedor.', 'error');
  }
}


  // Ordenamiento
  ordenarPor(columna: keyof ProveedorDto) {
    if (this.ordenColumna === columna) {
      this.ordenAscendente = !this.ordenAscendente;
    } else {
      this.ordenColumna = columna;
      this.ordenAscendente = true;
    }
  }

  // Filtro
  get proveedoresFiltrados(): ProveedorDto[] {
    const f = this.filtro.trim().toLowerCase();
    return this.proveedores.filter(p =>
      p.razonSocial.toLowerCase().includes(f) ||
      p.sectorComercial?.toLowerCase().includes(f) ||
      p.tipoDocumento.toLowerCase().includes(f) ||
      p.numDocumento?.toLowerCase().includes(f) ||
      p.direccion?.toLowerCase().includes(f) ||
      p.telefono?.toLowerCase().includes(f) ||
      p.email?.toLowerCase().includes(f)
    );
  }

  // Ordenados
  get proveedoresOrdenados(): ProveedorDto[] {
    const lista = [...this.proveedoresFiltrados];
    if (this.ordenColumna) {
      const columna = this.ordenColumna as keyof ProveedorDto;
      lista.sort((a, b) => {
        const valA = (a[columna] ?? '').toString().toLowerCase();
        const valB = (b[columna] ?? '').toString().toLowerCase();
        return this.ordenAscendente
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      });
    }
    return lista;
  }

  // Paginados
  get proveedoresPaginados(): ProveedorDto[] {
    const inicio = (this.paginaActual - 1) * this.proveedoresPorPagina;
    return this.proveedoresOrdenados.slice(inicio, inicio + this.proveedoresPorPagina);
  }

  get totalPaginas(): number {
  return Math.ceil(this.proveedoresOrdenados.length / this.proveedoresPorPagina);
}


  cambiarPagina(delta: number) {
    const totalPaginas = Math.ceil(this.proveedoresOrdenados.length / this.proveedoresPorPagina);
    this.paginaActual = Math.min(Math.max(this.paginaActual + delta, 1), totalPaginas);
  }
}
