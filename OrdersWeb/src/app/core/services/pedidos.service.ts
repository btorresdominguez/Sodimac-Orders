import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductoPedido {
  producto_id: number;
  cantidad: number;
  precio_unitario: number;
  subtotal?: number;
}

export interface PedidoRequest {
  cliente_id: number;
  ruta_id: number;
  fecha_entrega: string;
  estado_pedido: string;
  observaciones?: string;
  productos: ProductoPedido[];
}

export interface AsignacionRutaRequest {
  ruta_id: number;
  observaciones?: string;
}

export interface Pedido {
  id: number;
  cliente_id: number;
  ruta_id: number;
  fecha_pedido: string;
  fecha_entrega: string;
  fecha_actualizacion: string;
  estado_pedido: string;
  observaciones?: string;
  valor_total: number;
  productos: ProductoPedido[];
  cliente?: any;
  ruta?: any;
  nombre_cliente: string;
  nombre_ruta: string;
}

export interface PaginatedResponse<T> {
  datos: T[]; // Cambiado
  pagina_actual: number; // Cambiado
  tamano_pagina: number; // Cambiado
  total_registros: number; // Cambiado
  total_paginas: number; // Cambiado
  tiene_pagina_anterior: boolean; // Cambiado
  tiene_pagina_siguiente: boolean; // Cambiado
}

@Injectable({
  providedIn: 'root'
})
export class PedidosService {
  private baseUrl = `${environment.apiUrl}/api/pedidos`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    };
  }

  obtenerTodos(pagina: number = 1, tamano: number = 10): Observable<PaginatedResponse<Pedido>> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<Pedido>>(this.baseUrl, { params });
  }

  obtenerPorId(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  crear(pedido: PedidoRequest): Observable<Pedido> {
    return this.http.post<Pedido>(this.baseUrl, pedido, this.getHttpOptions());
  }

  actualizar(id: number, pedido: Partial<PedidoRequest>): Observable<Pedido> {
    return this.http.put<Pedido>(`${this.baseUrl}/${id}`, pedido, this.getHttpOptions());
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  cambiarEstado(id: number, nuevoEstado: string): Observable<Pedido> {
    const body = { estado_pedido: nuevoEstado };
    return this.http.patch<Pedido>(`${this.baseUrl}/${id}/estado`, body, this.getHttpOptions());
  }

  // Nuevo método para asignar ruta
  asignarRuta(id: number, asignacion: AsignacionRutaRequest): Observable<Pedido> {
    return this.http.put<Pedido>(`${this.baseUrl}/${id}/asignar-ruta`, asignacion, this.getHttpOptions());
  }

  obtenerPorCliente(clienteId: number, pagina: number = 1, tamano: number = 10): Observable<PaginatedResponse<Pedido>> {
    const params = new HttpParams()
      .set('cliente_id', clienteId.toString())
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<Pedido>>(`${this.baseUrl}/cliente`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerPorEstado(estado: string, pagina: number = 1, tamano: number = 10): Observable<PaginatedResponse<Pedido>> {
    const params = new HttpParams()
      .set('estado', estado)
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<Pedido>>(`${this.baseUrl}/estado`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerPorRango(fechaInicio: string, fechaFin: string, pagina: number = 1, tamano: number = 10): Observable<PaginatedResponse<Pedido>> {
    const params = new HttpParams()
      .set('fecha_inicio', fechaInicio)
      .set('fecha_fin', fechaFin)
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<Pedido>>(`${this.baseUrl}/rango`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  exportarPedidos(formato: 'excel' | 'pdf' = 'excel', filtros?: any): Observable<Blob> {
    let params = new HttpParams().set('formato', formato);
    
    if (filtros) {
      Object.keys(filtros).forEach(key => {
        if (filtros[key] !== null && filtros[key] !== undefined) {
          params = params.set(key, filtros[key].toString());
        }
      });
    }

    return this.http.get(`${this.baseUrl}/exportar`, {
      params,
      responseType: 'blob',
      headers: new HttpHeaders({
        'Accept': formato === 'excel' 
          ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
          : 'application/pdf'
      })
    });
  }

  // Métodos para reportes específicos
  obtenerEntregasPendientes(pagina: number = 1, tamano: number = 30): Observable<PaginatedResponse<any>> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<any>>(`${environment.apiUrl}/api/reportes/entregas-pendientes`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerEntregasCompletadas(pagina: number = 1, tamano: number = 40): Observable<PaginatedResponse<any>> {
    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamano_pagina', tamano.toString());

    return this.http.get<PaginatedResponse<any>>(`${environment.apiUrl}/api/reportes/entregas-completadas`, { 
      ...this.getHttpOptions(),
      params 
    });
  }
}