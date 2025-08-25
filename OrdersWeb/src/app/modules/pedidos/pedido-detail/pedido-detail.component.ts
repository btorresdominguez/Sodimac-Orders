import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { PedidosService, Pedido } from '../../../core/services/pedidos.service';

@Component({
  selector: 'app-pedido-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './pedido-detail.component.html',
  styleUrl: './pedido-detail.component.scss'
})
export class PedidoDetailComponent implements OnInit {
  pedido: Pedido | null = null;
  pedidoId: number = 0;
  loading = false;
  error: string | null = null;
  success: string | null = null;

  estadosPedido = ['Pendiente', 'Confirmado', 'En_Preparacion', 'Enviado', 'Entregado', 'Cancelado'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pedidosService: PedidosService
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.pedidoId = +params['id'];
      if (this.pedidoId) {
        this.cargarPedido();
      }
    });
  }

  cargarPedido(): void {
    this.loading = true;
    this.error = null;
    this.success = null;
        
    this.pedidosService.obtenerPorId(this.pedidoId).subscribe({
      next: (pedido) => {
        this.pedido = pedido;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error al cargar el pedido';
        this.loading = false;
        console.error(err);
      }
    });
  }

  getEstadoClass(estado: string): string {
    const clases: { [key: string]: string } = {
      'Pendiente': 'bg-warning text-dark',
      'Confirmado': 'bg-info',
      'En_Preparacion': 'bg-primary', 
      'Enviado': 'bg-secondary',
      'Entregado': 'bg-success',
      'Cancelado': 'bg-danger'
    };
    return `badge ${clases[estado] || 'bg-secondary'}`;
  }

  cambiarEstado(nuevoEstado: string): void {
    if (confirm(`¿Cambiar estado a ${nuevoEstado}?`)) {
      this.pedidosService.cambiarEstado(this.pedidoId, nuevoEstado).subscribe({
        next: (pedidoActualizado) => {
          this.pedido = pedidoActualizado;
          this.success = `Estado cambiado a ${nuevoEstado} exitosamente`;
          // Limpiar el mensaje después de 3 segundos
          setTimeout(() => this.success = null, 3000);
        },
        error: (err) => {
          this.error = 'Error al cambiar el estado';
          console.error(err);
          // Limpiar el error después de 5 segundos
          setTimeout(() => this.error = null, 5000);
        }
      });
    }
  }

  eliminarPedido(): void {
    if (confirm('¿Está seguro de que desea eliminar este pedido?')) {
      this.pedidosService.eliminar(this.pedidoId).subscribe({
        next: () => {
          // CORREGIDO: Redirigir a la ruta correcta de la lista de pedidos
          this.router.navigate(['/app/pedidos']);
        },
        error: (err) => {
          this.error = 'Error al eliminar el pedido';
          console.error(err);
          // Limpiar el error después de 5 segundos
          setTimeout(() => this.error = null, 5000);
        }
      });
    }
  }

  volver(): void {
    // CORREGIDO: Redirigir a la ruta correcta de la lista de pedidos
    this.router.navigate(['/app/pedidos']);
  }

  editarPedido(): void {
    // Navegar al formulario de edición con el ID del pedido
    this.router.navigate(['/app/pedidos/editar', this.pedidoId]);
  }

  crearNuevoPedido(): void {
    // Navegar al formulario de creación
    this.router.navigate(['/app/pedidos/crear']);
  }

  imprimirPedido(): void {
    window.print();
  }

  exportarPDF(): void {
    this.pedidosService.exportarPedidos('pdf', { pedido_id: this.pedidoId }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `pedido_${this.pedidoId}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.error = 'Error al exportar PDF';
        console.error(err);
        // Limpiar el error después de 5 segundos
        setTimeout(() => this.error = null, 5000);
      }
    });
  }
}