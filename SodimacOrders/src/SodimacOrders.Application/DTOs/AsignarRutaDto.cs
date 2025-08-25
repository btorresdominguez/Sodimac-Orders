using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Application.DTOs
{
    public class AsignarRutaDto
    {
        [Required(ErrorMessage = "El ID de la ruta es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la ruta debe ser mayor a 0")]
        public int ruta_id { get; set; }

        public string? observaciones { get; set; }
    }
}
