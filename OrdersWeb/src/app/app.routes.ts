import { Routes } from '@angular/router';
import { LoginComponent } from './modules/auth/login/login.component';
import { PedidoFormComponent } from './modules/pedidos/pedido-form/pedido-form.component';
import { PedidoListComponent } from './modules/pedidos/pedido-list/pedido-list.component';
import { DashboardComponent } from './modules/dashboard/dashboard.component'; // ✅ Importa tu Dashboard
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'pedidos/pedido-form',
    component: PedidoFormComponent,
    canActivate: [authGuard]
  },
  {
    path: 'pedidos/pedido-list', 
    component: PedidoListComponent,
    canActivate: [authGuard]
  },
  { 
    path: '**', 
    redirectTo: '/login' 
  }
];
