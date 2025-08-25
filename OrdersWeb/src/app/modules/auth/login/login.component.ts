import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [FormsModule, CommonModule]
})
export class LoginComponent {
  loginRequest = { email: '', contraseña: '' };
  errorMessage: string | null = null;
  isLoading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    if (this.isLoading) return;
    
    this.isLoading = true;
    this.errorMessage = null;

    this.authService.login(this.loginRequest).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.isLoading = false;
        // Redirigir a la lista de pedidos
        this.router.navigate(['/pedidos/pedido-form']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Error al iniciar sesión. Por favor, intenta de nuevo.';
        
        // Limpiar el error después de 5 segundos
        setTimeout(() => {
          this.errorMessage = null;
        }, 5000);
      }
    });
  }
}