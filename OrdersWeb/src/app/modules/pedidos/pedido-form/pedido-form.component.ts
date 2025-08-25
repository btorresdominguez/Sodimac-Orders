import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PedidosService, PedidoRequest } from '../../../core/services/pedidos.service';
import { ClienteService, Cliente } from '../../../core/services/clientes.service';
import { ProductoService, Producto } from '../../../core/services/productos.service';
import { RutaService, Ruta } from '../../../core/services/rutas.service';

@Component({
  selector: 'app-pedido-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './pedido-form.component.html',
  styleUrl: './pedido-form.component.scss'
})
export class PedidoFormComponent implements OnInit {
  pedidoForm!: FormGroup;
  loading = false;
  error: string | null = null;
  success: string | null = null;
  isEditMode = false;
  pedidoId: number | null = null;
  loadingData = false;

  estadosPedido = ['Pendiente', 'Confirmado', 'En_Preparacion', 'Enviado', 'Entregado', 'Cancelado'];
  
  clientes: Cliente[] = [];
  rutas: Ruta[] = [];
  productos: Producto[] = [];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private pedidosService: PedidosService,
    private clienteService: ClienteService,
    private productoService: ProductoService,
    private rutaService: RutaService
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.pedidoId = +params['id'];
        this.isEditMode = true;
      }
    });

    this.cargarDatosIniciales();
  }

  initializeForm(): void {
    this.pedidoForm = this.fb.group({
      cliente_id: [null, [Validators.required]],
      ruta_id: [null, [Validators.required]],
      fecha_entrega: ['', [Validators.required]],
      estado_pedido: ['Pendiente', [Validators.required]],
      observaciones: [''],
      productos: this.fb.array([], [Validators.required, Validators.minLength(1)])
    });
  }

  async cargarDatosIniciales(): Promise<void> {
    this.loadingData = true;
    this.error = null;

    try {
      await Promise.all([
        this.cargarClientes(),
        this.cargarProductos(),
        this.cargarRutas()
      ]);

      if (this.isEditMode && this.pedidoId) {
        await this.cargarPedido();
      } else {
        this.agregarProducto();
      }

    } catch (error) {
      console.error('Error al cargar datos iniciales:', error);
      this.error = 'Error al cargar los datos necesarios';
    } finally {
      this.loadingData = false;
    }
  }

  get productosFormArray(): FormArray {
    return this.pedidoForm.get('productos') as FormArray;
  }

  crearProductoFormGroup(): FormGroup {
    return this.fb.group({
      producto_id: [null, [Validators.required]],
      cantidad: [1, [Validators.required, Validators.min(1)]],
      precio_unitario: [0, [Validators.required, Validators.min(0.01)]]
    });
  }

  agregarProducto(): void {
    this.productosFormArray.push(this.crearProductoFormGroup());
  }

  removerProducto(index: number): void {
    if (this.productosFormArray.length > 1) {
      this.productosFormArray.removeAt(index);
    }
  }

  async cargarPedido(): Promise<void> {
    if (!this.pedidoId) return;

    this.loading = true;
    this.error = null;

    try {
      const pedido = await this.pedidosService.obtenerPorId(this.pedidoId).toPromise();
      
      if (pedido) {
        this.pedidoForm.patchValue({
          cliente_id: pedido.cliente_id,
          ruta_id: pedido.ruta_id,
          fecha_entrega: this.formatearFechaParaInput(pedido.fecha_entrega),
          estado_pedido: pedido.estado_pedido,
          observaciones: pedido.observaciones
        });

        this.productosFormArray.clear();
        pedido.productos.forEach(producto => {
          const productoGroup = this.crearProductoFormGroup();
          productoGroup.patchValue({
            producto_id: producto.producto_id,
            cantidad: producto.cantidad,
            precio_unitario: producto.precio_unitario
          });
          this.productosFormArray.push(productoGroup);
        });
      }
    } catch (err) {
      this.error = 'Error al cargar el pedido';
      console.error('Error:', err);
    } finally {
      this.loading = false;
    }
  }

  async cargarClientes(): Promise<void> {
    try {
      const response = await this.clienteService.obtenerTodos().toPromise();
      if (response) {
        this.clientes = response;
          // Redirigir a la lista de pedidos
        //this.router.navigate(['/pedidos/pedido-list']);
      }
    } catch (error) {
      console.error('Error al cargar clientes:', error);
      throw error;
    }
  }

  async cargarRutas(): Promise<void> {
    try {
      const response = await this.rutaService.obtenerTodos().toPromise();
      if (response) {
        this.rutas = response;
      }
    } catch (error) {
      console.error('Error al cargar rutas:', error);
      throw error;
    }
  }

  async cargarProductos(): Promise<void> {
    try {
      const response = await this.productoService.obtenerTodos().toPromise();
      if (response) {
        this.productos = response;
      }
    } catch (error) {
      console.error('Error al cargar productos:', error);
      throw error;
    }
  }

  onProductoChange(index: number): void {
    const productoControl = this.productosFormArray.at(index);
    const productoId = productoControl.get('producto_id')?.value;
    
    if (productoId) {
      const producto = this.productos.find(p => p.id === +productoId);
      if (producto) {
        productoControl.patchValue({
          precio_unitario: producto.precio_unitario
        });
      }
    }
  }

  calcularSubtotal(index: number): number {
    const productoControl = this.productosFormArray.at(index);
    const cantidad = productoControl.get('cantidad')?.value || 0;
    const precio = productoControl.get('precio_unitario')?.value || 0;
    return cantidad * precio;
  }

  calcularTotal(): number {
    let total = 0;
    for (let i = 0; i < this.productosFormArray.length; i++) {
      total += this.calcularSubtotal(i);
    }
    return total;
  }

  onSubmit(): void {
    if (this.pedidoForm.invalid) {
      this.marcarCamposComoTocados();
      this.error = 'Por favor, completa todos los campos obligatorios';
      return;
    }

    this.loading = true;
    this.error = null;
    this.success = null;

    const pedidoData: PedidoRequest = {
      ...this.pedidoForm.value,
      productos: this.pedidoForm.value.productos.map((p: any) => ({
        ...p,
        subtotal: p.cantidad * p.precio_unitario
      }))
    };

    // Log del objeto que se enviará (para debug)
    console.log('Pedido a enviar:', JSON.stringify(pedidoData, null, 2));

    const operacion = this.isEditMode 
      ? this.pedidosService.actualizar(this.pedidoId!, pedidoData)
      : this.pedidosService.crear(pedidoData);

    operacion.subscribe({
      next: (response) => {
        this.success = this.isEditMode 
          ? '¡Pedido actualizado exitosamente! 🎉'
          : '¡Pedido creado exitosamente! 🎉';
        
        this.loading = false;
        
        // Limpiar mensajes después de un tiempo
        setTimeout(() => {
          this.success = null;
        }, 5000);
        
        // Redirigir después del éxito
        setTimeout(() => {
            // Redirigir a la lista de pedidos
        this.router.navigate(['/pedidos/pedido-list']);
        }, 2000);
      },
      error: (err) => {
        this.error = this.isEditMode 
          ? 'Error al actualizar el pedido. Intenta nuevamente.'
          : 'Error al crear el pedido. Intenta nuevamente.';
        
        this.loading = false;
        console.error('Error:', err);
        
        // Limpiar error después de un tiempo
        setTimeout(() => {
          this.error = null;
        }, 5000);
      }
    });
  }

  private marcarCamposComoTocados(): void {
    Object.keys(this.pedidoForm.controls).forEach(key => {
      const control = this.pedidoForm.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormArray) {
        control.controls.forEach(arrayControl => {
          Object.keys((arrayControl as FormGroup).controls).forEach(arrayKey => {
            arrayControl.get(arrayKey)?.markAsTouched();
          });
        });
      }
    });
  }

  private formatearFechaParaInput(fecha: string): string {
    return new Date(fecha).toISOString().slice(0, 16);
  }

  onCancel(): void {
    if (this.pedidoForm.dirty) {
      if (confirm('¿Estás seguro de que quieres cancelar? Se perderán todos los cambios.')) {
        this.resetearFormulario();
      }
    } else {
      this.router.navigate(['/pedidos/pedido-form']);
    }
  }

  private resetearFormulario(): void {
    this.pedidoForm.reset();
    this.productosFormArray.clear();
    this.agregarProducto();
    this.error = null;
    this.success = null;
  }

  // Métodos de validación para el template
  isFieldInvalid(fieldName: string): boolean {
    const field = this.pedidoForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  isProductFieldInvalid(index: number, fieldName: string): boolean {
    const field = this.productosFormArray.at(index).get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.pedidoForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} es requerido`;
      if (field.errors['min']) return `${fieldName} debe ser mayor a ${field.errors['min'].min}`;
    }
    return '';
  }

    // ===============================
  // MÉTODOS DE NAVEGACIÓN
  // ===============================

  irAFormulario(): void {
    this.router.navigate(['/pedidos/pedido-list']);
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