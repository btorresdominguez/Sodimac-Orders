using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public int cliente_id { get; set; }
        public string nombre_cliente { get; set; } = string.Empty;
        public string direccion_cliente { get; set; } = string.Empty;
        public string email_cliente { get; set; } = string.Empty;
        public int? ruta_id { get; set; }
        public string? nombre_ruta { get; set; }
        public DateTime fecha_pedido { get; set; }
        public DateTime fecha_entrega { get; set; }
        public string estado_pedido { get; set; } = string.Empty;
        public decimal valor_total { get; set; }
        public string? observaciones { get; set; }
        public DateTime fecha_actualizacion { get; set; }
        public List<DetallePedidoResponseDto> productos { get; set; } = new();
    }
}
