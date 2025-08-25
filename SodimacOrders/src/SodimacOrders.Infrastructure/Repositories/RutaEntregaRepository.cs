using Microsoft.EntityFrameworkCore;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Entities;
using SodimacOrders.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Infrastructure.Repositories
{
    public class RutaEntregaRepository : IRutaEntregaRepository
    {
        private readonly SodimacOrdersDbContext _context;

        public RutaEntregaRepository(SodimacOrdersDbContext context)
        {
            _context = context;
        }

        public async Task<RutaEntrega?> ObtenerPorIdAsync(int id)
        {
            return await _context.RutasEntrega.FindAsync(id);
        }

        public async Task<IEnumerable<RutaEntrega>> ObtenerActivasAsync()
        {
            return await _context.RutasEntrega
                .Where(r => r.activo && r.estado == "Activa")
                .OrderBy(r => r.nombre_ruta)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.RutasEntrega.AnyAsync(r => r.Id == id && r.activo);
        }
    }
}
