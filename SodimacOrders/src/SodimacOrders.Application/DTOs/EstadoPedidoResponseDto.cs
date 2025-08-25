using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class EstadoPedidoResponseDto
    {
        public int pedido_id { get; set; }
        public string estado_actual { get; set; } = string.Empty;
        public DateTime fecha_pedido { get; set; }
        public DateTime fecha_entrega_programada { get; set; }
        public DateTime fecha_ultima_actualizacion { get; set; }
        public int? ruta_asignada { get; set; }
        public string? nombre_ruta { get; set; }
        public string? observaciones { get; set; }
    }
}
