using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    [Table("Clients")]
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Column("fecha_creacion")]
        public DateTime fecha_creacion { get; set; } = DateTime.Now;

        public bool activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

}
