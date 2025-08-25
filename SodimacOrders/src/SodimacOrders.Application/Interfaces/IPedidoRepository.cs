using SodimacOrders.Application.DTOs;
using SodimacOrders.Domain.Common;
using SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.Interfaces
{
    public interface IPedidoRepository
    {
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Pedido>> ObtenerTodosAsync();
        Task<IEnumerable<Pedido>> ObtenerPorClienteAsync(int clienteId);
        Task<Pedido> CrearAsync(Pedido pedido);
        Task<Pedido?> ActualizarAsync(int id, Pedido pedido);
        Task<bool> EliminarAsync(int id);
        Task<bool> ActualizarEstadoAsync(int id, string estado, int? rutaId = null);
        Task<IEnumerable<ReporteEntregasPendientesDto>> ObtenerEntregasPendientesAsync();
        Task<IEnumerable<ReporteEntregasCompletadasDto>> ObtenerEntregasCompletadasAsync();
        Task<bool> ExisteAsync(int id);

        Task<RespuestaPaginada<PedidoResponseDto>> ObtenerPedidosPaginados(
            ParametrosPaginacionPedidos parametros,
            string rutaBase);

        Task<RespuestaPaginada<PedidoResponseDto>> ObtenerPedidosClientePaginados(
            int clienteId,
            ParametrosPaginacion parametros,
            string rutaBase);

        Task<RespuestaPaginada<ReporteEntregasPendientesDto>> ObtenerEntregasPendientesPaginadas(
            ParametrosPaginacionReportes parametros,
            string rutaBase);

        Task<RespuestaPaginada<ReporteEntregasCompletadasDto>> ObtenerEntregasCompletadasPaginadas(
            ParametrosPaginacionReportes parametros,
            string rutaBase);


        Task<DetallePedido?> ActualizarDetalleAsync(int id, DetallePedido detallePedido);
        Task<bool> EliminarDetalleAsync(int id);
    }
}
