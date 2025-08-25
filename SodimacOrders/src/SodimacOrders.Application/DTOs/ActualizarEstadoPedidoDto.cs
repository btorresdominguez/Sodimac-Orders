using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class ActualizarEstadoPedidoDto
    {
        [Required]
        [MaxLength(50)]
        public string estado_pedido { get; set; } = string.Empty;

        public int? ruta_id { get; set; }

        public string? observaciones { get; set; }
    }
}
