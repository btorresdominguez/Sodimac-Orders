using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    /// <summary>
    /// DTO para actualizar un pedido completo
    /// </summary>
    public class ActualizarPedidoDto
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del cliente debe ser mayor a 0")]
        public int cliente_id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la ruta debe ser mayor a 0")]
        public int? ruta_id { get; set; }

        [Required(ErrorMessage = "La fecha de entrega es requerida")]
        [DataType(DataType.DateTime)]
        public DateTime fecha_entrega { get; set; }

        [Required(ErrorMessage = "El estado del pedido es requerido")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
        public string estado_pedido { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? observaciones { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<DetallePedidoDto> productos { get; set; } = new List<DetallePedidoDto>();
    }
}
