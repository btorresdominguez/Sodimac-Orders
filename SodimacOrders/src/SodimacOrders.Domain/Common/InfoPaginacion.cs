using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Common
{
    /// <summary>
    /// Información adicional de la paginación
    /// </summary>
    public class InfoPaginacion
    {
        public int desde { get; set; }
        public int hasta { get; set; }
        public int mostrando { get; set; }
    }
}
