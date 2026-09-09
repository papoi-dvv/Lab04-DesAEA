using System.Data;
using Microsoft.Data.SqlClient;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Acceso a datos de Pedidos y de su detalle. La cabecera y las líneas se
/// manejan por separado porque en la interfaz también se editan por separado:
/// primero se graba el pedido y luego se le agregan o quitan productos.
/// </summary>
public class PedidoRepositorio : RepositorioBase
{
    public List<Pedido> Listar() =>
        Consultar("usp_Pedidos_Listar", null, Mapear);

    public Pedido? ObtenerPorId(int pedidoId) =>
        Consultar("usp_Pedidos_ObtenerPorId",
            p => p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId,
            Mapear).FirstOrDefault();

    public int Insertar(Pedido pedido) =>
        EjecutarConIdentidad("usp_Pedidos_Insertar", "@PedidoID",
            p => AgregarCampos(p, pedido));

    public void Actualizar(Pedido pedido) =>
        Ejecutar("usp_Pedidos_Actualizar", p =>
        {
            p.Add("@PedidoID", SqlDbType.Int).Value = pedido.PedidoID;
            AgregarCampos(p, pedido);
        });

    /// <summary>
    /// Elimina el pedido junto con todo su detalle. El procedimiento hace
    /// ambos borrados dentro de una transacción.
    /// </summary>
    public void Eliminar(int pedidoId) =>
        Ejecutar("usp_Pedidos_Eliminar",
            p => p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId);

    // ---------- Detalle ----------

    public List<DetallePedido> ListarDetalle(int pedidoId) =>
        Consultar("usp_DetallePedidos_ListarPorPedido",
            p => p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId,
            MapearDetalle);

    public void InsertarDetalle(DetallePedido detalle) =>
        Ejecutar("usp_DetallePedidos_Insertar", p => AgregarCamposDetalle(p, detalle));

    public void ActualizarDetalle(DetallePedido detalle) =>
        Ejecutar("usp_DetallePedidos_Actualizar", p => AgregarCamposDetalle(p, detalle));

    public void EliminarDetalle(int pedidoId, int productoId) =>
        Ejecutar("usp_DetallePedidos_Eliminar", p =>
        {
            p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId;
            p.Add("@ProductoID", SqlDbType.Int).Value = productoId;
        });

    // ---------- Apoyo ----------

    private static void AgregarCampos(SqlParameterCollection p, Pedido pedido)
    {
        p.Add("@ClienteID", SqlDbType.Int).Value = pedido.ClienteID.ODbNull();
        p.Add("@EmpleadoID", SqlDbType.Int).Value = pedido.EmpleadoID.ODbNull();
        p.Add("@FechaPedido", SqlDbType.Date).Value = pedido.FechaPedido.Date;
        p.Add("@FechaRequerida", SqlDbType.Date).Value = ((object?)pedido.FechaRequerida?.Date).ODbNull();
        p.Add("@FechaEnvio", SqlDbType.Date).Value = ((object?)pedido.FechaEnvio?.Date).ODbNull();
        p.Add("@TransportistaID", SqlDbType.Int).Value = pedido.TransportistaID.ODbNull();
        p.Add("@Destinatario", SqlDbType.NVarChar, 60).Value = pedido.Destinatario.ODbNull();
        p.Add("@CiudadDestino", SqlDbType.NVarChar, 30).Value = pedido.CiudadDestino.ODbNull();
        p.Add("@PaisDestino", SqlDbType.NVarChar, 30).Value = pedido.PaisDestino.ODbNull();
    }

    private static void AgregarCamposDetalle(SqlParameterCollection p, DetallePedido detalle)
    {
        p.Add("@PedidoID", SqlDbType.Int).Value = detalle.PedidoID;
        p.Add("@ProductoID", SqlDbType.Int).Value = detalle.ProductoID;
        p.Add("@PrecioUnidad", SqlDbType.Decimal).Value = detalle.PrecioUnidad;
        p.Add("@Cantidad", SqlDbType.SmallInt).Value = detalle.Cantidad;
        p.Add("@Descuento", SqlDbType.Decimal).Value = detalle.Descuento;
    }

    private static Pedido Mapear(SqlDataReader lector) => new()
    {
        PedidoID = lector.Entero("PedidoID"),
        ClienteID = lector.EnteroNulo("ClienteID"),
        NombreCliente = lector.Texto("NombreCliente"),
        EmpleadoID = lector.EnteroNulo("EmpleadoID"),
        NombreEmpleado = lector.Texto("NombreEmpleado"),
        FechaPedido = lector.Fecha("FechaPedido"),
        FechaRequerida = lector.FechaNula("FechaRequerida"),
        FechaEnvio = lector.FechaNula("FechaEnvio"),
        TransportistaID = lector.EnteroNulo("TransportistaID"),
        NombreTransportista = lector.Texto("NombreTransportista"),
        Destinatario = lector.Texto("Destinatario"),
        CiudadDestino = lector.Texto("CiudadDestino"),
        PaisDestino = lector.Texto("PaisDestino"),
        Total = lector.Decimal("Total")
    };

    private static DetallePedido MapearDetalle(SqlDataReader lector) => new()
    {
        PedidoID = lector.Entero("PedidoID"),
        ProductoID = lector.Entero("ProductoID"),
        NombreProducto = lector.Texto("NombreProducto"),
        PrecioUnidad = lector.Decimal("PrecioUnidad"),
        Cantidad = lector.Corto("Cantidad"),
        Descuento = lector.Decimal("Descuento"),
        Subtotal = lector.Decimal("Subtotal")
    };
}
