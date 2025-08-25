using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    public class UserRol
    {
        public int IdUsuario { get; set; }
        public required User Usuario { get; set; } // Ensure this points to the correct Usuario entity

        public int IdRol { get; set; }
        public required Role Rol { get; set; } // Ensure this points to the correct Rol entity
    }
}
