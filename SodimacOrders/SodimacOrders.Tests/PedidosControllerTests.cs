using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using SodimacOrders.WebApi.Controllers;
using SodimacOrders.Domain.Entities;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;

namespace SodimacOrders.Tests
{
    public class PedidosControllerTests
    {
        private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<IRutaEntregaRepository> _rutaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<ILogger<PedidosController>> _loggerMock;
        private readonly PedidosController _controller;

        public PedidosControllerTests()
        {
            _pedidoRepositoryMock = new Mock<IPedidoRepository>();
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _rutaRepositoryMock = new Mock<IRutaEntregaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _loggerMock = new Mock<ILogger<PedidosController>>();

            _controller = new PedidosController(
                _pedidoRepositoryMock.Object,
                _clienteRepositoryMock.Object,
                _rutaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CrearPedido_ConDatosValidos_DeberiaRetornarCreatedResult()
        {
            // Arrange
            var crearPedidoDto = new CrearPedidoDto
            {
                cliente_id = 1,
                ruta_id = 1,
                fecha_entrega = DateTime.Now.AddDays(2),
                observaciones = "Pedido urgente",
                productos = new List<DetallePedidoDto>
                {
                    new DetallePedidoDto
                    {
                        producto_id = 1,
                        cantidad = 2,
                        precio_unitario = 150.00m
                    },
                    new DetallePedidoDto
                    {
                        producto_id = 2,
                        cantidad = 1,
                        precio_unitario = 300.00m
                    }
                }
            };

            // Mock de validaciones
            _clienteRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _rutaRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _productoRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var pedidoCreado = new Pedido
            {
                Id = 1,
                cliente_id = 1,
                ruta_id = 1,
                fecha_entrega = crearPedidoDto.fecha_entrega,
                estado_pedido = "Pendiente",
                observaciones = crearPedidoDto.observaciones,
                fecha_pedido = DateTime.Now,
                valor_total = 600.00m
            };

            _pedidoRepositoryMock
                .Setup(r => r.CrearAsync(It.IsAny<Pedido>()))
                .ReturnsAsync(pedidoCreado);

            var pedidoCompleto = new Pedido
            {
                Id = 1,
                cliente_id = 1,
                ruta_id = 1,
                fecha_entrega = crearPedidoDto.fecha_entrega,
                estado_pedido = "Pendiente",
                observaciones = crearPedidoDto.observaciones,
                fecha_pedido = DateTime.Now,
                valor_total = 600.00m,
                Cliente = new Cliente { nombre = "Cliente Test", direccion = "Dirección Test", email = "test@test.com" },
                RutaEntrega = new RutaEntrega { nombre_ruta = "Ruta Test" },
                Productos = new List<DetallePedido>
                {
                    new DetallePedido
                    {
                        Id = 1,
                        ProductoId = 1,
                        Cantidad = 2,
                        PrecioUnitario = 150.00m,
                        Producto = new Producto { nombre_producto = "Producto 1", codigo_producto = "P001" }
                    },
                    new DetallePedido
                    {
                        Id = 2,
                        ProductoId = 2,
                        Cantidad = 1,
                        PrecioUnitario = 300.00m,
                        Producto = new Producto { nombre_producto = "Producto 2", codigo_producto = "P002" }
                    }
                }
            };

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(pedidoCompleto);

            // Act
            var result = await _controller.CrearPedido(crearPedidoDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<PedidoResponseDto>(createdResult.Value);

            Assert.Equal(1, response.Id);
            Assert.Equal("Pendiente", response.estado_pedido);
            Assert.Equal(2, response.productos.Count);
            Assert.Equal(600.00m, response.valor_total);

            // Verificar que se llamaron los métodos del repositorio
            _pedidoRepositoryMock.Verify(r => r.CrearAsync(It.IsAny<Pedido>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.ExisteAsync(1), Times.Once);
            _rutaRepositoryMock.Verify(r => r.ExisteAsync(1), Times.Once);
            _productoRepositoryMock.Verify(r => r.ExisteAsync(It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ObtenerPedidoPorId_ConIdValido_DeberiaRetornarOkResult()
        {
            // Arrange
            int pedidoId = 1;
            var pedido = new Pedido
            {
                Id = pedidoId,
                cliente_id = 1,
                ruta_id = 1,
                fecha_entrega = DateTime.Now.AddDays(3),
                estado_pedido = "Confirmado",
                observaciones = "Test pedido",
                fecha_pedido = DateTime.Now.AddDays(-1),
                valor_total = 450.00m,
                Cliente = new Cliente
                {
                    Id = 1,
                    nombre = "Juan Pérez",
                    direccion = "Calle 123",
                    email = "juan@email.com"
                },
                RutaEntrega = new RutaEntrega
                {
                    Id = 1,
                    nombre_ruta = "Ruta Norte"
                },
                Productos = new List<DetallePedido>
                {
                    new DetallePedido
                    {
                        Id = 1,
                        ProductoId = 1,
                        Cantidad = 3,
                        PrecioUnitario = 150.00m,
                        Producto = new Producto
                        {
                            Id = 1,
                            nombre_producto = "Cemento",
                            codigo_producto = "CEM001"
                        }
                    }
                }
            };

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(pedidoId))
                .ReturnsAsync(pedido);

            // Act
            var result = await _controller.ObtenerPedidoPorId(pedidoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PedidoResponseDto>(okResult.Value);

            Assert.Equal(pedidoId, response.Id);
            Assert.Equal("Juan Pérez", response.nombre_cliente);
            Assert.Equal("Ruta Norte", response.nombre_ruta);
            Assert.Equal("Confirmado", response.estado_pedido);
            Assert.Single(response.productos);
            Assert.Equal("Cemento", response.productos.First().nombre_producto);

            // Verificar que se llamó el método del repositorio
            _pedidoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(pedidoId), Times.Once);
        }

        [Fact]
        public async Task ObtenerPedidoPorId_ConIdInexistente_DeberiaRetornarNotFound()
        {
            // Arrange
            int pedidoIdInexistente = 999;

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(pedidoIdInexistente))
                .ReturnsAsync((Pedido)null);

            // Act
            var result = await _controller.ObtenerPedidoPorId(pedidoIdInexistente);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"No se encontró el pedido con ID {pedidoIdInexistente}", notFoundResult.Value);

            // Verificar que se llamó el método del repositorio
            _pedidoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(pedidoIdInexistente), Times.Once);
        }

        [Fact]
        public async Task ActualizarPedido_ConDatosValidos_DeberiaRetornarOkResult()
        {
            // Arrange
            int pedidoId = 1;
            var actualizarPedidoDto = new ActualizarPedidoDto
            {
                cliente_id = 1,
                ruta_id = 2,
                fecha_entrega = DateTime.Now.AddDays(5),
                estado_pedido = "Confirmado",
                observaciones = "Actualización del pedido",
                productos = new List<DetallePedidoDto>
                {
                    new DetallePedidoDto
                    {
                        producto_id = 1,
                        cantidad = 5,
                        precio_unitario = 200.00m
                    }
                }
            };

            // Mock de validaciones
            _pedidoRepositoryMock
                .Setup(r => r.ExisteAsync(pedidoId))
                .ReturnsAsync(true);

            _clienteRepositoryMock
                .Setup(r => r.ExisteAsync(actualizarPedidoDto.cliente_id))
                .ReturnsAsync(true);

            _rutaRepositoryMock
                .Setup(r => r.ExisteAsync(actualizarPedidoDto.ruta_id.Value))
                .ReturnsAsync(true);

            _productoRepositoryMock
                .Setup(r => r.ExisteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var pedidoActualizado = new Pedido
            {
                Id = pedidoId,
                cliente_id = actualizarPedidoDto.cliente_id,
                ruta_id = actualizarPedidoDto.ruta_id,
                fecha_entrega = actualizarPedidoDto.fecha_entrega,
                estado_pedido = actualizarPedidoDto.estado_pedido,
                observaciones = actualizarPedidoDto.observaciones,
                valor_total = 1000.00m
            };

            _pedidoRepositoryMock
                .Setup(r => r.ActualizarAsync(pedidoId, It.IsAny<Pedido>()))
                .ReturnsAsync(pedidoActualizado);

            var pedidoCompleto = new Pedido
            {
                Id = pedidoId,
                cliente_id = actualizarPedidoDto.cliente_id,
                ruta_id = actualizarPedidoDto.ruta_id,
                fecha_entrega = actualizarPedidoDto.fecha_entrega,
                estado_pedido = actualizarPedidoDto.estado_pedido,
                observaciones = actualizarPedidoDto.observaciones,
                valor_total = 1000.00m,
                Cliente = new Cliente { nombre = "Cliente Actualizado", direccion = "Nueva Dirección", email = "nuevo@email.com" },
                RutaEntrega = new RutaEntrega { nombre_ruta = "Ruta Actualizada" },
                Productos = new List<DetallePedido>
                {
                    new DetallePedido
                    {
                        Id = 1,
                        ProductoId = 1,
                        Cantidad = 5,
                        PrecioUnitario = 200.00m,
                        Producto = new Producto { nombre_producto = "Producto Actualizado", codigo_producto = "P001" }
                    }
                }
            };

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoCompleto);

            // Act
            var result = await _controller.ActualizarPedido(pedidoId, actualizarPedidoDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PedidoResponseDto>(okResult.Value);

            Assert.Equal(pedidoId, response.Id);
            Assert.Equal("Confirmado", response.estado_pedido);
            Assert.Equal("Actualización del pedido", response.observaciones);
            Assert.Equal(1000.00m, response.valor_total);

            // Verificar que se llamaron los métodos del repositorio
            _pedidoRepositoryMock.Verify(r => r.ExisteAsync(pedidoId), Times.Once);
            _pedidoRepositoryMock.Verify(r => r.ActualizarAsync(pedidoId, It.IsAny<Pedido>()), Times.Once);
            _clienteRepositoryMock.Verify(r => r.ExisteAsync(actualizarPedidoDto.cliente_id), Times.Once);
            _rutaRepositoryMock.Verify(r => r.ExisteAsync(actualizarPedidoDto.ruta_id.Value), Times.Once);
        }

        [Fact]
        public async Task EliminarPedido_ConIdValido_DeberiaRetornarNoContent()
        {
            // Arrange
            int pedidoId = 1;
            var pedidoExistente = new Pedido
            {
                Id = pedidoId,
                estado_pedido = "Pendiente" // Estado válido para eliminar
            };

            _pedidoRepositoryMock
                .Setup(r => r.ExisteAsync(pedidoId))
                .ReturnsAsync(true);

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoExistente);

            _pedidoRepositoryMock
                .Setup(r => r.EliminarAsync(pedidoId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.EliminarPedido(pedidoId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verificar que se llamaron los métodos del repositorio
            _pedidoRepositoryMock.Verify(r => r.ExisteAsync(pedidoId), Times.Once);
            _pedidoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(pedidoId), Times.Once);
            _pedidoRepositoryMock.Verify(r => r.EliminarAsync(pedidoId), Times.Once);
        }

        [Fact]
        public async Task EliminarPedido_ConPedidoEntregado_DeberiaRetornarBadRequest()
        {
            // Arrange
            int pedidoId = 1;
            var pedidoEntregado = new Pedido
            {
                Id = pedidoId,
                estado_pedido = "Entregado" // Estado que no permite eliminación
            };

            _pedidoRepositoryMock
                .Setup(r => r.ExisteAsync(pedidoId))
                .ReturnsAsync(true);

            _pedidoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoEntregado);

            // Act
            var result = await _controller.EliminarPedido(pedidoId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No se puede eliminar un pedido que ya fue entregado", badRequestResult.Value);

            // Verificar que NO se llamó el método de eliminación
            _pedidoRepositoryMock.Verify(r => r.EliminarAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CrearPedido_ConClienteInexistente_DeberiaRetornarBadRequest()
        {
            // Arrange
            var crearPedidoDto = new CrearPedidoDto
            {
                cliente_id = 999, // Cliente inexistente
                fecha_entrega = DateTime.Now.AddDays(2),
                productos = new List<DetallePedidoDto>
                {
                    new DetallePedidoDto { producto_id = 1, cantidad = 1, precio_unitario = 100.00m }
                }
            };

            _clienteRepositoryMock
                .Setup(r => r.ExisteAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CrearPedido(crearPedidoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal($"El cliente con ID {crearPedidoDto.cliente_id} no existe", badRequestResult.Value);
        }

        [Fact]
        public async Task CrearPedido_ConFechaEntregaPasada_DeberiaRetornarBadRequest()
        {
            // Arrange
            var crearPedidoDto = new CrearPedidoDto
            {
                cliente_id = 1,
                fecha_entrega = DateTime.Now.AddDays(-1), // Fecha pasada
                productos = new List<DetallePedidoDto>
                {
                    new DetallePedidoDto { producto_id = 1, cantidad = 1, precio_unitario = 100.00m }
                }
            };

            _clienteRepositoryMock
                .Setup(r => r.ExisteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CrearPedido(crearPedidoDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("La fecha de entrega debe ser futura", badRequestResult.Value);
        }
    }
}