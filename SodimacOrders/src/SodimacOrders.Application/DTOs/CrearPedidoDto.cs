using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    // DTOs para transferencia de datos
    public class CrearPedidoDto
    {
        [Required]
        public int cliente_id { get; set; }

        [Required]
        public DateTime fecha_entrega { get; set; }

        public int? ruta_id { get; set; }

        public string? observaciones { get; set; }

        [Required]
        public List<DetallePedidoDto> productos { get; set; } = new();
    }

}
