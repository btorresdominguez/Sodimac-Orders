using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Common
{
    /// <summary>
    /// Clase genérica para respuestas paginadas
    /// </summary>
    public class RespuestaPaginada<T>
    {
        public IEnumerable<T> datos { get; set; } = new List<T>();
        public int pagina_actual { get; set; }
        public int tamano_pagina { get; set; }
        public int total_registros { get; set; }
        public int total_paginas { get; set; }
        public bool tiene_pagina_anterior => pagina_actual > 1;
        public bool tiene_pagina_siguiente => pagina_actual < total_paginas;
        public string? enlace_pagina_anterior { get; set; }
        public string? enlace_pagina_siguiente { get; set; }
        public string? enlace_primera_pagina { get; set; }
        public string? enlace_ultima_pagina { get; set; }

        /// <summary>
        /// Información adicional de paginación
        /// </summary>
        public InfoPaginacion info_paginacion => new InfoPaginacion
        {
            desde = (pagina_actual - 1) * tamano_pagina + 1,
            hasta = Math.Min(pagina_actual * tamano_pagina, total_registros),
            mostrando = datos.Count()
        };
    }
}
