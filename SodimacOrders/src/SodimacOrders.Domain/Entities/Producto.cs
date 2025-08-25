using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SodimacOrders.Domain.Entities
{
    [Table("Products")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("nombre_producto")]
        public string nombre_producto { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("codigo_producto")]
        public string codigo_producto { get; set; } = string.Empty;

        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal precio_unitario { get; set; }

        [MaxLength(100)]
        public string? categoria { get; set; }

        public bool activo { get; set; } = true;

        // Navegación
        public virtual ICollection<DetallePedido> DetallesPedidos { get; set; } = new List<DetallePedido>();
    }
}
