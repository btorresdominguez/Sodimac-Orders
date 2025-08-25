using SodimacOrders.Domain.Entities.SodimacOrders.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    [Table("Roles")]
    public class Role
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreRol { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;

        // Relaciones
        public ICollection<UserRol> UsuarioRoles { get; set; } = new List<UserRol>();

    }
}
