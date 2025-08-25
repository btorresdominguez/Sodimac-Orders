using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Entities
{
    [Table("Orders")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("cliente_id")]
        public int cliente_id { get; set; }

        [Column("ruta_id")]
        public int? ruta_id { get; set; }

        [Column("fecha_pedido")]
        public DateTime fecha_pedido { get; set; } = DateTime.Now;

        [Required]
        [Column("fecha_entrega")]
        public DateTime fecha_entrega { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("estado_pedido")]
        public string estado_pedido { get; set; } = "Pendiente";

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal valor_total { get; set; }

        [MaxLength(1000)]
        public string? observaciones { get; set; }

        [Column("fecha_actualizacion")]
        public DateTime fecha_actualizacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("cliente_id")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("ruta_id")]
        public virtual RutaEntrega? RutaEntrega { get; set; }

        public virtual ICollection<DetallePedido> Productos { get; set; } = new List<DetallePedido>();
    }
}
