import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Cliente {
  id: number;
  nombre: string;
  telefono?: string;
  email?: string;
  direccion?: string;
  ciudad?: string;
  activo: boolean;
  fecha_creacion?: string;
  fecha_actualizacion?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private baseUrl = `${environment.apiUrl}/api/clientes`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    };
  }

  // ✅ Ahora devuelve todos los clientes directamente sin paginado
  obtenerTodos(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(this.baseUrl, this.getHttpOptions());
  }

  obtenerPorId(id: number): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  crear(cliente: Omit<Cliente, 'id' | 'fecha_creacion' | 'fecha_actualizacion'>): Observable<Cliente> {
    return this.http.post<Cliente>(this.baseUrl, cliente, this.getHttpOptions());
  }

  actualizar(id: number, cliente: Partial<Cliente>): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.baseUrl}/${id}`, cliente, this.getHttpOptions());
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  buscarPorNombre(nombre: string): Observable<Cliente[]> {
    const params = new HttpParams().set('nombre', nombre);
    return this.http.get<Cliente[]>(`${this.baseUrl}/buscar`, { 
      ...this.getHttpOptions(),
      params 
    });
  }
}
