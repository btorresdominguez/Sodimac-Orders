using SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.Interfaces
{
    public interface IRutaEntregaRepository
    {
        Task<RutaEntrega?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<RutaEntrega>> ObtenerActivasAsync();
        Task<bool> ExisteAsync(int id);
    }
}
