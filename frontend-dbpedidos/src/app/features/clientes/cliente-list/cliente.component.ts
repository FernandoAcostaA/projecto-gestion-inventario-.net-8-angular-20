import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { ClienteDto, ClienteService } from '../../../core/services/cliente.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-cliente',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './cliente.component.html',
  styleUrls: ['./cliente.component.scss']
})
export class ClienteComponent implements OnInit {
  clientes: ClienteDto[] = [];
  clientesFiltrados: ClienteDto[] = [];
  clientesPaginados: ClienteDto[] = [];

  form!: FormGroup;
  cargando = true;
  error = '';
  editandoId: number | null = null;
  mostrarFormulario = false;

  filtro = '';
  cantidadPorPagina = 5;
  paginaActual = 1;
  campoOrdenamiento: keyof ClienteDto = 'idCliente';
  ascendente = true;

  constructor(
    private clienteService: ClienteService,
    private fb: FormBuilder
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      nombre: ['', Validators.required],
      apellidos: [''],
      sexo: [''],
      fechaNacimiento: [''],
      tipoDocumento: ['CI', Validators.required],
      numDocumento: [''],
      direccion: [''],
      telefono: [''],
      email: ['', [Validators.email]]
    });

    await this.cargarClientes();
  }

  async cargarClientes() {
    this.cargando = true;
    try {
      this.clientes = await this.clienteService.getAll();
      this.actualizarVista();
    } catch {
      this.error = 'Error al cargar los clientes';
    } finally {
      this.cargando = false;
    }
  }

  actualizarVista() {
    const texto = this.filtro.toLowerCase();

    this.clientesFiltrados = this.clientes.filter(c =>
      (c.nombre?.toLowerCase().includes(texto) || '') ||
      (c.apellidos?.toLowerCase().includes(texto) || '') ||
      (c.sexo?.toLowerCase().includes(texto) || '') ||
      (c.fechaNacimiento?.toString().includes(texto) || '') ||
      (c.tipoDocumento?.toLowerCase().includes(texto) || '') ||
      (c.numDocumento?.toLowerCase().includes(texto) || '') ||
      (c.direccion?.toLowerCase().includes(texto) || '') ||
      (c.telefono?.toLowerCase().includes(texto) || '') ||
      (c.email?.toLowerCase().includes(texto) || '')
    );

    this.clientesFiltrados.sort((a, b) => {
      const valA = a[this.campoOrdenamiento] ?? '';
      const valB = b[this.campoOrdenamiento] ?? '';
      return this.ascendente
        ? valA > valB ? 1 : -1
        : valA < valB ? 1 : -1;
    });

    const inicio = (this.paginaActual - 1) * this.cantidadPorPagina;
    const fin = inicio + this.cantidadPorPagina;
    this.clientesPaginados = this.clientesFiltrados.slice(inicio, fin);
  }

  cambiarOrden(campo: keyof ClienteDto) {
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

    const datos = this.form.value;

    try {
      if (this.editandoId === null) {
        await this.clienteService.create(datos);
        Swal.fire('Creado', 'Cliente registrado correctamente', 'success');
      } else {
        await this.clienteService.update(this.editandoId, { ...datos, idCliente: this.editandoId });
        Swal.fire('Actualizado', 'Cliente actualizado correctamente', 'success');
      }

      await this.cargarClientes();
      this.cancelar();
    } catch {
      Swal.fire('Error', 'Error al guardar', 'error');
    }
  }

  editar(cliente: ClienteDto) {
    this.editandoId = cliente.idCliente!;
    const fechaFormateada = cliente.fechaNacimiento?.substring(0, 10) ?? '';

    this.form.patchValue({
      ...cliente,
      fechaNacimiento: fechaFormateada
    });

    this.mostrarFormulario = true;
  }

  cancelar() {
    this.form.reset({ tipoDocumento: 'CI' });
    this.editandoId = null;
    this.mostrarFormulario = false;
  }

  async eliminar(id: number) {
    const result = await Swal.fire({
      title: '¿Eliminar cliente?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar'
    });

    if (result.isConfirmed) {
      try {
        await this.clienteService.delete(id);
        await this.cargarClientes();
        Swal.fire('Eliminado', 'Cliente eliminado correctamente', 'success');
      } catch {
        Swal.fire('Error', 'No se pudo eliminar el cliente', 'error');
      }
    }
  }
}
