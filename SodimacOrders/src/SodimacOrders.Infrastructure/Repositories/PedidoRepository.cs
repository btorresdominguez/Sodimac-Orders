using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Common;
using SodimacOrders.Domain.Entities;
using SodimacOrders.Domain.Extensions;
using SodimacOrders.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly SodimacOrdersDbContext _context;
        private readonly ILogger<PedidoRepository> _logger;

        public PedidoRepository(SodimacOrdersDbContext context, ILogger<PedidoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Pedido?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.RutaEntrega)
                .Include(p => p.Productos)
                    .ThenInclude(dp => dp.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> ObtenerTodosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.RutaEntrega)
                .Include(p => p.Productos)
                    .ThenInclude(dp => dp.Producto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> ObtenerPorClienteAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.RutaEntrega)
                .Include(p => p.Productos)
                    .ThenInclude(dp => dp.Producto)
                .Where(p => p.cliente_id == clienteId)
                .OrderByDescending(p => p.fecha_pedido)
                .ToListAsync();
        }

        public async Task<Pedido> CrearAsync(Pedido pedido)
        {
            // Use the execution strategy to handle retries and transactions
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Set timestamps
                    pedido.fecha_pedido = DateTime.Now;
                    pedido.fecha_actualizacion = DateTime.Now;

                    // Calculate subtotals for order details
                    foreach (var detalle in pedido.Productos)
                    {
                        detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                        // Make sure PedidoId is not set yet (will be set after saving the order)
                        detalle.PedidoId = 0;
                    }

                    // Calculate total order value
                    pedido.valor_total = pedido.Productos.Sum(p => p.Subtotal);

                    _logger.LogInformation("Adding new order for client ID {ClienteId} with total value {ValorTotal}",
                        pedido.cliente_id, pedido.valor_total);

                    // Add the order first (without details)
                    var orderDetails = pedido.Productos.ToList(); // Store details temporarily
                    pedido.Productos.Clear(); // Clear the collection to avoid saving details with the order

                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync(); // Save the order first

                    _logger.LogInformation("Order created successfully with ID {OrderId}", pedido.Id);

                    // Now add order details with the correct PedidoId
                    foreach (var detalle in orderDetails)
                    {
                        // Create new entity instance to ensure clean state
                        var newDetalle = new DetallePedido
                        {
                            // DO NOT set Id - it will be auto-generated
                            PedidoId = pedido.Id,
                            ProductoId = detalle.ProductoId,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = detalle.PrecioUnitario,
                            Subtotal = detalle.Subtotal
                        };

                        _context.DetallesPedidos.Add(newDetalle);
                    }

                    await _context.SaveChangesAsync(); // Save order details
                    await transaction.CommitAsync();

                    _logger.LogInformation("Order details saved successfully for Order ID {OrderId}", pedido.Id);

                    // Reload the order with its details for return
                    var completedOrder = await _context.Pedidos
                        .Include(p => p.Productos)
                        .FirstOrDefaultAsync(p => p.Id == pedido.Id);

                    return completedOrder ?? pedido;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving order: {Message}. Inner exception: {InnerException}",
                        ex.Message, ex.InnerException?.Message);
                    throw;
                }
            });
        }

        public async Task<Pedido?> ActualizarAsync(int id, Pedido pedido)
        {
            var pedidoExistente = await _context.Pedidos
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedidoExistente == null)
                return null;

            // Actualizar propiedades
            pedidoExistente.cliente_id = pedido.cliente_id;
            pedidoExistente.ruta_id = pedido.ruta_id;
            pedidoExistente.fecha_entrega = pedido.fecha_entrega;
            pedidoExistente.estado_pedido = pedido.estado_pedido;
            pedidoExistente.observaciones = pedido.observaciones;
            pedidoExistente.fecha_actualizacion = DateTime.Now;

            // Actualizar productos (eliminar existentes y agregar nuevos)
            _context.DetallesPedidos.RemoveRange(pedidoExistente.Productos);

            foreach (var detalle in pedido.Productos)
            {
                detalle.PedidoId = id;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
            }

            pedidoExistente.Productos = pedido.Productos;
            pedidoExistente.valor_total = pedido.Productos.Sum(p => p.Subtotal);

            await _context.SaveChangesAsync();
            return pedidoExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return false;

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarEstadoAsync(int id, string estado, int? rutaId = null)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return false;

            pedido.estado_pedido = estado;
            if (rutaId.HasValue)
                pedido.ruta_id = rutaId;
            pedido.fecha_actualizacion = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReporteEntregasPendientesDto>> ObtenerEntregasPendientesAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.RutaEntrega)
                .Where(p => p.estado_pedido == "Pendiente" ||
                           p.estado_pedido == "Confirmado" ||
                           p.estado_pedido == "En_Preparacion" ||
                           p.estado_pedido == "En_Transito")
                .Select(p => new ReporteEntregasPendientesDto
                {
                    pedido_id = p.Id,
                    nombre_cliente = p.Cliente.nombre,
                    direccion_entrega = p.Cliente.direccion,
                    email = p.Cliente.email,
                    fecha_entrega = p.fecha_entrega,
                    valor_total = p.valor_total,
                    nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                    dias_para_entrega = EF.Functions.DateDiffDay(DateTime.Now, p.fecha_entrega)
                })
                .OrderBy(p => p.fecha_entrega)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReporteEntregasCompletadasDto>> ObtenerEntregasCompletadasAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.RutaEntrega)
                .Include(p => p.Productos)
                .Where(p => p.estado_pedido == "Entregado")
                .Select(p => new ReporteEntregasCompletadasDto
                {
                    pedido_id = p.Id,
                    nombre_cliente = p.Cliente.nombre,
                    direccion_entrega = p.Cliente.direccion,
                    fecha_pedido = p.fecha_pedido,
                    fecha_entrega = p.fecha_entrega,
                    valor_total = p.valor_total,
                    nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                    total_productos = p.Productos.Count(),
                    total_items = p.Productos.Sum(dp => dp.Cantidad)
                })
                .OrderByDescending(p => p.fecha_entrega)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Pedidos.AnyAsync(p => p.Id == id);
        }

          
            /// <summary>
            /// Obtiene pedidos con paginación y filtros
            /// </summary>
            public async Task<RespuestaPaginada<PedidoResponseDto>> ObtenerPedidosPaginados(
                ParametrosPaginacionPedidos parametros,
                string rutaBase)
            {
                var query = _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.RutaEntrega)
                    .Include(p => p.Productos)
                        .ThenInclude(dp => dp.Producto)
                    .AsQueryable();

                // Aplicar filtros
                query = AplicarFiltrosPedidos(query, parametros);

                // Proyectar a DTO
                var queryDto = query.Select(p => new PedidoResponseDto
                {
                    Id = p.Id,
                    cliente_id = p.cliente_id,
                    nombre_cliente = p.Cliente.nombre,
                    direccion_cliente = p.Cliente.direccion,
                    email_cliente = p.Cliente.email,
                    ruta_id = p.ruta_id,
                    nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                    fecha_pedido = p.fecha_pedido,
                    fecha_entrega = p.fecha_entrega,
                    estado_pedido = p.estado_pedido,
                    valor_total = p.valor_total,
                    observaciones = p.observaciones,
                    fecha_actualizacion = p.fecha_actualizacion,
                    productos = p.Productos.Select(dp => new DetallePedidoResponseDto
                    {
                        Id = dp.Id,
                        producto_id = dp.ProductoId,
                        nombre_producto = dp.Producto.nombre_producto,
                        codigo_producto = dp.Producto.codigo_producto,
                        cantidad = dp.Cantidad,
                        precio_unitario = dp.PrecioUnitario,
                        subtotal = dp.Subtotal
                    }).ToList()
                });

                return await queryDto.CrearRespuestaPaginada(parametros, rutaBase);
            }

            /// <summary>
            /// Obtiene pedidos de un cliente con paginación
            /// </summary>
            public async Task<RespuestaPaginada<PedidoResponseDto>> ObtenerPedidosClientePaginados(
                int clienteId,
                ParametrosPaginacion parametros,
                string rutaBase)
            {
                var query = _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.RutaEntrega)
                    .Include(p => p.Productos)
                        .ThenInclude(dp => dp.Producto)
                    .Where(p => p.cliente_id == clienteId)
                    .Select(p => new PedidoResponseDto
                    {
                        Id = p.Id,
                        cliente_id = p.cliente_id,
                        nombre_cliente = p.Cliente.nombre,
                        direccion_cliente = p.Cliente.direccion,
                        email_cliente = p.Cliente.email,
                        ruta_id = p.ruta_id,
                        nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                        fecha_pedido = p.fecha_pedido,
                        fecha_entrega = p.fecha_entrega,
                        estado_pedido = p.estado_pedido,
                        valor_total = p.valor_total,
                        observaciones = p.observaciones,
                        fecha_actualizacion = p.fecha_actualizacion,
                        productos = p.Productos.Select(dp => new DetallePedidoResponseDto
                        {
                            Id = dp.Id,
                            producto_id = dp.ProductoId,
                            nombre_producto = dp.Producto.nombre_producto,
                            codigo_producto = dp.Producto.codigo_producto,
                            cantidad = dp.Cantidad,
                            precio_unitario = dp.PrecioUnitario,
                            subtotal = dp.Subtotal
                        }).ToList()
                    });

                return await query.CrearRespuestaPaginada(parametros, rutaBase);
            }

            /// <summary>
            /// Obtiene entregas pendientes con paginación
            /// </summary>
            public async Task<RespuestaPaginada<ReporteEntregasPendientesDto>> ObtenerEntregasPendientesPaginadas(
                ParametrosPaginacionReportes parametros,
                string rutaBase)
            {
                var query = _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.RutaEntrega)
                    .Where(p => p.estado_pedido == "Pendiente" ||
                               p.estado_pedido == "Confirmado" ||
                               p.estado_pedido == "En_Preparacion" ||
                               p.estado_pedido == "En_Transito")
                    .AsQueryable();

                // Aplicar filtros específicos
                query = AplicarFiltrosReportes(query, parametros);

                var queryDto = query.Select(p => new ReporteEntregasPendientesDto
                {
                    pedido_id = p.Id,
                    nombre_cliente = p.Cliente.nombre,
                    direccion_entrega = p.Cliente.direccion,
                    email = p.Cliente.email,
                    fecha_entrega = p.fecha_entrega,
                    valor_total = p.valor_total,
                    nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                    dias_para_entrega = EF.Functions.DateDiffDay(DateTime.Now, p.fecha_entrega)
                });

                return await queryDto.CrearRespuestaPaginada(parametros, rutaBase);
            }

            /// <summary>
            /// Obtiene entregas completadas con paginación
            /// </summary>
            public async Task<RespuestaPaginada<ReporteEntregasCompletadasDto>> ObtenerEntregasCompletadasPaginadas(
                ParametrosPaginacionReportes parametros,
                string rutaBase)
            {
                var query = _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.RutaEntrega)
                    .Include(p => p.Productos)
                    .Where(p => p.estado_pedido == "Entregado")
                    .AsQueryable();

                // Aplicar filtros específicos
                query = AplicarFiltrosReportes(query, parametros);

                var queryDto = query.Select(p => new ReporteEntregasCompletadasDto
                {
                    pedido_id = p.Id,
                    nombre_cliente = p.Cliente.nombre,
                    direccion_entrega = p.Cliente.direccion,
                    fecha_pedido = p.fecha_pedido,
                    fecha_entrega = p.fecha_entrega,
                    valor_total = p.valor_total,
                    nombre_ruta = p.RutaEntrega != null ? p.RutaEntrega.nombre_ruta : null,
                    total_productos = p.Productos.Count(),
                    total_items = p.Productos.Sum(dp => dp.Cantidad)
                });

                return await queryDto.CrearRespuestaPaginada(parametros, rutaBase);
            }

            #region Métodos Privados para Filtros

            private IQueryable<Pedido> AplicarFiltrosPedidos(IQueryable<Pedido> query, ParametrosPaginacionPedidos parametros)
            {
                if (parametros.cliente_id.HasValue)
                    query = query.Where(p => p.cliente_id == parametros.cliente_id.Value);

                if (!string.IsNullOrEmpty(parametros.estado_pedido))
                    query = query.Where(p => p.estado_pedido == parametros.estado_pedido);

                if (parametros.ruta_id.HasValue)
                    query = query.Where(p => p.ruta_id == parametros.ruta_id.Value);

                if (parametros.fecha_desde.HasValue)
                    query = query.Where(p => p.fecha_pedido >= parametros.fecha_desde.Value);

                if (parametros.fecha_hasta.HasValue)
                    query = query.Where(p => p.fecha_pedido <= parametros.fecha_hasta.Value);

                if (parametros.valor_minimo.HasValue)
                    query = query.Where(p => p.valor_total >= parametros.valor_minimo.Value);

                if (parametros.valor_maximo.HasValue)
                    query = query.Where(p => p.valor_total <= parametros.valor_maximo.Value);

                if (!string.IsNullOrEmpty(parametros.filtro_busqueda))
                {
                    var filtro = parametros.filtro_busqueda.ToLower();
                    query = query.Where(p =>
                        p.Cliente.nombre.ToLower().Contains(filtro) ||
                        p.Cliente.email.ToLower().Contains(filtro) ||
                        (p.observaciones != null && p.observaciones.ToLower().Contains(filtro)));
                }

                return query;
            }

            private IQueryable<Pedido> AplicarFiltrosReportes(IQueryable<Pedido> query, ParametrosPaginacionReportes parametros)
            {
                if (parametros.fecha_entrega_desde.HasValue)
                    query = query.Where(p => p.fecha_entrega >= parametros.fecha_entrega_desde.Value);

                if (parametros.fecha_entrega_hasta.HasValue)
                    query = query.Where(p => p.fecha_entrega <= parametros.fecha_entrega_hasta.Value);

                if (parametros.ruta_id.HasValue)
                    query = query.Where(p => p.ruta_id == parametros.ruta_id.Value);

                if (!string.IsNullOrEmpty(parametros.nombre_cliente))
                {
                    var filtro = parametros.nombre_cliente.ToLower();
                    query = query.Where(p => p.Cliente.nombre.ToLower().Contains(filtro));
                }

                if (parametros.valor_minimo.HasValue)
                    query = query.Where(p => p.valor_total >= parametros.valor_minimo.Value);

                if (!string.IsNullOrEmpty(parametros.filtro_busqueda))
                {
                    var filtro = parametros.filtro_busqueda.ToLower();
                    query = query.Where(p =>
                        p.Cliente.nombre.ToLower().Contains(filtro) ||
                        p.Cliente.direccion.ToLower().Contains(filtro) ||
                        (p.RutaEntrega != null && p.RutaEntrega.nombre_ruta.ToLower().Contains(filtro)));
                }

                return query;
            }

        public async Task<DetallePedido?> ActualizarDetalleAsync(int id, DetallePedido detallePedido)
        {
            var detalleExistente = await _context.DetallesPedidos.FindAsync(id);
            if (detalleExistente == null)
            {
                return null; // Return null if detail doesn't exist
            }

            // Update properties
            detalleExistente.Cantidad = detallePedido.Cantidad;
            detalleExistente.PrecioUnitario = detallePedido.PrecioUnitario;
            detalleExistente.Subtotal = detallePedido.Cantidad * detallePedido.PrecioUnitario;

            await _context.SaveChangesAsync();
            return detalleExistente;
        }

        public async Task<bool> EliminarDetalleAsync(int id)
        {
            var detallePedido = await _context.DetallesPedidos.FindAsync(id);
            if (detallePedido == null)
            {
                return false; // Return false if detail doesn't exist
            }

            _context.DetallesPedidos.Remove(detallePedido);
            await _context.SaveChangesAsync();
            return true;
        }


        #endregion

    }

}
