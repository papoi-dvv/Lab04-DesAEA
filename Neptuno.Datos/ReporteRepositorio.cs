using System.Data;
using Microsoft.Data.SqlClient;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Reportes del laboratorio. Por ahora sólo el listado de detalles de
/// pedidos filtrado por intervalo de fechas.
/// </summary>
public class ReporteRepositorio : RepositorioBase
{
    /// <summary>
    /// Detalles de pedidos entre dos fechas. Ambos extremos son opcionales:
    /// si no se envía ninguno, devuelve todo el histórico.
    /// </summary>
    public List<ReporteDetallePedido> DetallePedidosPorFechas(DateTime? desde, DateTime? hasta) =>
        Consultar("usp_Reporte_DetallePedidos_PorFechas", p =>
        {
            p.Add("@FechaInicio", SqlDbType.Date).Value = ((object?)desde?.Date).ODbNull();
            p.Add("@FechaFin", SqlDbType.Date).Value = ((object?)hasta?.Date).ODbNull();
        }, Mapear);

    private static ReporteDetallePedido Mapear(SqlDataReader lector) => new()
    {
        PedidoID = lector.Entero("PedidoID"),
        FechaPedido = lector.Fecha("FechaPedido"),
        FechaRequerida = lector.FechaNula("FechaRequerida"),
        FechaEnvio = lector.FechaNula("FechaEnvio"),
        NombreCliente = lector.Texto("NombreCliente"),
        NombreEmpleado = lector.Texto("NombreEmpleado"),
        ProductoID = lector.Entero("ProductoID"),
        NombreProducto = lector.TextoObligatorio("NombreProducto"),
        NombreCategoria = lector.Texto("NombreCategoria"),
        PrecioUnidad = lector.Decimal("PrecioUnidad"),
        Cantidad = lector.Corto("Cantidad"),
        Descuento = lector.Decimal("Descuento"),
        Subtotal = lector.Decimal("Subtotal"),
        CiudadDestino = lector.Texto("CiudadDestino"),
        PaisDestino = lector.Texto("PaisDestino")
    };
}
