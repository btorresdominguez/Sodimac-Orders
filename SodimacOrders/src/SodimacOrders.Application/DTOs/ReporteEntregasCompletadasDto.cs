using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class ReporteEntregasCompletadasDto
    {
        public int pedido_id { get; set; }
        public string nombre_cliente { get; set; } = string.Empty;
        public string direccion_entrega { get; set; } = string.Empty;
        public DateTime fecha_pedido { get; set; }
        public DateTime fecha_entrega { get; set; }
        public decimal valor_total { get; set; }
        public string? nombre_ruta { get; set; }
        public int total_productos { get; set; }
        public int total_items { get; set; }
    }
}
