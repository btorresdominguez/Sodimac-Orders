using Azure.Core;
using Azure;
using Microsoft.AspNetCore.Mvc;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Common;

using SodimacOrders.Domain.Entities;
using SodimacOrders.Domain.Enums;

/// <summary>
/// Controlador para gestión de pedidos de Sodimac Colombia
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IRutaEntregaRepository _rutaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(
        IPedidoRepository pedidoRepository,
        IClienteRepository clienteRepository,
        IRutaEntregaRepository rutaRepository,
        IProductoRepository productoRepository,
        ILogger<PedidosController> logger)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _rutaRepository = rutaRepository;
        _productoRepository = productoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los pedidos con paginación y filtros avanzados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RespuestaPaginada<PedidoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaPaginada<PedidoResponseDto>>> ObtenerTodosPedidos(
        [FromQuery] ParametrosPaginacionPedidos parametros)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rutaBase = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pedidosPaginados = await _pedidoRepository.ObtenerPedidosPaginados(parametros, rutaBase);

            Response.Headers.Add("X-Pagination",
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    pedidosPaginados.pagina_actual,
                    pedidosPaginados.tamano_pagina,
                    pedidosPaginados.total_registros,
                    pedidosPaginados.total_paginas,
                    pedidosPaginados.tiene_pagina_anterior,
                    pedidosPaginados.tiene_pagina_siguiente
                }));

            _logger.LogInformation(
                "Se obtuvieron {Count} pedidos de {Total} (página {Pagina} de {TotalPaginas})",
                pedidosPaginados.datos.Count(),
                pedidosPaginados.total_registros,
                pedidosPaginados.pagina_actual,
                pedidosPaginados.total_paginas);

            return Ok(pedidosPaginados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pedidos paginados con filtros: {@Parametros}", parametros);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene un pedido específico por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponseDto>> ObtenerPedidoPorId([FromRoute] int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);

            if (pedido == null)
            {
                _logger.LogWarning("No se encontró el pedido con ID {Id}", id);
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            var response = MapearPedidoAResponseDto(pedido);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// NUEVO: Consulta el estado específico de un pedido
    /// </summary>
    [HttpGet("{id}/estado")]
    [ProducesResponseType(typeof(EstadoPedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EstadoPedidoResponseDto>> ConsultarEstadoPedido([FromRoute] int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);

            if (pedido == null)
            {
                _logger.LogWarning("No se encontró el pedido con ID {Id}", id);
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            var response = new EstadoPedidoResponseDto
            {
                pedido_id = pedido.Id,
                estado_actual = pedido.estado_pedido,
                fecha_pedido = pedido.fecha_pedido,
                fecha_entrega_programada = pedido.fecha_entrega,
                fecha_ultima_actualizacion = pedido.fecha_actualizacion,
                ruta_asignada = pedido.ruta_id,
                nombre_ruta = pedido.RutaEntrega?.nombre_ruta,
                observaciones = pedido.observaciones
            };

            _logger.LogInformation("Consulta de estado para pedido {Id}: {Estado}", id, pedido.estado_pedido);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar estado del pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// NUEVO: Asignar pedido a una ruta logística
    /// </summary>
    [HttpPatch("{id}/asignar-ruta")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponseDto>> AsignarRutaLogistica(
        [FromRoute] int id,
        [FromBody] AsignarRutaDto asignarRutaDto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el pedido existe
            var pedidoExiste = await _pedidoRepository.ExisteAsync(id);
            if (!pedidoExiste)
            {
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            // Verificar que la ruta existe
            var rutaExiste = await _rutaRepository.ExisteAsync(asignarRutaDto.ruta_id);
            if (!rutaExiste)
            {
                return BadRequest($"La ruta con ID {asignarRutaDto.ruta_id} no existe");
            }

            // Validar que el pedido esté en estado válido para asignar ruta
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            var estadosValidosParaAsignacion = new[] { "Pendiente", "Confirmado" };
            if (!estadosValidosParaAsignacion.Contains(pedido.estado_pedido))
            {
                return BadRequest($"No se puede asignar ruta a un pedido en estado '{pedido.estado_pedido}'. Estados válidos: {string.Join(", ", estadosValidosParaAsignacion)}");
            }

            // Asignar ruta y cambiar estado si es necesario
            var nuevoEstado = pedido.estado_pedido == "Pendiente" ? "Confirmado" : pedido.estado_pedido;
            var actualizado = await _pedidoRepository.ActualizarEstadoAsync(id, nuevoEstado, asignarRutaDto.ruta_id);

            if (!actualizado)
            {
                return StatusCode(500, "Error al asignar la ruta al pedido");
            }

            // Obtener el pedido actualizado
            var pedidoActualizado = await _pedidoRepository.ObtenerPorIdAsync(id);
            var response = MapearPedidoAResponseDto(pedidoActualizado!);

            _logger.LogInformation("Ruta {RutaId} asignada al pedido {PedidoId}", asignarRutaDto.ruta_id, id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar ruta al pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crear nuevo pedido
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponseDto>> CrearPedido([FromBody] CrearPedidoDto crearPedidoDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validaciones
            var clienteExiste = await _clienteRepository.ExisteAsync(crearPedidoDto.cliente_id);
            if (!clienteExiste)
            {
                return BadRequest($"El cliente con ID {crearPedidoDto.cliente_id} no existe");
            }

            if (crearPedidoDto.ruta_id.HasValue)
            {
                var rutaExiste = await _rutaRepository.ExisteAsync(crearPedidoDto.ruta_id.Value);
                if (!rutaExiste)
                {
                    return BadRequest($"La ruta con ID {crearPedidoDto.ruta_id} no existe");
                }
            }

            foreach (var detalle in crearPedidoDto.productos)
            {
                var productoExiste = await _productoRepository.ExisteAsync(detalle.producto_id);
                if (!productoExiste)
                {
                    return BadRequest($"El producto con ID {detalle.producto_id} no existe");
                }
            }

            if (crearPedidoDto.fecha_entrega <= DateTime.Now)
            {
                return BadRequest("La fecha de entrega debe ser futura");
            }

            var pedido = new Pedido
            {
                cliente_id = crearPedidoDto.cliente_id,
                ruta_id = crearPedidoDto.ruta_id,
                fecha_entrega = crearPedidoDto.fecha_entrega,
                observaciones = crearPedidoDto.observaciones,
                estado_pedido = "Pendiente",
                Productos = crearPedidoDto.productos.Select(p => new DetallePedido
                {
                    ProductoId = p.producto_id,
                    Cantidad = p.cantidad,
                    PrecioUnitario = p.precio_unitario
                }).ToList()
            };

            var pedidoCreado = await _pedidoRepository.CrearAsync(pedido);
            var pedidoCompleto = await _pedidoRepository.ObtenerPorIdAsync(pedidoCreado.Id);
            var response = MapearPedidoAResponseDto(pedidoCompleto!);

            _logger.LogInformation("Pedido creado exitosamente con ID {Id}", pedidoCreado.Id);
            return CreatedAtAction(nameof(ObtenerPedidoPorId), new { id = pedidoCreado.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el pedido");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar pedido completo - CORREGIDO para traer información relacionada
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponseDto>> ActualizarPedido(
        [FromRoute] int id,
        [FromBody] ActualizarPedidoDto actualizarPedidoDto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // CORRECCIÓN 1: Validar estados usando enum
            var estadosPermitidos = Enum.GetNames<EstadoPedido>();
            if (!estadosPermitidos.Contains(actualizarPedidoDto.estado_pedido))
            {
                return BadRequest($"Estado '{actualizarPedidoDto.estado_pedido}' no es válido. Estados permitidos: {string.Join(", ", estadosPermitidos)}");
            }

            // Verificaciones de existencia
            var pedidoExiste = await _pedidoRepository.ExisteAsync(id);
            if (!pedidoExiste)
            {
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            var clienteExiste = await _clienteRepository.ExisteAsync(actualizarPedidoDto.cliente_id);
            if (!clienteExiste)
            {
                return BadRequest($"El cliente con ID {actualizarPedidoDto.cliente_id} no existe");
            }

            if (actualizarPedidoDto.ruta_id.HasValue)
            {
                var rutaExiste = await _rutaRepository.ExisteAsync(actualizarPedidoDto.ruta_id.Value);
                if (!rutaExiste)
                {
                    return BadRequest($"La ruta con ID {actualizarPedidoDto.ruta_id} no existe");
                }
            }

            foreach (var detalle in actualizarPedidoDto.productos)
            {
                var productoExiste = await _productoRepository.ExisteAsync(detalle.producto_id);
                if (!productoExiste)
                {
                    return BadRequest($"El producto con ID {detalle.producto_id} no existe");
                }
            }

            // Validación de fecha flexible
            if (actualizarPedidoDto.estado_pedido == "Pendiente" && actualizarPedidoDto.fecha_entrega <= DateTime.Now)
            {
                return BadRequest("La fecha de entrega debe ser futura para pedidos pendientes");
            }

            var pedidoActualizado = new Pedido
            {
                cliente_id = actualizarPedidoDto.cliente_id,
                ruta_id = actualizarPedidoDto.ruta_id,
                fecha_entrega = actualizarPedidoDto.fecha_entrega,
                observaciones = actualizarPedidoDto.observaciones,
                estado_pedido = actualizarPedidoDto.estado_pedido,
                Productos = actualizarPedidoDto.productos.Select(p => new DetallePedido
                {
                    ProductoId = p.producto_id,
                    Cantidad = p.cantidad,
                    PrecioUnitario = p.precio_unitario
                }).ToList()
            };

            var resultado = await _pedidoRepository.ActualizarAsync(id, pedidoActualizado);
            if (resultado == null)
            {
                return NotFound($"No se pudo actualizar el pedido con ID {id}");
            }

            //  Obtener pedido completo con todas las relaciones
            var pedidoCompleto = await _pedidoRepository.ObtenerPorIdAsync(id);
            var response = MapearPedidoAResponseDto(pedidoCompleto!);

            _logger.LogInformation("Pedido {Id} actualizado exitosamente", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar solo el estado de un pedido 
    /// </summary>
    [HttpPatch("{id}/estado")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PedidoResponseDto>> ActualizarEstadoPedido(
        [FromRoute] int id,
        [FromBody] ActualizarEstadoPedidoDto actualizarEstadoDto)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // CORRECCIÓN: Usar enum para validar estados
            var estadosPermitidos = Enum.GetNames<EstadoPedido>();
            if (!estadosPermitidos.Contains(actualizarEstadoDto.estado_pedido))
            {
                return BadRequest($"Estado no válido. Estados permitidos: {string.Join(", ", estadosPermitidos)}");
            }

            if (actualizarEstadoDto.ruta_id.HasValue)
            {
                var rutaExiste = await _rutaRepository.ExisteAsync(actualizarEstadoDto.ruta_id.Value);
                if (!rutaExiste)
                {
                    return BadRequest($"La ruta con ID {actualizarEstadoDto.ruta_id} no existe");
                }
            }

            var actualizado = await _pedidoRepository.ActualizarEstadoAsync(id, actualizarEstadoDto.estado_pedido, actualizarEstadoDto.ruta_id);
            if (!actualizado)
            {
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            var pedidoActualizado = await _pedidoRepository.ObtenerPorIdAsync(id);
            var response = MapearPedidoAResponseDto(pedidoActualizado!);

            _logger.LogInformation("Estado del pedido {Id} actualizado a {Estado}", id, actualizarEstadoDto.estado_pedido);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el estado del pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar pedido - eliminar pedido y sus detalles
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EliminarPedido([FromRoute] int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest("El ID del pedido debe ser mayor a 0");
            }

            var pedidoExiste = await _pedidoRepository.ExisteAsync(id);
            if (!pedidoExiste)
            {
                return NotFound($"No se encontró el pedido con ID {id}");
            }

            // Verificar estado antes de eliminar
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            if (pedido?.estado_pedido == "Entregado")
            {
                return BadRequest("No se puede eliminar un pedido que ya fue entregado");
            }

            // El repositorio debe manejar la eliminación en cascada de los detalles
            var eliminado = await _pedidoRepository.EliminarAsync(id);
            if (!eliminado)
            {
                return StatusCode(500, $"No se pudo eliminar el pedido con ID {id}");
            }

            _logger.LogInformation("Pedido {Id} y sus detalles eliminados exitosamente", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el pedido {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Mapea una entidad Pedido a PedidoResponseDto -
    /// </summary>
    private static PedidoResponseDto MapearPedidoAResponseDto(Pedido pedido)
    {
        return new PedidoResponseDto
        {
            Id = pedido.Id,
            cliente_id = pedido.cliente_id,
            nombre_cliente = pedido.Cliente?.nombre ?? string.Empty,
            direccion_cliente = pedido.Cliente?.direccion ?? string.Empty,
            email_cliente = pedido.Cliente?.email ?? string.Empty,
            ruta_id = pedido.ruta_id,
            nombre_ruta = pedido.RutaEntrega?.nombre_ruta,
            fecha_pedido = pedido.fecha_pedido,
            fecha_entrega = pedido.fecha_entrega,
            estado_pedido = pedido.estado_pedido,
            valor_total = pedido.valor_total,
            observaciones = pedido.observaciones,
            fecha_actualizacion = pedido.fecha_actualizacion,
            productos = pedido.Productos?.Select(dp => new DetallePedidoResponseDto
            {
                Id = dp.Id,
                producto_id = dp.ProductoId,
                nombre_producto = dp.Producto?.nombre_producto ?? string.Empty,
                codigo_producto = dp.Producto?.codigo_producto ?? string.Empty,
                cantidad = dp.Cantidad,
                precio_unitario = dp.PrecioUnitario,
                subtotal = dp.Subtotal
            }).ToList() ?? new List<DetallePedidoResponseDto>()
        };
    }
}
