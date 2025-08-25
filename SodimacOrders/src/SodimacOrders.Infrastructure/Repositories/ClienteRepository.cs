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
    public class ClienteRepository : IClienteRepository
    {
        private readonly SodimacOrdersDbContext _context;

        public ClienteRepository(SodimacOrdersDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes
                .Where(c => c.activo)
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Clientes.AnyAsync(c => c.Id == id && c.activo);
        }

        public async Task<Cliente?> ObtenerPorEmailAsync(string email)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.email == email && c.activo);
        }
    }
}
