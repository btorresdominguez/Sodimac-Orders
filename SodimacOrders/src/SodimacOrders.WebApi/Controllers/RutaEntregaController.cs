using Microsoft.AspNetCore.Mvc;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SodimacOrders.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RutaEntregaController : ControllerBase
    {
        private readonly IRutaEntregaRepository _rutaEntregaRepository;

        public RutaEntregaController(IRutaEntregaRepository rutaEntregaRepository)
        {
            _rutaEntregaRepository = rutaEntregaRepository;
        }

        /// <summary>
        /// Obtiene todas las rutas de entrega activas.
        /// </summary>
        /// <returns>Lista de rutas activas.</returns>
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<RutaEntrega>>> ObtenerRutasActivas()
        {
            var rutasActivas = await _rutaEntregaRepository.ObtenerActivasAsync();
            return Ok(rutasActivas);
        }

     
    }
}