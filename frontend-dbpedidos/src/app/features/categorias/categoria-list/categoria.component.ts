import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoriaService, CategoriaDto } from '../../../core/services/categoria.service';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-categoria',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './categoria.component.html',
  styleUrls: ['./categoria.component.scss']
})
export class CategoriaComponent implements OnInit {
  categorias: CategoriaDto[] = [];
  form!: FormGroup;
  cargando = true;
  error = '';
  editandoId: number | null = null;

  filtro = '';
  mostrarFormulario = false;

  categoriasPorPagina = 5;
  paginaActual = 1;

  ordenColumna: keyof CategoriaDto | '' = '';
  ordenAscendente = true;

  constructor(
    private categoriaService: CategoriaService,
    private fb: FormBuilder
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      descripcion: ['']
    });

    await this.cargarCategorias();
  }

  async cargarCategorias() {
    this.cargando = true;
    try {
      this.categorias = await this.categoriaService.getAll();
    } catch {
      this.error = 'Error al cargar las categorías';
    } finally {
      this.cargando = false;
    }
  }

  async submit() {
    if (this.form.invalid) return;

    try {
      if (this.editandoId === null) {
        const nueva = await this.categoriaService.create(this.form.value);
        this.categorias.push(nueva);
        Swal.fire('¡Registrado!', 'Categoría creada exitosamente.', 'success');
      } else {
        const actualizada = await this.categoriaService.update(this.editandoId, this.form.value);
        const i = this.categorias.findIndex(c => c.idCategoria === this.editandoId);
        if (i !== -1) this.categorias[i] = actualizada;
        Swal.fire('¡Actualizada!', 'Categoría actualizada correctamente.', 'success');
      }

      this.cancelar();
    } catch {
      Swal.fire('Error', 'No se pudo guardar la categoría.', 'error');
    }
  }

  editar(categoria: CategoriaDto) {
    this.editandoId = categoria.idCategoria;
    this.form.patchValue(categoria);
    this.mostrarFormulario = true;
  }

  cancelar() {
    this.form.reset();
    this.editandoId = null;
    this.mostrarFormulario = false;
  }

  async eliminar(id: number) {
    const confirm = await Swal.fire({
      title: '¿Eliminar categoría?',
      text: 'No podrás revertir esto',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    });

    if (!confirm.isConfirmed) return;

    try {
      await this.categoriaService.delete(id);
      this.categorias = this.categorias.filter(c => c.idCategoria !== id);
      Swal.fire('¡Eliminada!', 'Categoría eliminada.', 'success');
    } catch {
      Swal.fire('Error', 'No se pudo eliminar.', 'error');
    }
  }

  ordenarPor(columna: keyof CategoriaDto) {
    if (this.ordenColumna === columna) {
      this.ordenAscendente = !this.ordenAscendente;
    } else {
      this.ordenColumna = columna;
      this.ordenAscendente = true;
    }
  }

  get categoriasFiltradas(): CategoriaDto[] {
    const f = this.filtro.trim().toLowerCase();
    return this.categorias.filter(c =>
      c.nombre.toLowerCase().includes(f) ||
      c.descripcion?.toLowerCase().includes(f)
    );
  }

  get categoriasOrdenadas(): CategoriaDto[] {
    const lista = [...this.categoriasFiltradas];
    if (this.ordenColumna) {
      const col = this.ordenColumna;
      lista.sort((a, b) => {
        const valA = (a[col] ?? '').toString().toLowerCase();
        const valB = (b[col] ?? '').toString().toLowerCase();
        return this.ordenAscendente ? valA.localeCompare(valB) : valB.localeCompare(valA);
      });
    }
    return lista;
  }

  get categoriasPaginadas(): CategoriaDto[] {
    const start = (this.paginaActual - 1) * this.categoriasPorPagina;
    return this.categoriasOrdenadas.slice(start, start + this.categoriasPorPagina);
  }

  get totalPaginas(): number {
    return Math.ceil(this.categoriasOrdenadas.length / this.categoriasPorPagina);
  }

  cambiarPagina(delta: number) {
    const total = this.totalPaginas;
    this.paginaActual = Math.min(Math.max(this.paginaActual + delta, 1), total);
  }

  actualizarVista() {
    this.paginaActual = 1;
  }
}
