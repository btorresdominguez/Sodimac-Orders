import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7036/api/Auth'; 
  private readonly TOKEN_KEY = 'auth_token';

  constructor(private http: HttpClient) {}

  // Llama a la API de login
  login(loginRequest: { email: string; contraseña: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, loginRequest).pipe(
      tap((response: any) => {
        // Si el backend devuelve un token lo guardamos
        if (response && response.token) {
          localStorage.setItem(this.TOKEN_KEY, response.token);
        }
      }),
      catchError(this.handleError)
    );
  }

  // Elimina el token → logout
  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // Verifica si existe token
  isLoggedIn(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  // Devuelve el token
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private handleError(error: HttpErrorResponse) {
    return throwError(() => new Error(error.error.message || 'Error en el servidor'));
  }
}
