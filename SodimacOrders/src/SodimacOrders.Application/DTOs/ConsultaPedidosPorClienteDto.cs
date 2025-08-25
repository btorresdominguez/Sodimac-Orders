using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class ConsultaPedidosPorClienteDto
    {
        public int cliente_id { get; set; }
        public string nombre_cliente { get; set; } = string.Empty;
        public string email_cliente { get; set; } = string.Empty;
        public List<PedidoResponseDto> pedidos { get; set; } = new();
    }
}
