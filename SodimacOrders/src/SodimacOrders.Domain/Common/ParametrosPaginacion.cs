using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Common
{
    /// <summary>
    /// Clase para parámetros de paginación
    /// </summary>
    public class ParametrosPaginacion
    {
        private const int PAGINA_MAXIMA = 100;
        private const int PAGINA_PREDETERMINADA = 10;

        /// <summary>
        /// Número de página (inicia en 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        public int pagina { get; set; } = 1;

        private int _tamano_pagina = PAGINA_PREDETERMINADA;

        /// <summary>
        /// Cantidad de registros por página (máximo 100)
        /// </summary>
        [Range(1, PAGINA_MAXIMA, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
        public int tamano_pagina
        {
            get => _tamano_pagina;
            set => _tamano_pagina = value > PAGINA_MAXIMA ? PAGINA_MAXIMA : value;
        }

        /// <summary>
        /// Campo para ordenamiento
        /// </summary>
        public string? ordenar_por { get; set; }

        /// <summary>
        /// Dirección del ordenamiento (asc/desc)
        /// </summary>
        public string direccion_orden { get; set; } = "asc";

        /// <summary>
        /// Filtro de búsqueda general
        /// </summary>
        public string? filtro_busqueda { get; set; }
    }

}
