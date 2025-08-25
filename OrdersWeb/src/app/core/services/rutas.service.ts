import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Ruta {
  id: number;
  nombre_ruta: string;
  descripcion?: string;
  zona: string;
  activa: boolean;
  orden_entrega?: number;
  tiempo_estimado?: number; // en minutos
  fecha_creacion?: string;
  fecha_actualizacion?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RutaService {
  private baseUrl = `${environment.apiUrl}/api/rutaentrega/activas`;

  constructor(private http: HttpClient) { }

  private getHttpOptions() {
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      })
    };
  }

  // ✅ Ahora devuelve directamente un array de rutas sin paginación
  obtenerTodos(): Observable<Ruta[]> {
    return this.http.get<Ruta[]>(this.baseUrl, this.getHttpOptions());
  }

  obtenerPorId(id: number): Observable<Ruta> {
    return this.http.get<Ruta>(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  crear(ruta: Omit<Ruta, 'id' | 'fecha_creacion' | 'fecha_actualizacion'>): Observable<Ruta> {
    return this.http.post<Ruta>(this.baseUrl, ruta, this.getHttpOptions());
  }

  actualizar(id: number, ruta: Partial<Ruta>): Observable<Ruta> {
    return this.http.put<Ruta>(`${this.baseUrl}/${id}`, ruta, this.getHttpOptions());
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`, this.getHttpOptions());
  }

  obtenerActivas(): Observable<Ruta[]> {
    return this.http.get<Ruta[]>(`${this.baseUrl}/activas`, this.getHttpOptions());
  }

  obtenerPorZona(zona: string): Observable<Ruta[]> {
    const params = new HttpParams().set('zona', zona);
    return this.http.get<Ruta[]>(`${this.baseUrl}/zona`, { 
      ...this.getHttpOptions(),
      params 
    });
  }

  cambiarEstado(id: number, activa: boolean): Observable<Ruta> {
    const body = { activa };
    return this.http.patch<Ruta>(`${this.baseUrl}/${id}/estado`, body, this.getHttpOptions());
  }
}
