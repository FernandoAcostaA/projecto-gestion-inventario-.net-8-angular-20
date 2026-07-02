import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';

import { VentaService } from '../../../core/services/venta.service';
import { ClienteService } from '../../../core/services/cliente.service';
import { TrabajadorService } from '../../../core/services/trabajador.service';
import { ArticuloService, ArticuloDto } from '../../../core/services/articulo.service';
import { DetalleIngresoService } from '../../../core/services/detalle-ingreso.service';
import { AiService } from '../../../core/services/ai.service';
import { RefreshService } from '../../../core/services/refresh.service';

import { ClienteDto } from '../../../core/services/cliente.service';
import { TrabajadorDto } from '../../../core/services/trabajador.service';
import { DetalleIngresoDto } from '../../../core/models/detalle-ingreso.model';
import { VentaDto, VentaConDetallesDto } from '../../../core/models/venta.model';

import Swal from 'sweetalert2';

@Component({
  selector: 'app-venta',
  standalone: true,
  templateUrl: './venta.component.html',
  styleUrls: ['./venta.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, FormsModule]
})
export class VentaComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  private refreshSub?: Subscription;
  ventas: VentaDto[] = [];
  ventasFiltradasPaginadas: VentaDto[] = [];

  clientes: ClienteDto[] = [];
  trabajadores: TrabajadorDto[] = [];
  articulos: ArticuloDto[] = [];
  detallesIngreso: DetalleIngresoDto[] = [];

  detalles: VentaConDetallesDto['detalles'] = [];
  idVentaEditando: number | null = null;
  mostrarFormulario = false;

  // Autocomplete cliente
  clienteTexto = '';
  mostrarDropdown = false;
  clienteSeleccionado: ClienteDto | null = null;
  creandoCliente = false;

  // AI insights cliente
  mostrarInsightsCliente = false;
  insightsCliente = '';
  cargandoInsight = false;

  // AI búsqueda artículos
  modoBusquedaIA = false;
  articulosIAResultados: ArticuloDto[] = [];

  // Autocomplete artículo
  articuloTexto = '';
  mostrarDropdownArticulo = false;

  // Paginación y orden
  filtro = '';
  campoOrdenamiento: keyof VentaDto = 'idVenta';
  ordenAscendente = true;
  paginaActual = 1;
  cantidadPorPagina = 5;

  constructor(
    private fb: FormBuilder,
    private ventaService: VentaService,
    private clienteService: ClienteService,
    private trabajadorService: TrabajadorService,
    private articuloService: ArticuloService,
    private detalleIngresoService: DetalleIngresoService,
    private aiService: AiService,
    private refreshService: RefreshService
  ) {}

  async ngOnInit() {
    this.form = this.fb.group({
      idCliente: [null],
      idTrabajador: [null],
      fecha: [new Date().toISOString().substring(0, 10), Validators.required],
      tipoComprobante: ['Boleta', Validators.required],
      serie: ['0001', Validators.required],
      correlativo: ['0000001', Validators.required],
      igv: [18, Validators.required]
    });

    await Promise.all([
      this.cargarVentas(),
      this.cargarClientes(),
      this.cargarTrabajadores(),
      this.cargarArticulos()
    ]);

    this.refreshSub = this.refreshService.changes$.subscribe(event => {
      if (event.entity === 'articulos') {
        this.cargarArticulos();
      }
      if (event.entity === 'clientes') {
        this.cargarClientes();
      }
    });
  }

  ngOnDestroy() {
    this.refreshSub?.unsubscribe();
  }

  async cargarVentas() {
    this.ventas = await this.ventaService.getAll();
    this.actualizarVista();
  }

  actualizarVista() {
    const filtradas = this.ventas.filter(v =>
      (v.clienteNombre ?? '').toLowerCase().includes(this.filtro.toLowerCase()) ||
      (v.trabajadorNombre ?? '').toLowerCase().includes(this.filtro.toLowerCase())
    );

    filtradas.sort((a, b) => {
  const valA = a[this.campoOrdenamiento] ?? '';
  const valB = b[this.campoOrdenamiento] ?? '';

  if (typeof valA === 'number' && typeof valB === 'number') {
    return this.ordenAscendente ? valA - valB : valB - valA;
  }

  return this.ordenAscendente
    ? valA.toString().localeCompare(valB.toString())
    : valB.toString().localeCompare(valA.toString());
});


    const inicio = (this.paginaActual - 1) * this.cantidadPorPagina;
    this.ventasFiltradasPaginadas = filtradas.slice(inicio, inicio + this.cantidadPorPagina);
  }

  cambiarOrden(campo: keyof VentaDto) {
    if (this.campoOrdenamiento === campo) {
      this.ordenAscendente = !this.ordenAscendente;
    } else {
      this.campoOrdenamiento = campo;
      this.ordenAscendente = true;
    }
    this.actualizarVista();
  }

  cambiarPagina(pagina: number) {
    this.paginaActual = pagina;
    this.actualizarVista();
  }

  actualizarCantidadPorPagina(n: number) {
    this.cantidadPorPagina = n;
    this.paginaActual = 1;
    this.actualizarVista();
  }

  async cargarClientes() {
    this.clientes = await this.clienteService.getAll();
  }

  async cargarTrabajadores() {
    this.trabajadores = await this.trabajadorService.getAll();
  }

  async cargarArticulos() {
    this.articulos = await this.articuloService.getAll();
    this.detallesIngreso = await this.detalleIngresoService.getAll();
  }

  agregarDetalle() {
    this.detalles.push({ idDetalleIngreso: 0, cantidad: 1, precioVenta: 0, descuento: 0 });
  }

  eliminarDetalle(i: number) {
    this.detalles.splice(i, 1);
  }

  get clientesFiltrados(): ClienteDto[] {
    if (!this.clienteTexto.trim()) return [];
    const texto = this.clienteTexto.toLowerCase();
    return this.clientes.filter(c =>
      `${c.nombre} ${c.apellidos ?? ''}`.toLowerCase().includes(texto) ||
      c.numDocumento.toLowerCase().includes(texto)
    );
  }

  get articulosFiltrados(): ArticuloDto[] {
    if (!this.articuloTexto.trim()) return [];
    if (this.modoBusquedaIA && this.articulosIAResultados.length > 0) {
      return this.articulosIAResultados;
    }
    const texto = this.articuloTexto.toLowerCase();
    return this.articulos.filter(a =>
      a.nombre.toLowerCase().includes(texto) ||
      a.codigo.toLowerCase().includes(texto)
    );
  }

  seleccionarArticulo(a: ArticuloDto) {
    this.articuloTexto = `${a.nombre} (${a.codigo})`;
    this.mostrarDropdownArticulo = false;

    // Buscar el batch más antiguo (FIFO) con stock > 0 para este artículo
    const batches = this.detallesIngreso
      .filter(d => d.idArticulo === a.idArticulo && d.stockActual > 0)
      .sort((x, y) => x.idDetalleIngreso - y.idDetalleIngreso);

    if (batches.length === 0) {
      Swal.fire('Sin stock', `"${a.nombre}" no tiene stock disponible.`, 'warning');
      return;
    }

    const batch = batches[0];
    this.detalles.push({
      idDetalleIngreso: batch.idDetalleIngreso,
      cantidad: 1,
      precioVenta: batch.precioVenta,
      descuento: 0
    });
  }

  getNombreArticuloDesdeBatch(idDetalleIngreso: number): string {
    const batch = this.detallesIngreso.find(d => d.idDetalleIngreso === idDetalleIngreso);
    if (!batch) return 'Artículo';
    const articulo = this.articulos.find(a => a.idArticulo === batch.idArticulo);
    return articulo ? `${articulo.nombre} (${articulo.codigo})` : 'Artículo';
  }

  onBlurArticulo() {
    setTimeout(() => {
      this.mostrarDropdownArticulo = false;
    }, 200);
  }

  seleccionarCliente(c: ClienteDto) {
    this.clienteSeleccionado = c;
    this.clienteTexto = `${c.nombre} ${c.apellidos ?? ''}`;
    this.mostrarDropdown = false;
    this.form.patchValue({ idCliente: c.idCliente });
  }

  async verInsightsCliente() {
    if (!this.clienteSeleccionado) return;
    this.cargandoInsight = true;
    this.mostrarInsightsCliente = true;
    this.insightsCliente = '';
    try {
      this.insightsCliente = await this.aiService.getClientInsights(this.clienteSeleccionado.idCliente);
    } catch {
      this.insightsCliente = 'No se pudieron obtener los insights del cliente. Verifica que la IA esté configurada.';
    } finally {
      this.cargandoInsight = false;
    }
  }

  cerrarInsights() {
    this.mostrarInsightsCliente = false;
    this.insightsCliente = '';
  }

  toggleBusquedaIA() {
    this.modoBusquedaIA = !this.modoBusquedaIA;
    this.articulosIAResultados = [];
    if (!this.modoBusquedaIA) {
      this.articuloTexto = '';
    }
  }

  async buscarConIA() {
    const texto = this.articuloTexto.trim();
    if (!texto) return;
    this.cargandoInsight = true;
    try {
      const resultados = await this.aiService.searchArticlesByAI(texto);
      this.articulosIAResultados = resultados.map(r => ({
        idArticulo: r.id,
        codigo: r.codigo || '',
        nombre: r.nombre,
        stockTotal: 0,
        idCategoria: 0,
        idPresentacion: 0
      }));
      const articulosCompletos = this.articulos.filter(a =>
        resultados.some(r => r.id === a.idArticulo)
      );
      if (articulosCompletos.length > 0) {
        this.articulosIAResultados = articulosCompletos;
      }
      this.mostrarDropdownArticulo = true;
    } catch {
      Swal.fire('Error', 'Error en la búsqueda IA. Intenta de nuevo.', 'error');
    } finally {
      this.cargandoInsight = false;
    }
  }

  async crearClienteRapido() {
    const nombreCompleto = this.clienteTexto.trim();
    if (!nombreCompleto || this.creandoCliente) return;

    this.creandoCliente = true;
    try {
      const partes = nombreCompleto.split(' ');
      const nombre = partes[0];
      const apellidos = partes.slice(1).join(' ') || nombre;

      const nuevo = await this.clienteService.create({
        nombre,
        apellidos,
        tipoDocumento: 'DNI',
        numDocumento: '00000000'
      });

      this.clientes.push(nuevo);
      this.seleccionarCliente(nuevo);
    } catch {
      Swal.fire('Error', 'No se pudo crear el cliente.', 'error');
    } finally {
      this.creandoCliente = false;
    }
  }

  onBlurCliente() {
    setTimeout(() => {
      this.mostrarDropdown = false;
    }, 200);
  }

  async submit() {
    if (this.detalles.length === 0) {
      Swal.fire('Error', 'Agrega al menos un detalle (artículo) a la venta.', 'error');
      return;
    }

    // Validar/crear cliente
    if (!this.clienteSeleccionado) {
      if (this.clienteTexto.trim()) {
        await this.crearClienteRapido();
        if (!this.clienteSeleccionado) return;
      } else {
        Swal.fire('Error', 'Selecciona o escribe el nombre de un cliente.', 'error');
        return;
      }
    }

    const dto: VentaConDetallesDto = {
      ...this.form.value,
      idCliente: this.clienteSeleccionado.idCliente,
      detalles: this.detalles
    };

    try {
      if (this.idVentaEditando) {
        await this.ventaService.update(this.idVentaEditando, dto);
        Swal.fire('Actualizado', 'La venta fue actualizada correctamente.', 'success');
      } else {
        await this.ventaService.createVentaConDetalles(dto);
        Swal.fire('Registrado', 'La venta fue registrada correctamente.', 'success');
      }

      this.cancelarEdicion();
      await this.cargarVentas();
    } catch (err) {
      Swal.fire('Error', 'Ocurrió un error al guardar la venta.', 'error');
    }
  }

  editarVenta(v: VentaDto) {
    this.idVentaEditando = v.idVenta;
    this.mostrarFormulario = true;

    const fechaFormateada = new Date(v.fecha).toISOString().split('T')[0];

    const cliente = this.clientes.find(c => c.idCliente === v.idCliente);
    if (cliente) {
      this.clienteSeleccionado = cliente;
      this.clienteTexto = `${cliente.nombre} ${cliente.apellidos ?? ''}`;
    }

    this.form.patchValue({
      idCliente: v.idCliente,
      idTrabajador: v.idTrabajador,
      fecha: fechaFormateada,
      tipoComprobante: v.tipoComprobante,
      serie: v.serie,
      correlativo: v.correlativo,
      igv: v.igv
    });

    this.detalles = v.detalles.map(d => ({
      idDetalleIngreso: d.idDetalleIngreso,
      cantidad: d.cantidad,
      precioVenta: d.precioVenta,
      descuento: d.descuento
    }));

    // Precargar artículo texto para el primer detalle
    if (this.detalles.length > 0) {
      const primerDetalle = this.detallesIngreso.find(d => d.idDetalleIngreso === this.detalles[0].idDetalleIngreso);
      if (primerDetalle) {
        const articulo = this.articulos.find(a => a.idArticulo === primerDetalle.idArticulo);
        if (articulo) {
          this.articuloTexto = `${articulo.nombre} (${articulo.codigo})`;
        }
      }
    }
  }

  cancelarEdicion() {
    this.idVentaEditando = null;
    this.mostrarFormulario = false;
    this.form.reset({
      tipoComprobante: 'Boleta',
      serie: '0001',
      correlativo: '0000001',
      igv: 18,
      fecha: new Date().toISOString().substring(0, 10)
    });
    this.clienteTexto = '';
    this.clienteSeleccionado = null;
    this.mostrarDropdown = false;
    this.articuloTexto = '';
    this.mostrarDropdownArticulo = false;
    this.detalles = [];
  }

  async eliminarVenta(id: number) {
    const result = await Swal.fire({
      title: '¿Eliminar venta?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    });

    if (result.isConfirmed) {
      try {
        await this.ventaService.delete(id);
        await this.cargarVentas();
        Swal.fire('Eliminado', 'La venta fue eliminada correctamente.', 'success');
      } catch {
        Swal.fire('Error', 'No se pudo eliminar la venta.', 'error');
      }
    }
  }
}
