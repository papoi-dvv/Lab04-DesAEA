namespace Neptuno.Entidades;

/// <summary>
/// Línea de detalle de un pedido (dbo.DetallePedidos).
/// La clave primaria es compuesta: PedidoID + ProductoID.
/// </summary>
public class DetallePedido
{
    public int PedidoID { get; set; }
    public int ProductoID { get; set; }
    public string? NombreProducto { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short Cantidad { get; set; } = 1;
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }

    public override string ToString() => $"{NombreProducto} x{Cantidad}";
}
