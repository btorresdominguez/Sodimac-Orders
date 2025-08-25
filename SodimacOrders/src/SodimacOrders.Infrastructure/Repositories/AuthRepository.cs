using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SodimacOrders.Application.Interfaces;
using SodimacOrders.Domain.Entities;
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using SodimacOrders.Infrastructure.Data;

namespace SodimacOrders.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SodimacOrdersDbContext _context;
        private readonly ILogger<AuthRepository> _logger;  

        public AuthRepository(
            SodimacOrdersDbContext context,
            ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
           
        }

        // Método mejorado sin raw SQL, usando Entity Framework y BCrypt
        public async Task<User?> GetUserByEmailAsync(string email, string password)
        {
            _logger.LogDebug("Searching for user by email: {Email}", email);
            try
            {
                var user = await _context.Usuarios
                    .AsNoTracking()
                    .Include(u => u.UsuarioRoles)
                        .ThenInclude(ur => ur.Rol)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Contraseña))
                {
                    // Llenar la propiedad Role manualmente
                    user.Role = user.UsuarioRoles
                        .Where(ur => ur.Rol != null)
                        .Select(ur => ur.Rol.NombreRol)
                        .ToList();

                    _logger.LogDebug("User found with {RoleCount} roles", user.Role.Count);
                    return user;
                }
                else
                {
                    _logger.LogDebug("No user found with email: {Email} or password verification failed", email);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for user by email: {Email}", email);
                throw;
            }
        }

        public async Task SaveToken(int idUsuario, string token)
        {
            try
            {
                // Verificar todos los usuarios que existen
                var todosLosUsuarios = await _context.Usuarios
                    .Select(u => new { u.IdUsuario, u.Email })
                    .ToListAsync();

                Console.WriteLine($"Usuarios en BD: {string.Join(", ", todosLosUsuarios.Select(u => $"ID:{u.IdUsuario} Email:{u.Email}"))}");
                Console.WriteLine($"Buscando usuario con ID: {idUsuario}");

                // Verificar que el usuario existe
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario);
                if (!usuarioExiste)
                {
                    throw new Exception($"El usuario con ID {idUsuario} no existe. Usuarios disponibles: {string.Join(", ", todosLosUsuarios.Select(u => u.IdUsuario))}");
                }

                _context.UsuarioTokens.Add(new UserTokens
                {
                    IdUsuario = idUsuario,
                    TokenValor = token,
                    FechaExpiracion = DateTime.UtcNow.AddHours(2)
                });

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completo: {ex}");
                throw;
            }
        }
    }
}