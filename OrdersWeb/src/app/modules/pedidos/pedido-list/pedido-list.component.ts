import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { PedidosService, Pedido, PaginatedResponse } from '../../../core/services/pedidos.service';
import { ClienteService, Cliente } from '../../../core/services/clientes.service';
import { ProductoService, Producto } from '../../../core/services/productos.service';
import { RutaService, Ruta } from '../../../core/services/rutas.service';

@Component({
  selector: 'app-pedido-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './pedido-list.component.html',
  styleUrls: ['./pedido-list.component.scss']
})
export class PedidoListComponent implements OnInit {
  pedidos: Pedido[] = [];
  private todosPedidos: Pedido[] = [];
  
  loading = false;
  error: string | null = null;
  success: string | null = null;

  // Variables para el modal de edición
  mostrarModalEdicion = false;
  pedidoIdEditar: number | null = null;
  modalLoading = false;
  modalError: string | null = null;
  modalSuccess: string | null = null;

  // Variables para el modal de asignación de ruta
  mostrarModalAsignacion = false;
  pedidoIdAsignar: number | null = null;
  modalAsignacionLoading = false;
  modalAsignacionError: string | null = null;
  modalAsignacionSuccess: string | null = null;
  
  // Formulario del modal
  pedidoForm!: FormGroup;
  asignacionForm!: FormGroup;
  clientes: Cliente[] = [];
  rutas: Ruta[] = [];
  productos: Producto[] = [];

  // Paginación
  paginaActual = 1;
  totalPaginas = 1;
  tamanoPagina = 10;
  tienePaginaAnterior = false;
  tienePaginaSiguiente = false;

  filtroEstado = '';
  filtroFechaInicio = '';
  filtroFechaFin = '';
  filtroCliente = '';

  estadosPedido = ['Pendiente', 'Confirmado', 'En_Preparacion', 'En_Transito', 'Entregado', 'Cancelado'];

  private clientesMap: Record<string | number, string> = {};
  private rutasMap: Record<string | number, string> = {};

  constructor(
    private pedidosService: PedidosService, 
    private clienteService: ClienteService,
    private productoService: ProductoService,
    private rutaService: RutaService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.initForm();
    this.initAsignacionForm();
  }

  ngOnInit(): void {
    this.cargarPedidos();
    this.cargarCatalogos();
    this.cargarCatalogosModal();
  }

  cargarPedidos(): void {
    this.loading = true;
    this.error = null;

    this.pedidosService.obtenerTodos(this.paginaActual, this.tamanoPagina).subscribe({
      next: (response: PaginatedResponse<Pedido>) => {
        this.todosPedidos = response.datos || [];
        this.pedidos = [...this.todosPedidos];
        this.totalPaginas = response.total_paginas;
        this.tienePaginaAnterior = response.tiene_pagina_anterior;
        this.tienePaginaSiguiente = response.tiene_pagina_siguiente;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar los pedidos';
        this.loading = false;
        console.error('Error:', err);
      }
    });
  }

  private cargarCatalogos(): void {
    // Cargar clientes para el mapa de nombres
    this.clienteService.obtenerTodos().subscribe({
      next: (clientes: Cliente[]) => {
        for (const c of clientes || []) {
          this.clientesMap[c.id] = c.nombre ?? c.nombre ?? String(c.id);
        }
      },
      error: (err) => {
        console.error('Error cargando clientes para mapa:', err);
      }
    });

    // Cargar rutas para el mapa de nombres
    this.rutaService.obtenerTodos().subscribe({
      next: (rutas: Ruta[]) => {
        for (const r of rutas || []) {
          this.rutasMap[r.id] = r.nombre_ruta ?? String(r.id);
        }
      },
      error: (err) => {
        console.error('Error cargando rutas para mapa:', err);
      }
    });
  }

  getClienteNombre(id: number | string): string {
    return this.clientesMap[id] ?? String(id);
  }

  getRutaNombre(id: number | string): string {
    return this.rutasMap[id] ?? String(id);
  }

  getEstadoClass(estado: string): string {
    const clases: { [k: string]: string } = {
      'Pendiente': 'badge bg-warning text-dark',
      'Confirmado': 'badge bg-info text-white',
      'En_Preparacion': 'badge bg-primary text-white',
      'En_Transito': 'badge bg-secondary text-white',
      'Entregado': 'badge bg-success text-white',
      'Cancelado': 'badge bg-danger text-white'
    };
    return clases[estado] || 'badge bg-secondary';
  }

  cambiarEstadoPedido(pedidoId: number, nuevoEstado: string): void {
    this.pedidosService.cambiarEstado(pedidoId, nuevoEstado).subscribe({
      next: () => {
        const p = this.todosPedidos.find(x => x.id === pedidoId);
        if (p) {
          p.estado_pedido = nuevoEstado;
          p.fecha_actualizacion = new Date().toISOString();
        }
        const v = this.pedidos.find(x => x.id === pedidoId);
        if (v) v.estado_pedido = nuevoEstado;

        this.success = 'Estado cambiado con éxito ✅';
        setTimeout(() => (this.success = null), 2500);
      },
      error: (err) => {
        this.error = 'Error al cambiar el estado del pedido';
        console.error('Error:', err);
      }
    });
  }

  eliminarPedido(pedidoId: number): void {
    if (!confirm('¿Está seguro de que desea eliminar este pedido?')) return;

    this.pedidosService.eliminar(pedidoId).subscribe({
      next: () => {
        this.todosPedidos = this.todosPedidos.filter(p => p.id !== pedidoId);
        this.pedidos = this.pedidos.filter(p => p.id !== pedidoId);
        this.success = 'Pedido eliminado correctamente 🗑️';
        setTimeout(() => (this.success = null), 2500);
      },
      error: (err) => {
        this.error = 'Error al eliminar el pedido';
        console.error('Error:', err);
      }
    });
  }

  aplicarFiltros(): void {
    if (!Array.isArray(this.todosPedidos)) {
      this.pedidos = [];
      return;
    }

    const hasIni = !!this.filtroFechaInicio;
    const hasFin = !!this.filtroFechaFin;

    const ini = hasIni ? new Date(this.filtroFechaInicio) : null;
    const fin = hasFin ? new Date(this.filtroFechaFin) : null;

    const desde = ini ?? fin ? new Date((ini ?? fin)!) : null;
    const hasta = fin ?? ini ? new Date((fin ?? ini)!) : null;

    if (hasta) hasta.setHours(23, 59, 59, 999);

    const term = (this.filtroCliente || '').toLowerCase().trim();

    this.pedidos = this.todosPedidos.filter(p => {
      if (this.filtroEstado && p.estado_pedido !== this.filtroEstado) return false;

      if (term) {
        const nombre = this.getClienteNombre(p.cliente_id).toLowerCase();
        const idTxt = String(p.cliente_id ?? '').toLowerCase();
        if (!nombre.includes(term) && !idTxt.includes(term)) return false;
      }

      if (desde && hasta) {
        const f = new Date(p.fecha_entrega);
        if (isNaN(+f)) return false;
        if (f < desde || f > hasta) return false;
      }

      return true;
    });
  }

  limpiarFiltros(): void {
    this.filtroEstado = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.filtroCliente = '';

    if (Array.isArray(this.todosPedidos)) {
      this.pedidos = [...this.todosPedidos];
    } else {
      this.pedidos = [];
    }
  }

  cambiarPagina(nuevaPagina: number): void {
    if (nuevaPagina < 1 || nuevaPagina > this.totalPaginas) {
      return;
    }
    this.paginaActual = nuevaPagina;
    this.cargarPedidos();
  }

  // ===============================
  // MÉTODOS DEL MODAL
  // ===============================

  irAEditar(pedidoId: number): void {
    this.pedidoIdEditar = pedidoId;
    this.mostrarModalEdicion = true;
    this.cargarPedidoModal(pedidoId);
  }

  cerrarModalEdicion(): void {
    this.mostrarModalEdicion = false;
    this.pedidoIdEditar = null;
    this.limpiarAlertasModal();
  }

  private initForm(): void {
    this.pedidoForm = this.fb.group({
      cliente_id: ['', Validators.required],
      ruta_id: ['', Validators.required],
      fecha_entrega: ['', Validators.required],
      estado_pedido: ['Pendiente', Validators.required],
      observaciones: [''],
      productos: this.fb.array([])
    });
  }

  private initAsignacionForm(): void {
    this.asignacionForm = this.fb.group({
      ruta_id: ['', Validators.required],
      observaciones: ['']
    });
  }

  get productosFormArray(): FormArray {
    return this.pedidoForm.get('productos') as FormArray;
  }

  private async cargarCatalogosModal(): Promise<void> {
    try {
      // Cargar clientes
      const clientes = await this.clienteService.obtenerTodos().toPromise();
      if (clientes) {
        this.clientes = clientes;
        console.log('Clientes cargados:', this.clientes);
      }

      // Cargar rutas
      const rutas = await this.rutaService.obtenerTodos().toPromise();
      if (rutas) {
        this.rutas = rutas;
        console.log('Rutas cargadas:', this.rutas);
      }

      // Cargar productos
      const productos = await this.productoService.obtenerTodos().toPromise();
      if (productos) {
        this.productos = productos;
        console.log('Productos cargados:', this.productos);
      }

    } catch (error) {
      console.error('Error cargando catálogos:', error);
      this.error = 'Error al cargar los catálogos necesarios';
    }
  }

  private cargarPedidoModal(pedidoId: number): void {
    this.modalLoading = true;
    this.modalError = null;

    this.pedidosService.obtenerPorId(pedidoId).subscribe({
      next: (pedido: Pedido) => {
        this.llenarFormulario(pedido);
        this.modalLoading = false;
      },
      error: (err) => {
        this.modalError = 'Error al cargar el pedido';
        this.modalLoading = false;
        console.error('Error:', err);
      }
    });
  }

  private llenarFormulario(pedido: Pedido): void {
    // Limpiar productos existentes
    while (this.productosFormArray.length) {
      this.productosFormArray.removeAt(0);
    }

    const fechaEntrega = new Date(pedido.fecha_entrega);
    const fechaLocal = new Date(fechaEntrega.getTime() - fechaEntrega.getTimezoneOffset() * 60000)
      .toISOString().slice(0, 16);

    this.pedidoForm.patchValue({
      cliente_id: pedido.cliente_id,
      ruta_id: pedido.ruta_id,
      fecha_entrega: fechaLocal,
      estado_pedido: pedido.estado_pedido,
      observaciones: pedido.observaciones || ''
    });

    if (pedido.productos && pedido.productos.length > 0) {
      pedido.productos.forEach(producto => {
        this.productosFormArray.push(this.crearProductoFormGroup({
          producto_id: producto.producto_id,
          cantidad: producto.cantidad,
          precio_unitario: producto.precio_unitario
        }));
      });
    } else {
      this.agregarProductoModal();
    }
  }

  private crearProductoFormGroup(data?: any): FormGroup {
    return this.fb.group({
      producto_id: [data?.producto_id || '', Validators.required],
      cantidad: [data?.cantidad || 1, [Validators.required, Validators.min(1)]],
      precio_unitario: [data?.precio_unitario || 0, [Validators.required, Validators.min(0.01)]]
    });
  }

  agregarProductoModal(): void {
    this.productosFormArray.push(this.crearProductoFormGroup());
  }

  removerProductoModal(index: number): void {
    if (this.productosFormArray.length > 1) {
      this.productosFormArray.removeAt(index);
    }
  }

  onProductoChangeModal(index: number): void {
    const productoControl = this.productosFormArray.at(index);
    const productoId = productoControl.get('producto_id')?.value;
    
    if (productoId) {
      const producto = this.productos.find(p => p.id == productoId);
      if (producto) {
        productoControl.patchValue({
          precio_unitario: producto.precio_unitario
        });
      }
    }
  }

  calcularSubtotalModal(index: number): number {
    const producto = this.productosFormArray.at(index);
    const cantidad = producto.get('cantidad')?.value || 0;
    const precio = producto.get('precio_unitario')?.value || 0;
    return cantidad * precio;
  }

  calcularTotalModal(): number {
    return this.productosFormArray.controls.reduce((total, _, index) => {
      return total + this.calcularSubtotalModal(index);
    }, 0);
  }

  onSubmitModal(): void {
    if (this.pedidoForm.valid && this.pedidoIdEditar) {
      this.modalLoading = true;
      this.modalError = null;
      this.modalSuccess = null;

      const formData = this.pedidoForm.value;
      
      // Convertir fecha a formato ISO
      const fechaEntrega = new Date(formData.fecha_entrega);
      formData.fecha_entrega = fechaEntrega.toISOString();

      this.pedidosService.actualizar(this.pedidoIdEditar, formData).subscribe({
        next: (pedidoActualizado: Pedido) => {
          this.modalSuccess = 'Pedido actualizado correctamente ✅';
          this.modalLoading = false;
          
          // Actualizar listas
          this.onPedidoActualizado(pedidoActualizado);
          
          // Cerrar modal después de un breve delay
          setTimeout(() => {
            this.cerrarModalEdicion();
          }, 1500);
        },
        error: (err) => {
          this.modalError = 'Error al actualizar el pedido';
          this.modalLoading = false;
          console.error('Error:', err);
        }
      });
    }
  }

  onPedidoActualizado(pedidoActualizado: Pedido): void {
    // Actualizar en la lista completa
    const indexTodos = this.todosPedidos.findIndex(p => p.id === pedidoActualizado.id);
    if (indexTodos !== -1) {
      this.todosPedidos[indexTodos] = { ...pedidoActualizado };
    }

    // Actualizar en la lista filtrada
    const indexFiltrados = this.pedidos.findIndex(p => p.id === pedidoActualizado.id);
    if (indexFiltrados !== -1) {
      this.pedidos[indexFiltrados] = { ...pedidoActualizado };
    }

    // Mostrar mensaje de éxito en la lista principal
    this.success = 'Pedido actualizado correctamente ✅';
    setTimeout(() => (this.success = null), 3000);
  }

  private limpiarAlertasModal(): void {
    this.modalError = null;
    this.modalSuccess = null;
  }

  // ===============================
  // MÉTODOS DEL MODAL DE ASIGNACIÓN
  // ===============================

  irAAsignarRuta(pedidoId: number): void {
    this.pedidoIdAsignar = pedidoId;
    this.mostrarModalAsignacion = true;
    this.asignacionForm.reset({
      ruta_id: '',
      observaciones: ''
    });
    this.limpiarAlertasAsignacion();
  }

  cerrarModalAsignacion(): void {
    this.mostrarModalAsignacion = false;
    this.pedidoIdAsignar = null;
    this.limpiarAlertasAsignacion();
  }

  onSubmitAsignacion(): void {
    if (this.asignacionForm.valid && this.pedidoIdAsignar) {
      this.modalAsignacionLoading = true;
      this.modalAsignacionError = null;
      this.modalAsignacionSuccess = null;

      const formData = this.asignacionForm.value;

      this.pedidosService.asignarRuta(this.pedidoIdAsignar, formData).subscribe({
        next: (pedidoActualizado: Pedido) => {
          this.modalAsignacionSuccess = 'Ruta asignada correctamente ✅';
          this.modalAsignacionLoading = false;
          
          // Actualizar listas
          this.onPedidoActualizado(pedidoActualizado);
          
          // Cerrar modal después de un breve delay
          setTimeout(() => {
            this.cerrarModalAsignacion();
          }, 1500);
        },
        error: (err) => {
          this.modalAsignacionError = 'Error al asignar la ruta';
          this.modalAsignacionLoading = false;
          console.error('Error:', err);
        }
      });
    }
  }

  private limpiarAlertasAsignacion(): void {
    this.modalAsignacionError = null;
    this.modalAsignacionSuccess = null;
  }

  // ===============================
  // MÉTODOS DE NAVEGACIÓN
  // ===============================

  irAFormulario(): void {
    this.router.navigate(['/pedidos/pedido-form']);
  }

  irADashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    if (confirm('¿Está seguro de que desea cerrar sesión?')) {
      // Aquí puedes agregar la lógica de logout (limpiar tokens, etc.)
      localStorage.removeItem('token');
      sessionStorage.clear();
      this.router.navigate(['/login']);
    }
  }
}