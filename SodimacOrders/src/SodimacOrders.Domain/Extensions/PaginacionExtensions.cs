using Microsoft.EntityFrameworkCore;
using SodimacOrders.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SodimacOrders.Domain.Extensions
{
    /// <summary>
    /// Extensiones para implementar paginación
    /// </summary>
// <summary>
    /// Extensiones para implementar paginación
    /// </summary>
    public static class PaginacionExtensions
    {
        /// <summary>
        /// Aplica paginación a un IQueryable
        /// </summary>
        public static IQueryable<T> AplicarPaginacion<T>(
            this IQueryable<T> query,
            ParametrosPaginacion parametros)
        {
            return query
                .Skip((parametros.pagina - 1) * parametros.tamano_pagina)
                .Take(parametros.tamano_pagina);
        }

        /// <summary>
        /// Aplica ordenamiento dinámico
        /// </summary>
        public static IQueryable<T> AplicarOrdenamiento<T>(
            this IQueryable<T> query,
            string? ordenarPor,
            string direccion = "asc")
        {
            if (string.IsNullOrWhiteSpace(ordenarPor))
                return query;

            var parametroExpresion = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var propiedadInfo = typeof(T).GetProperty(ordenarPor);

            if (propiedadInfo == null)
                return query;

            var expresionPropiedad = System.Linq.Expressions.Expression.Property(parametroExpresion, propiedadInfo);
            var expresionLambda = System.Linq.Expressions.Expression.Lambda(expresionPropiedad, parametroExpresion);

            var tipoResultado = typeof(Queryable);
            var nombreMetodo = direccion.ToLower() == "desc" ? "OrderByDescending" : "OrderBy";

            var metodo = tipoResultado.GetMethods()
                .Where(m => m.Name == nombreMetodo && m.GetParameters().Length == 2)
                .Single()
                .MakeGenericMethod(typeof(T), propiedadInfo.PropertyType);

            return (IQueryable<T>)metodo.Invoke(null, new object[] { query, expresionLambda })!;
        }

        /// <summary>
        /// Crea una respuesta paginada
        /// </summary>
        public static async Task<RespuestaPaginada<T>> CrearRespuestaPaginada<T>(
            this IQueryable<T> query,
            ParametrosPaginacion parametros,
            string rutaBase)
        {
            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)parametros.tamano_pagina);

            var datos = await query
                .AplicarOrdenamiento(parametros.ordenar_por, parametros.direccion_orden)
                .AplicarPaginacion(parametros)
                .ToListAsync();

            var respuesta = new RespuestaPaginada<T>
            {
                datos = datos,
                pagina_actual = parametros.pagina,
                tamano_pagina = parametros.tamano_pagina,
                total_registros = totalRegistros,
                total_paginas = totalPaginas
            };

            // Generar enlaces de navegación
            respuesta.enlace_primera_pagina = GenerarEnlacePagina(rutaBase, 1, parametros);
            respuesta.enlace_ultima_pagina = GenerarEnlacePagina(rutaBase, totalPaginas, parametros);

            if (respuesta.tiene_pagina_anterior)
                respuesta.enlace_pagina_anterior = GenerarEnlacePagina(rutaBase, parametros.pagina - 1, parametros);

            if (respuesta.tiene_pagina_siguiente)
                respuesta.enlace_pagina_siguiente = GenerarEnlacePagina(rutaBase, parametros.pagina + 1, parametros);

            return respuesta;
        }

        private static string GenerarEnlacePagina(string rutaBase, int pagina, ParametrosPaginacion parametros)
        {
            var parametrosQuery = new List<string>
            {
                $"pagina={pagina}",
                $"tamano_pagina={parametros.tamano_pagina}"
            };

            if (!string.IsNullOrEmpty(parametros.ordenar_por))
                parametrosQuery.Add($"ordenar_por={parametros.ordenar_por}");

            if (!string.IsNullOrEmpty(parametros.direccion_orden))
                parametrosQuery.Add($"direccion_orden={parametros.direccion_orden}");

            if (!string.IsNullOrEmpty(parametros.filtro_busqueda))
                parametrosQuery.Add($"filtro_busqueda={Uri.EscapeDataString(parametros.filtro_busqueda)}");

            return $"{rutaBase}?{string.Join("&", parametrosQuery)}";
        }
    }
}
