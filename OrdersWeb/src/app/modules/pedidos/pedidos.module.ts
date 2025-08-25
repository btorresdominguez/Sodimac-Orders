import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { PedidosRoutingModule } from './pedidos-routing.module';
import { PedidoListComponent } from './pedido-list/pedido-list.component';
import { PedidoDetailComponent } from './pedido-detail/pedido-detail.component';
import { PedidoFormComponent } from './pedido-form/pedido-form.component';

@NgModule({
  declarations: [
    // Los componentes standalone ya no se declaran aquí
    // pero mantenemos la estructura del módulo para lazy loading
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    PedidosRoutingModule
  ],
  providers: [
    // PedidosService se provee en 'root' desde el servicio
  ]
})
export class PedidosModule { }