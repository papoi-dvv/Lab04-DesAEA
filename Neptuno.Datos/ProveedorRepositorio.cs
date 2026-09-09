using System.Data;
using Microsoft.Data.SqlClient;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Acceso a datos de Proveedores, incluida la búsqueda por nombre de
/// contacto y ciudad que pide el laboratorio.
/// </summary>
public class ProveedorRepositorio : RepositorioBase
{
    public List<Proveedor> Listar() =>
        Consultar("usp_Proveedores_Listar", null, Mapear);

    /// <summary>
    /// Búsqueda con filtros opcionales. Un filtro vacío se envía como NULL
    /// para que el procedimiento lo ignore, de modo que sirve tanto para
    /// buscar por un solo criterio como por ambos combinados.
    /// </summary>
    public List<Proveedor> Buscar(string? nombreContacto, string? ciudad) =>
        Consultar("usp_Proveedores_Buscar", p =>
        {
            p.Add("@NombreContacto", SqlDbType.NVarChar, 40).Value =
                string.IsNullOrWhiteSpace(nombreContacto) ? DBNull.Value : nombreContacto.Trim();
            p.Add("@Ciudad", SqlDbType.NVarChar, 30).Value =
                string.IsNullOrWhiteSpace(ciudad) ? DBNull.Value : ciudad.Trim();
        }, Mapear);

    public Proveedor? ObtenerPorId(int proveedorId) =>
        Consultar("usp_Proveedores_ObtenerPorId",
            p => p.Add("@ProveedorID", SqlDbType.Int).Value = proveedorId,
            Mapear).FirstOrDefault();

    public int Insertar(Proveedor proveedor) =>
        EjecutarConIdentidad("usp_Proveedores_Insertar", "@ProveedorID",
            p => AgregarCampos(p, proveedor));

    public void Actualizar(Proveedor proveedor) =>
        Ejecutar("usp_Proveedores_Actualizar", p =>
        {
            p.Add("@ProveedorID", SqlDbType.Int).Value = proveedor.ProveedorID;
            AgregarCampos(p, proveedor);
        });

    public void Eliminar(int proveedorId) =>
        Ejecutar("usp_Proveedores_Eliminar",
            p => p.Add("@ProveedorID", SqlDbType.Int).Value = proveedorId);

    private static void AgregarCampos(SqlParameterCollection p, Proveedor proveedor)
    {
        p.Add("@CompaniaNombre", SqlDbType.NVarChar, 60).Value = proveedor.CompaniaNombre;
        p.Add("@NombreContacto", SqlDbType.NVarChar, 40).Value = proveedor.NombreContacto.ODbNull();
        p.Add("@CargoContacto", SqlDbType.NVarChar, 40).Value = proveedor.CargoContacto.ODbNull();
        p.Add("@Direccion", SqlDbType.NVarChar, 80).Value = proveedor.Direccion.ODbNull();
        p.Add("@Ciudad", SqlDbType.NVarChar, 30).Value = proveedor.Ciudad.ODbNull();
        p.Add("@CodigoPostal", SqlDbType.NVarChar, 10).Value = proveedor.CodigoPostal.ODbNull();
        p.Add("@Pais", SqlDbType.NVarChar, 30).Value = proveedor.Pais.ODbNull();
        p.Add("@Telefono", SqlDbType.NVarChar, 24).Value = proveedor.Telefono.ODbNull();
        p.Add("@Fax", SqlDbType.NVarChar, 24).Value = proveedor.Fax.ODbNull();
    }

    private static Proveedor Mapear(SqlDataReader lector) => new()
    {
        ProveedorID = lector.Entero("ProveedorID"),
        CompaniaNombre = lector.TextoObligatorio("CompaniaNombre"),
        NombreContacto = lector.Texto("NombreContacto"),
        CargoContacto = lector.Texto("CargoContacto"),
        Direccion = lector.Texto("Direccion"),
        Ciudad = lector.Texto("Ciudad"),
        CodigoPostal = lector.Texto("CodigoPostal"),
        Pais = lector.Texto("Pais"),
        Telefono = lector.Texto("Telefono"),
        Fax = lector.Texto("Fax")
    };
}
