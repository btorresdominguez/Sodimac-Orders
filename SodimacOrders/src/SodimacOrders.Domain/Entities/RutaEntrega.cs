using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    [Table("DeliveryRoutes")]
    public class RutaEntrega
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre_ruta")]
        public string nombre_ruta { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string estado { get; set; } = "Activa";

        [Column("capacidad_maxima")]
        public int capacidad_maxima { get; set; } = 50;

        [Column("fecha_creacion")]
        public DateTime fecha_creacion { get; set; } = DateTime.Now;

        public bool activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
