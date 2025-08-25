# Sodimac Orders API

## Descripción

La API de **Sodimac Orders** permite gestionar pedidos, clientes, rutas de entrega y productos. Este proyecto está basado en **.NET 9** y utiliza Docker para facilitar el despliegue. También incluye pruebas automatizadas utilizando **xUnit**.

## Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Postman](https://www.postman.com/downloads/)
- [xUnit](https://xunit.net/)

### Estructura de Carpetas

D:\SodimacOrders\
    ├── src\
        ├── SodimacOrders.Application\
        ├── SodimacOrders.Domain\
        ├── SodimacOrders.Infrastructure\
        ├── SodimacOrders.WebApi\
    ├── Dockerfile
## Configuración del Entorno

### 1. Clonar el Repositorio

git clone https://github.com/btorresdominguez/Sodimac-Orders.git

### Uso de la API

La API estará disponible en https://localhost:7036.
Ejemplos de Endpoints

### Colección de Autenticación

{
  "email": "admin@sodimac.com",
  "contraseña": "123456"
}
### Pedidos


Obtener Todos los Pedidos: GET {{baseURL}}/api/pedidos

Crear Nuevo Pedido: POST {{baseURL}}/api/pedidos

{
  "cliente_id": 1,
  "ruta_id": 1,
  "fecha_entrega": "2025-08-30T10:00:00",
  "estado_pedido": "Pendiente",
  "observaciones": "Urgente",
  "productos": [
    {
      "producto_id": 1,
      "cantidad": 2,
      "precio_unitario": 150.00
    }
  ]
}
### Paginación

La API admite paginación en varios endpoints. Para obtener resultados paginados, puedes agregar parámetros de consulta como pagina y tamano_pagina. Por ejemplo:

Obtener Pedidos Pagados: GET {{baseURL}}/api/pedidos?pagina=1&tamano_pagina=10

### Pruebas Automatizadas
Las pruebas automatizadas están implementadas utilizando xUnit.

Ejecutar Pruebas

Para ejecutar las pruebas, navega a la carpeta del proyecto de pruebas y ejecuta:
dotnet test

### Ejemplos de Pruebas

El controlador PedidosController incluye pruebas que aseguran que:

Se pueden crear pedidos con datos válidos.

Se pueden obtener pedidos por ID.

Se manejan correctamente los errores cuando se proporcionan datos inválidos.

[Fact]
public async Task CrearPedido_ConDatosValidos_DeberiaRetornarCreatedResult()
{
    // Arrange
    
    // Act
    var result = await _controller.CrearPedido(crearPedidoDto);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
    
}

# Sodimac Orders - Dashboard Angular

## Descripción
Dashboard web desarrollado en **Angular** para la gestión y visualización de entregas de Sodimac. Este proyecto consume la API de **Sodimac Orders** (.NET 9) y proporciona una interfaz intuitiva para el seguimiento de pedidos, entregas pendientes, completadas y urgentes.

## Características Principales
- **Dashboard de Entregas**: Visualización en tiempo real del estado de entregas
- **Gestión de Pedidos**: Crear, consultar y actualizar pedidos
- **Reportes**: Entregas pendientes, completadas y urgentes
- **Autenticación**: Sistema de login seguro
- **Paginación**: Navegación eficiente en grandes volúmenes de datos
- **Responsive Design**: Adaptable a diferentes dispositivos

## Tecnologías Utilizadas
- **Angular 17+**
- **TypeScript**
- **Bootstrap 5**
- **Angular Material** (opcional)
- **Chart.js** (para gráficos)
- **RxJS** (manejo de observables)

## 📋 Requisitos
- [Node.js](https://nodejs.org/) (versión 18 o superior)
- [Angular CLI](https://angular.io/cli) (versión 17+)
- [Git](https://git-scm.com/)

## Instalación

### 1. Clonar el Repositorio
```bash
git clone https://github.com/btorresdominguez/Sodimac-Orders-Angular.git
cd Sodimac-Orders-Angular
```

### 2. Instalar Dependencias
```bash
npm install
```

### 3. Configurar Variables de Entorno
Crear archivo `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7036/api',
  baseURL: 'https://localhost:7036'
};
```

### 4. Ejecutar la Aplicación
```bash
ng serve
```

La aplicación estará disponible en `http://localhost:4200`

## Estructura del Proyecto
```
src/
├── app/
│   ├── core/                 # Servicios core y guards
│   │   ├── services/
│   │   ├── guards/
│   │   └── interceptors/
│   ├── shared/               # Componentes compartidos
│   │   ├── components/
│   │   ├── models/
│   │   └── utils/
│   ├── features/             # Módulos de características
│   │   ├── dashboard/
│   │   ├── pedidos/
│   │   ├── reportes/
│   │   └── auth/
│   └── layouts/              # Layouts de la aplicación
├── assets/                   # Recursos estáticos
├── environments/             # Configuraciones de entorno
└── styles/                   # Estilos globales
```

##  Autenticación
### Login
```typescript
// Credenciales por defecto
{
  "email": "admin@sodimac.com",
  "contraseña": "123456"
}
```

## Funcionalidades del Dashboard

### Panel Principal
- **Entregas Pendientes**: `12` (Valor total: $906.155)
- **Entregas Completadas**: `0` (Valor entregado: $0)
- **Entregas Urgentes**: `4` (Para hoy y mañana)
- **Promedio por Entrega**: `$0` (Valor promedio completadas)

### Secciones

#### Entregas Urgentes
Lista de entregas críticas con:
- Información del cliente
- Dirección de entrega
- Valor del pedido
- Fecha y hora límite
- Ruta asignada

#### Próximas Entregas
Programación de entregas futuras con:
- Detalles del destinatario
- Ubicación
- Monto
- Fecha programada
- Estado de la ruta

## Endpoints de la API

### Autenticación
- `POST /api/auth/login` - Iniciar sesión

### Pedidos
- `GET /api/pedidos` - Obtener todos los pedidos
- `POST /api/pedidos` - Crear nuevo pedido
- `GET /api/pedidos/{id}` - Obtener pedido por ID
- `PUT /api/pedidos/{id}` - Actualizar pedido

### Reportes
- `GET /api/reportes/entregas-pendientes?pagina=1&tamano_pagina=30`
- `GET /api/reportes/entregas-completadas?pagina=1&tamano_pagina=40`
- `GET /api/reportes/entregas-urgentes`

### Ejemplo de Petición - Crear Pedido
```typescript
const nuevoPedido = {
  cliente_id: 1,
  ruta_id: 1,
  fecha_entrega: "2025-08-30T10:00:00",
  estado_pedido: "Pendiente",
  observaciones: "Urgente",
  productos: [
    {
      producto_id: 1,
      cantidad: 2,
      precio_unitario: 150.00
    }
  ]
};
```

## Servicios Angular

### PedidosService
```typescript
@Injectable({
  providedIn: 'root'
})
export class PedidosService {
  private apiUrl = `${environment.apiUrl}/pedidos`;
  
  obtenerPedidos(pagina: number = 1, tamano: number = 10): Observable<any> {
    return this.http.get(`${this.apiUrl}?pagina=${pagina}&tamano_pagina=${tamano}`);
  }
  
  crearPedido(pedido: any): Observable<any> {
    return this.http.post(this.apiUrl, pedido);
  }
}
```

### ReportesService
```typescript
@Injectable({
  providedIn: 'root'
})
export class ReportesService {
  private apiUrl = `${environment.apiUrl}/reportes`;
  
  obtenerEntregasPendientes(pagina: number = 1): Observable<any> {
    return this.http.get(`${this.apiUrl}/entregas-pendientes?pagina=${pagina}&tamano_pagina=30`);
  }
  
  obtenerEntregasCompletadas(pagina: number = 1): Observable<any> {
    return this.http.get(`${this.apiUrl}/entregas-completadas?pagina=${pagina}&tamano_pagina=40`);
  }
}
```

##  Desarrollo
- **Backend API**: .NET 9 + Docker
- **Frontend**: Angular 17+ + Bootstrap
- **Base de Datos**: SQL Server / PostgreSQL


---
**Nota**: Asegúrate de que la API de Sodimac Orders esté ejecutándose antes de iniciar la aplicación Angular.

