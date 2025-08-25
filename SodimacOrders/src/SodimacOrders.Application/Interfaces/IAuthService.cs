using SodimacOrders.Application.DTOs;
using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);

    }
}
