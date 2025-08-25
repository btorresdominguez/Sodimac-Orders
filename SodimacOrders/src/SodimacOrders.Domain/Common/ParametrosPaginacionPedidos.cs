using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Common
{
    /// <summary>
    /// Parámetros específicos para filtros de pedidos
    /// </summary>
    public class ParametrosPaginacionPedidos : ParametrosPaginacion
    {
        public int? cliente_id { get; set; }
        public string? estado_pedido { get; set; }
        public int? ruta_id { get; set; }
        public DateTime? fecha_desde { get; set; }
        public DateTime? fecha_hasta { get; set; }
        public decimal? valor_minimo { get; set; }
        public decimal? valor_maximo { get; set; }
    }
}
