namespace Neptuno.Entidades;

/// <summary>
/// Cabecera de un pedido (dbo.Pedidos). Total lo calcula el procedimiento
/// almacenado sumando el detalle; no es una columna de la tabla.
/// </summary>
public class Pedido
{
    public int PedidoID { get; set; }
    public int? ClienteID { get; set; }
    public string? NombreCliente { get; set; }
    public int? EmpleadoID { get; set; }
    public string? NombreEmpleado { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.Today;
    public DateTime? FechaRequerida { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public int? TransportistaID { get; set; }
    public string? NombreTransportista { get; set; }
    public string? Destinatario { get; set; }
    public string? CiudadDestino { get; set; }
    public string? PaisDestino { get; set; }
    public decimal Total { get; set; }

    public override string ToString() => $"Pedido #{PedidoID}";
}
