namespace Neptuno.Entidades;

/// <summary>
/// Fila del reporte de detalles de pedidos filtrado por intervalo de fechas.
/// Corresponde al resultado de usp_Reporte_DetallePedidos_PorFechas, que hace
/// INNER JOIN entre DetallePedidos y Pedidos.
/// </summary>
public class ReporteDetallePedido
{
    public int PedidoID { get; set; }
    public DateTime FechaPedido { get; set; }
    public DateTime? FechaRequerida { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public string? NombreCliente { get; set; }
    public string? NombreEmpleado { get; set; }
    public int ProductoID { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? NombreCategoria { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short Cantidad { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public string? CiudadDestino { get; set; }
    public string? PaisDestino { get; set; }
}
