import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Producto {
  id: number;
  nombre_producto: string;
  descripcion?: string;
  precio_unitario: number;
  stock: number;
  categoria?: string;
  codigo_producto?: string;
  activo: boolean;
  fecha_creacion?: string;
  fecha_actualizacion?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductoService {
  private baseUrl = `${environment.apiUrl}/api/productos`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    };
  }

  // ✅ Sin paginación
  obtenerTodos(): Observable<Producto[]> {
    return this.http.get<Producto[]>(this.baseUrl, this.getHttpOptions());
  }

  obtenerPorId(id: number): Observable<Producto> {
    return this.http.get<Producto>(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  crear(producto: Omit<Producto, 'id' | 'fecha_creacion' | 'fecha_actualizacion'>): Observable<Producto> {
    return this.http.post<Producto>(this.baseUrl, producto, this.getHttpOptions());
  }

  actualizar(id: number, producto: Partial<Producto>): Observable<Producto> {
    return this.http.put<Producto>(`${this.baseUrl}/${id}`, producto, this.getHttpOptions());
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  buscarPorNombre(nombre: string): Observable<Producto[]> {
    const params = new HttpParams().set('nombre', nombre);
    return this.http.get<Producto[]>(`${this.baseUrl}/buscar`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  obtenerPorCategoria(categoria: string): Observable<Producto[]> {
    const params = new HttpParams().set('categoria', categoria);
    return this.http.get<Producto[]>(`${this.baseUrl}/categoria`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  actualizarStock(id: number, nuevoStock: number): Observable<Producto> {
    const body = { stock: nuevoStock };
    return this.http.patch<Producto>(`${this.baseUrl}/${id}/stock`, body, this.getHttpOptions());
  }
}
