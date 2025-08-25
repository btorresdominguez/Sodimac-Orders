using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SodimacOrders.Domain.Entities
{
    [Table("OrderDetails")]
    public class DetallePedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Specify that this is an identity column
        public int Id { get; set; }

        [Required]
        [Column("pedido_id")]
        public int PedidoId { get; set; } // Changed to PascalCase for consistency

        [Required]
        [Column("producto_id")]
        public int ProductoId { get; set; } // Changed to PascalCase for consistency

        [Required]
        public int Cantidad { get; set; } // Changed to PascalCase for consistency

        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; } // Changed to PascalCase for consistency

        [Column(TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; } // Changed to PascalCase for consistency

        // Navigation properties
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
}