import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface EntregaPendiente {
  pedido_id: number;
  nombre_cliente: string;
  direccion_entrega: string;
  email: string;
  fecha_entrega: string;
  valor_total: number;
  nombre_ruta: string;
  dias_para_entrega: number;
}

export interface EntregaCompletada {
  pedido_id: number;
  nombre_cliente: string;
  direccion_entrega: string;
  email: string;
  fecha_entrega: string;
  valor_total: number;
  total_productos: number;
  total_items: number;
  nombre_ruta: string;
}

export interface EstadisticasPendientes {
  total_pendientes: number;
  entregas_urgentes: number;
  entregas_proximas: number;
  valor_total_pendiente: number;
}

export interface EstadisticasCompletadas {
  total_completadas: number;
  valor_total_entregado: number;
  total_productos_entregados: number;
  total_items_entregados: number;
  promedio_valor_por_entrega: number;
}

export interface PaginatedResponse<T> {
  datos: T[];
  pagina_actual: number;
  tamano_pagina: number;
  total_registros: number;
  total_paginas: number;
  tiene_pagina_anterior: boolean;
  tiene_pagina_siguiente: boolean;
  enlace_pagina_anterior: string | null;
  enlace_pagina_siguiente: string | null;
  enlace_primera_pagina: string;
  enlace_ultima_pagina: string;
  info_paginacion: {
    desde: number;
    hasta: number;
    mostrando: number;
  };
}

export interface ResponsePendientes {
  estadisticas: EstadisticasPendientes;
  entregas: PaginatedResponse<EntregaPendiente>;
}

export interface ResponseCompletadas {
  estadisticas: EstadisticasCompletadas;
  entregas: PaginatedResponse<EntregaCompletada>;
}

export interface DashboardData {
  pendientes: ResponsePendientes;
  completadas: ResponseCompletadas;
  resumenGeneral: {
    total_entregas: number;
    total_valor: number;
    entregas_hoy: number;
    entregas_esta_semana: number;
  };
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private baseUrl = `${environment.apiUrl}/api/reportes`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    };
  }

  obtenerEntregasPendientes(pagina: number = 1, tamano: number = 30): Observable<ResponsePendientes> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<ResponsePendientes>(`${this.baseUrl}/entregas-pendientes`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerEntregasCompletadas(pagina: number = 1, tamano: number = 40): Observable<ResponseCompletadas> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<ResponseCompletadas>(`${this.baseUrl}/entregas-completadas`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerDashboard(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/dashboard`, this.getHttpOptions());
  }

obtenerDatosCompletos(pagina: number = 1, tamano: number = 30): Observable<DashboardData> {
    return forkJoin({
        pendientes: this.obtenerEntregasPendientes(pagina, tamano),
        completadas: this.obtenerEntregasCompletadas(pagina, tamano)
    }).pipe(
        map(({ pendientes, completadas }) => {
            const resumenGeneral = {
                total_entregas: pendientes.estadisticas.total_pendientes + completadas.estadisticas.total_completadas,
                total_valor: pendientes.estadisticas.valor_total_pendiente + completadas.estadisticas.valor_total_entregado,
                entregas_hoy: pendientes.entregas.datos.filter(e => e.dias_para_entrega === 0).length,
                entregas_esta_semana: pendientes.entregas.datos.filter(e => e.dias_para_entrega <= 7).length
            };

            return {
                pendientes,
                completadas,
                resumenGeneral
            };
        })
    );
}
}