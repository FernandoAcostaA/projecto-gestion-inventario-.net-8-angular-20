import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { TrabajadorService, TrabajadorDto } from '../../../core/services/trabajador.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-trabajador',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './trabajador.component.html',
  styleUrls: ['./trabajador.component.scss']
})
export class TrabajadorComponent implements OnInit {
  trabajadores: TrabajadorDto[] = [];
  trabajadoresFiltrados: TrabajadorDto[] = [];
  trabajadoresPaginados: TrabajadorDto[] = [];

  form!: FormGroup;
  cargando = true;
  error = '';
  editandoId: number | null = null;
  mostrarFormulario = false;

  filtro = '';
  cantidadPorPagina = 5;
  paginaActual = 1;
  campoOrdenamiento: keyof TrabajadorDto = 'idTrabajador';
  ascendente = true;

  constructor(
    private trabajadorService: TrabajadorService,
    private fb: FormBuilder
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      apellidos: [''],
      sexo: ['M'],
      fechaNacimiento: [new Date().toISOString().substring(0, 10)],
      numDocumento: [''],
      direccion: [''],
      telefono: [''],
      email: ['', [Validators.email]],
      acceso: ['Usuario'],
      usuario: [''],
      password: ['']
    });

    await this.cargarTrabajadores();
  }

  async cargarTrabajadores() {
    this.cargando = true;
    try {
      this.trabajadores = await this.trabajadorService.getAll();
      this.actualizarVista();
    } catch {
      this.error = 'Error al cargar los trabajadores';
    } finally {
      this.cargando = false;
    }
  }

  actualizarVista() {
    const texto = this.filtro.toLowerCase();
    this.trabajadoresFiltrados = this.trabajadores.filter(t =>
      (t.nombre?.toLowerCase().includes(texto) || '') ||
      (t.apellidos?.toLowerCase().includes(texto) || '') ||
      (t.sexo?.toLowerCase().includes(texto) || '') ||
      (t.fechaNacimiento?.toString().includes(texto) || '') ||
      (t.numDocumento?.toLowerCase().includes(texto) || '') ||
      (t.direccion?.toLowerCase().includes(texto) || '') ||
      (t.telefono?.toLowerCase().includes(texto) || '') ||
      (t.email?.toLowerCase().includes(texto) || '') ||
      (t.acceso?.toLowerCase().includes(texto) || '') ||
      (t.usuario?.toLowerCase().includes(texto) || '')
    );

    this.trabajadoresFiltrados.sort((a, b) => {
      const valA = a[this.campoOrdenamiento] ?? '';
      const valB = b[this.campoOrdenamiento] ?? '';
      return this.ascendente
        ? valA > valB ? 1 : -1
        : valA < valB ? 1 : -1;
    });

    const inicio = (this.paginaActual - 1) * this.cantidadPorPagina;
    const fin = inicio + this.cantidadPorPagina;
    this.trabajadoresPaginados = this.trabajadoresFiltrados.slice(inicio, fin);
  }

  cambiarOrden(campo: keyof TrabajadorDto) {
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
      if (this.editandoId === null) {
          await this.trabajadorService.create(this.form.value);
          Swal.fire('Registrado', 'Trabajador creado correctamente', 'success');
      } else {
          await this.trabajadorService.update(this.editandoId, {
          ...this.form.value,
          idTrabajador: this.editandoId
          });
      Swal.fire('Actualizado', 'Trabajador actualizado correctamente', 'success');
    }

await this.cargarTrabajadores(); // 👈 refresca la lista


      this.cancelar();
      this.actualizarVista();
    } catch (error) {
      console.error(error);
      Swal.fire('Error', 'Error al guardar trabajador', 'error');
    }
  }

  editar(trabajador: TrabajadorDto) {
    this.editandoId = trabajador.idTrabajador!;
    this.form.patchValue({
      ...trabajador,
      fechaNacimiento: trabajador.fechaNacimiento?.substring(0, 10),
      password: ''
    });
    this.mostrarFormulario = true;
  }

  cancelar() {
    this.form.reset({
      sexo: 'M',
      acceso: 'Usuario',
      fechaNacimiento: new Date().toISOString().substring(0, 10)
    });
    this.editandoId = null;
    this.mostrarFormulario = false;
  }

  async eliminar(id: number) {
    const result = await Swal.fire({
      title: '¿Eliminar trabajador?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar'
    });

    if (result.isConfirmed) {
      try {
        await this.trabajadorService.delete(id);
        this.trabajadores = this.trabajadores.filter(t => t.idTrabajador !== id);
        this.actualizarVista();
        Swal.fire('Eliminado', 'Trabajador eliminado correctamente', 'success');

      } catch {
        Swal.fire('Error', 'No se pudo eliminar el trabajador', 'error');
      }
    }
  }
}
