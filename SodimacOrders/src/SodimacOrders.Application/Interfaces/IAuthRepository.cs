using SodimacOrders.Domain.Entities;
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.Interfaces
{
    public interface IAuthRepository
    {
        /// <summary>
        /// Obtiene un usuario por su email
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User?> GetUserByEmailAsync(string email, string password);

        Task SaveToken(int idUsuario, string token);


    }
}