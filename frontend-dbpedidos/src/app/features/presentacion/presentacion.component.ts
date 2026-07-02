import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { PresentacionService, PresentacionDto } from '../../core/services/presentacion.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-presentacion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './presentacion.component.html',
  styleUrls: ['./presentacion.component.scss']
})
export class PresentacionComponent implements OnInit {
  form!: FormGroup;
  presentaciones: PresentacionDto[] = [];
  filtro = '';
  registrosPorPagina = 5;
  paginaActual = 1;
  columnaOrden: keyof PresentacionDto = 'nombre';
  direccionAscendente = true;
  editingId: number | null = null;
  mostrarFormulario = false;

  constructor(
    private fb: FormBuilder,
    private presentacionService: PresentacionService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      descripcion: ['']
    });

    this.cargarPresentaciones();
  }

  async cargarPresentaciones() {
    try {
      this.presentaciones = await this.presentacionService.getAll();
    } catch {
      Swal.fire('Error', 'No se pudieron cargar las presentaciones', 'error');
    }
  }

  get presentacionesFiltradas(): PresentacionDto[] {
    const texto = this.filtro.toLowerCase().trim();
    return this.presentaciones.filter(p =>
      p.nombre.toLowerCase().includes(texto) ||
      (p.descripcion || '').toLowerCase().includes(texto)
    );
  }

  get presentacionesOrdenadas(): PresentacionDto[] {
    return [...this.presentacionesFiltradas].sort((a, b) => {
      const valorA = String(a[this.columnaOrden] ?? '');
      const valorB = String(b[this.columnaOrden] ?? '');
      return this.direccionAscendente
        ? valorA.localeCompare(valorB)
        : valorB.localeCompare(valorA);
    });
  }

  get totalPaginas(): number {
    return Math.ceil(this.presentacionesOrdenadas.length / this.registrosPorPagina);
  }

  get presentacionesPaginadas(): PresentacionDto[] {
    const inicio = (this.paginaActual - 1) * this.registrosPorPagina;
    return this.presentacionesOrdenadas.slice(inicio, inicio + this.registrosPorPagina);
  }

  actualizarVista() {
    this.paginaActual = 1;
  }

  cambiarPagina(direccion: number) {
    const nuevaPagina = this.paginaActual + direccion;
    if (nuevaPagina >= 1 && nuevaPagina <= this.totalPaginas) {
      this.paginaActual = nuevaPagina;
    }
  }

  ordenarPor(columna: keyof PresentacionDto) {
    if (this.columnaOrden === columna) {
      this.direccionAscendente = !this.direccionAscendente;
    } else {
      this.columnaOrden = columna;
      this.direccionAscendente = true;
    }
  }

  async submit() {
    if (this.form.invalid) return;

    const data = this.form.value;
    try {
      if (this.editingId) {
        await this.presentacionService.update(this.editingId, data);
        Swal.fire('Actualizado', 'La presentación fue actualizada correctamente', 'success');
      } else {
        await this.presentacionService.create({
          nombre: data.nombre,
          descripcion: data.descripcion
        });
        Swal.fire('Creado', 'La presentación fue registrada correctamente', 'success');
      }
      await this.cargarPresentaciones();
      this.cancelar();
    } catch {
      Swal.fire('Error', 'Hubo un problema al guardar la presentación', 'error');
    }
  }

  editar(p: PresentacionDto) {
    this.form.patchValue(p);
    this.editingId = p.idPresentacion;
    this.mostrarFormulario = true;
  }

  async eliminar(id: number) {
    const resultado = await Swal.fire({
      title: '¿Estás seguro?',
      text: 'Esta acción eliminará la presentación.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    });

    if (resultado.isConfirmed) {
      try {
        await this.presentacionService.delete(id);
        await this.cargarPresentaciones();
        Swal.fire('Eliminado', 'La presentación ha sido eliminada', 'success');
      } catch {
        Swal.fire('Error', 'No se pudo eliminar la presentación', 'error');
      }
    }
  }

  cancelar() {
    this.form.reset();
    this.editingId = null;
    this.mostrarFormulario = false;
  }
}
