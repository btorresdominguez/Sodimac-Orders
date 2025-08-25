using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Enums
{
    public enum EstadoPedido
    {
        Pendiente,
        Confirmado,
        En_Preparacion,
        En_Transito,
        Entregado,
        Cancelado
    }
}
