import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { 
  DashboardService, 
  DashboardData, 
  EntregaPendiente, 
  EntregaCompletada,
  EstadisticasPendientes,
  EstadisticasCompletadas
} from '../../core/services/dashboard.service';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  // Estados de carga
  cargando = true;
  error: string | null = null;
  
  // Datos del dashboard
  dashboardData: DashboardData | null = null;
  
  // Estadísticas separadas para facilitar el acceso en el template
  estadisticasPendientes: EstadisticasPendientes | null = null;
  estadisticasCompletadas: EstadisticasCompletadas | null = null;
  
  // Listas de entregas para mostrar
  entregasUrgentes: EntregaPendiente[] = [];
  proximasEntregas: EntregaPendiente[] = [];
  ultimasCompletadas: EntregaCompletada[] = [];

  // Paginación
  paginaActual: number = 1;
  tamanoPorPagina: number = 40; // Cambia según lo necesites
  totalRegistros: number = 0;   // Total de registros para la paginación

  constructor(private dashboardService: DashboardService,   private router: Router) {}

  ngOnInit(): void {
    this.cargarDatosDashboard();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

 cargarDatosDashboard(): void {
    this.cargando = true;
    this.error = null;

    // Llama a los métodos específicos de entregar pendientes y completadas
    forkJoin([
        this.dashboardService.obtenerEntregasPendientes(this.paginaActual, this.tamanoPorPagina),
        this.dashboardService.obtenerEntregasCompletadas(this.paginaActual, this.tamanoPorPagina)
    ])
    .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.cargando = false)
    )
    .subscribe({
        next: ([pendientes, completadas]) => {
            this.dashboardData = {
                pendientes,
                completadas,
                resumenGeneral: {
                    total_entregas: pendientes.estadisticas.total_pendientes + completadas.estadisticas.total_completadas,
                    total_valor: pendientes.estadisticas.valor_total_pendiente + completadas.estadisticas.valor_total_entregado,
                    entregas_hoy: pendientes.entregas.datos.filter(e => e.dias_para_entrega === 0).length,
                    entregas_esta_semana: pendientes.entregas.datos.filter(e => e.dias_para_entrega <= 7).length
                }
            };
            this.estadisticasPendientes = pendientes.estadisticas;
            this.estadisticasCompletadas = completadas.estadisticas;
            this.procesarDatos(this.dashboardData);
            this.totalRegistros = completadas.entregas.total_registros; // Actualiza el total de registros
        },
        error: (error) => {
            console.error('Error al cargar dashboard:', error);
            this.error = 'Error al cargar los datos del dashboard. Por favor, intente nuevamente.';
        }
    });
}

  private procesarDatos(data: DashboardData): void {
    // Procesar datos para entregas urgentes, próximas y completadas
    this.entregasUrgentes = data.pendientes.entregas.datos
      .filter(e => e.dias_para_entrega <= 1)
      .slice(0, 5); // Mostrar máximo 5

    this.proximasEntregas = data.pendientes.entregas.datos
      .filter(e => e.dias_para_entrega > 1 && e.dias_para_entrega <= 7)
      .slice(0, 5); // Mostrar máximo 5

    this.ultimasCompletadas = data.completadas.entregas.datos
      .slice(0, 5); // Mostrar máximo 5
  }

  cambiarPagina(pagina: number): void {
    this.paginaActual = pagina;
    this.cargarDatosDashboard();
  }

exportarAExcel(): void {
    const EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
    
    if (this.dashboardData && this.dashboardData.completadas.entregas.datos.length > 0) {
        const datos = this.dashboardData.completadas.entregas.datos; // Asegúrate de que esta ruta sea correcta
        const hoja = XLSX.utils.json_to_sheet(datos);
        const libro = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(libro, hoja, 'Entregas');

        // Generar archivo Excel
        const excelBuffer: any = XLSX.write(libro, { bookType: 'xlsx', type: 'array' });
        const blob = new Blob([excelBuffer], { type: EXCEL_TYPE });

        saveAs(blob, 'entregas.xlsx'); // Usa saveAs para descargar el archivo
    } else {
        this.error = 'No hay datos para exportar.';
    }
}

  recargar(): void {
    this.cargarDatosDashboard();
  }

  formatearMoneda(valor: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(valor);
  }

  formatearFecha(fecha: string): string {
    return new Date(fecha).toLocaleDateString('es-CO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  formatearHora(fecha: string): string {
    return new Date(fecha).toLocaleTimeString('es-CO', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getClasePrioridad(dias: number): string {
    if (dias === 0) return 'urgente';
    if (dias === 1) return 'alta';
    if (dias <= 3) return 'media';
    return 'baja';
  }

  getTextoTiempo(dias: number): string {
    if (dias === 0) return 'Hoy';
    if (dias === 1) return 'Mañana';
    return `En ${dias} días`;
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