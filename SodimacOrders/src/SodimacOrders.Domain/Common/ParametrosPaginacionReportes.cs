using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Common
{
    /// <summary>
    /// Parámetros para filtros de reportes
    /// </summary>
    public class ParametrosPaginacionReportes : ParametrosPaginacion
    {
        public DateTime? fecha_entrega_desde { get; set; }
        public DateTime? fecha_entrega_hasta { get; set; }
        public int? ruta_id { get; set; }
        public string? nombre_cliente { get; set; }
        public decimal? valor_minimo { get; set; }
    }
}
