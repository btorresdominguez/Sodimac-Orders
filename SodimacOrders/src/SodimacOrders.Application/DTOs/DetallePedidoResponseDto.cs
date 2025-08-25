using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class DetallePedidoResponseDto
    {
        public int Id { get; set; }
        public int producto_id { get; set; }
        public string nombre_producto { get; set; } = string.Empty;
        public string codigo_producto { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal precio_unitario { get; set; }
        public decimal subtotal { get; set; }
    }
}
