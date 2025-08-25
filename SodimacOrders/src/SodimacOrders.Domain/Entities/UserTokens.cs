using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    [Table("UsuarioTokens")]
    public class UserTokens
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }  // FK

        public string TokenValor { get; set; } = null!;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }

        // Navegación
        public User Usuario { get; set; } = null!;
    }

}
