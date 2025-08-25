using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using SodimacOrders.Application.DTOs;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Common;

using SodimacOrders.Domain.Entities;

/// <summary>
/// Controlador actualizado con paginación para reportes de entregas de Sodimac Colombia
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportesController : ControllerBase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IPedidoRepository pedidoRepository, ILogger<ReportesController> logger)
    {
        _pedidoRepository = pedidoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el reporte de entregas pendientes con paginación y filtros
    /// </summary>
    /// <param name="parametros">Parámetros de filtros y paginación</param>
    /// <returns>Lista paginada de entregas pendientes</returns>
    [HttpGet("entregas-pendientes")]
    [ProducesResponseType(typeof(RespuestaPaginada<ReporteEntregasPendientesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaPaginada<ReporteEntregasPendientesDto>>> ObtenerEntregasPendientes(
        [FromQuery] ParametrosPaginacionReportes parametros)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rutaBase = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var entregas = await _pedidoRepository.ObtenerEntregasPendientesPaginadas(parametros, rutaBase);

            // Agregar estadísticas al response
            var estadisticas = new
            {
                total_pendientes = entregas.total_registros,
                entregas_urgentes = entregas.datos.Count(e => e.dias_para_entrega <= 1),
                entregas_proximas = entregas.datos.Count(e => e.dias_para_entrega <= 3),
                valor_total_pendiente = entregas.datos.Sum(e => e.valor_total)
            };

            var responseConEstadisticas = new
            {
                estadisticas,
                entregas
            };

            _logger.LogInformation(
                "Reporte entregas pendientes generado: {Count}/{Total} (página {Pagina})",
                entregas.datos.Count(),
                entregas.total_registros,
                entregas.pagina_actual);

            return Ok(responseConEstadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de entregas pendientes paginadas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene el reporte de entregas completadas con paginación y filtros
    /// </summary>
    /// <param name="parametros">Parámetros de filtros y paginación</param>
    /// <returns>Lista paginada de entregas completadas</returns>
    [HttpGet("entregas-completadas")]
    [ProducesResponseType(typeof(RespuestaPaginada<ReporteEntregasCompletadasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RespuestaPaginada<ReporteEntregasCompletadasDto>>> ObtenerEntregasCompletadas(
        [FromQuery] ParametrosPaginacionReportes parametros)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rutaBase = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var entregas = await _pedidoRepository.ObtenerEntregasCompletadasPaginadas(parametros, rutaBase);

            // Agregar estadísticas al response
            var estadisticas = new
            {
                total_completadas = entregas.total_registros,
                valor_total_entregado = entregas.datos.Sum(e => e.valor_total),
                total_productos_entregados = entregas.datos.Sum(e => e.total_productos),
                total_items_entregados = entregas.datos.Sum(e => e.total_items),
                promedio_valor_por_entrega = entregas.datos.Any() ? entregas.datos.Average(e => e.valor_total) : 0
            };

            var responseConEstadisticas = new
            {
                estadisticas,
                entregas
            };

            _logger.LogInformation(
                "Reporte entregas completadas generado: {Count}/{Total} (página {Pagina})",
                entregas.datos.Count(),
                entregas.total_registros,
                entregas.pagina_actual);

            return Ok(responseConEstadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de entregas completadas paginadas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Dashboard con resumen estadístico sin paginación
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ObtenerDashboard()
    {
        try
        {
            // Obtener datos para dashboard (sin paginación, solo estadísticas)
            var parametrosBasicos = new ParametrosPaginacionReportes { tamano_pagina = 1 };
            var rutaBase = "";

            var pendientes = await _pedidoRepository.ObtenerEntregasPendientesPaginadas(parametrosBasicos, rutaBase);
            var completadas = await _pedidoRepository.ObtenerEntregasCompletadasPaginadas(parametrosBasicos, rutaBase);

            var dashboard = new
            {
                fecha_reporte = DateTime.Now,
                resumen_entregas = new
                {
                    total_pendientes = pendientes.total_registros,
                    total_completadas = completadas.total_registros,
                    total_general = pendientes.total_registros + completadas.total_registros
                },
                estadisticas_tiempo = new
                {
                    entregas_urgentes_hoy = pendientes.datos.Count(e => e.dias_para_entrega == 0),
                    entregas_manana = pendientes.datos.Count(e => e.dias_para_entrega == 1),
                    entregas_esta_semana = pendientes.datos.Count(e => e.dias_para_entrega <= 7)
                }
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar dashboard");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    
}