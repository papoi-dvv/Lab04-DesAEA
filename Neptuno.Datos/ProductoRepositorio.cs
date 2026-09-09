using System.Data;
using Microsoft.Data.SqlClient;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Acceso a datos de Productos. El listado trae ya resueltos los nombres de
/// categoría y proveedor mediante el JOIN que hace el procedimiento.
/// </summary>
public class ProductoRepositorio : RepositorioBase
{
    public List<Producto> Listar() =>
        Consultar("usp_Productos_Listar", null, Mapear);

    public Producto? ObtenerPorId(int productoId) =>
        Consultar("usp_Productos_ObtenerPorId",
            p => p.Add("@ProductoID", SqlDbType.Int).Value = productoId,
            Mapear).FirstOrDefault();

    public int Insertar(Producto producto) =>
        EjecutarConIdentidad("usp_Productos_Insertar", "@ProductoID",
            p => AgregarCampos(p, producto));

    public void Actualizar(Producto producto) =>
        Ejecutar("usp_Productos_Actualizar", p =>
        {
            p.Add("@ProductoID", SqlDbType.Int).Value = producto.ProductoID;
            AgregarCampos(p, producto);
        });

    public void Eliminar(int productoId) =>
        Ejecutar("usp_Productos_Eliminar",
            p => p.Add("@ProductoID", SqlDbType.Int).Value = productoId);

    private static void AgregarCampos(SqlParameterCollection p, Producto producto)
    {
        p.Add("@NombreProducto", SqlDbType.NVarChar, 60).Value = producto.NombreProducto;
        p.Add("@ProveedorID", SqlDbType.Int).Value = producto.ProveedorID.ODbNull();
        p.Add("@CategoriaID", SqlDbType.Int).Value = producto.CategoriaID.ODbNull();
        p.Add("@CantidadPorUnidad", SqlDbType.NVarChar, 30).Value = producto.CantidadPorUnidad.ODbNull();
        p.Add("@PrecioUnidad", SqlDbType.Decimal).Value = producto.PrecioUnidad;
        p.Add("@UnidadesEnExistencia", SqlDbType.SmallInt).Value = producto.UnidadesEnExistencia;
        p.Add("@UnidadesEnPedido", SqlDbType.SmallInt).Value = producto.UnidadesEnPedido;
        p.Add("@NivelDeReorden", SqlDbType.SmallInt).Value = producto.NivelDeReorden;
        p.Add("@Descontinuado", SqlDbType.Bit).Value = producto.Descontinuado;
    }

    private static Producto Mapear(SqlDataReader lector) => new()
    {
        ProductoID = lector.Entero("ProductoID"),
        NombreProducto = lector.TextoObligatorio("NombreProducto"),
        ProveedorID = lector.EnteroNulo("ProveedorID"),
        NombreProveedor = lector.Texto("NombreProveedor"),
        CategoriaID = lector.EnteroNulo("CategoriaID"),
        NombreCategoria = lector.Texto("NombreCategoria"),
        CantidadPorUnidad = lector.Texto("CantidadPorUnidad"),
        PrecioUnidad = lector.Decimal("PrecioUnidad"),
        UnidadesEnExistencia = lector.Corto("UnidadesEnExistencia"),
        UnidadesEnPedido = lector.Corto("UnidadesEnPedido"),
        NivelDeReorden = lector.Corto("NivelDeReorden"),
        Descontinuado = lector.Booleano("Descontinuado")
    };
}
