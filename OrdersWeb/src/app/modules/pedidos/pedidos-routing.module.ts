import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PedidoListComponent } from './pedido-list/pedido-list.component';
import { PedidoDetailComponent } from './pedido-detail/pedido-detail.component';
import { PedidoFormComponent } from './pedido-form/pedido-form.component';

const routes: Routes = [
  {
    path: '',
    component: PedidoListComponent,
    title: 'Lista de Pedidos'
  },
  {
    path: 'nuevo',
    component: PedidoFormComponent,
    title: 'Nuevo Pedido'
  },
  {
    path: ':id',
    component: PedidoDetailComponent,
    title: 'Detalle del Pedido'
  },
  {
    path: ':id/editar',
    component: PedidoFormComponent,
    title: 'Editar Pedido'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PedidosRoutingModule { }