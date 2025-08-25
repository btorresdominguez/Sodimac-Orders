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
    public class ProductoRepository : IProductoRepository
    {
        private readonly SodimacOrdersDbContext _context;

        public ProductoRepository(SodimacOrdersDbContext context)
        {
            _context = context;
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .Where(p => p.activo)
                .OrderBy(p => p.nombre_producto)
                .ToListAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Productos.AnyAsync(p => p.Id == id && p.activo);
        }
    }
}
