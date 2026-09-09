using Microsoft.Data.SqlClient;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Listados de sólo lectura que alimentan los ComboBox de la interfaz
/// (clientes, empleados y transportistas en el mantenimiento de pedidos).
/// </summary>
public class CatalogoRepositorio : RepositorioBase
{
    public List<Cliente> ListarClientes() =>
        Consultar("usp_Clientes_Listar", null, lector => new Cliente
        {
            ClienteID = lector.Entero("ClienteID"),
            Empresa = lector.TextoObligatorio("Empresa"),
            NombreContacto = lector.Texto("NombreContacto"),
            Ciudad = lector.Texto("Ciudad"),
            Pais = lector.Texto("Pais"),
            Telefono = lector.Texto("Telefono")
        });

    public List<Empleado> ListarEmpleados() =>
        Consultar("usp_Empleados_Listar", null, lector => new Empleado
        {
            EmpleadoID = lector.Entero("EmpleadoID"),
            Nombre = lector.TextoObligatorio("Nombre"),
            Apellidos = lector.TextoObligatorio("Apellidos"),
            NombreCompleto = lector.TextoObligatorio("NombreCompleto"),
            Cargo = lector.Texto("Cargo"),
            Ciudad = lector.Texto("Ciudad"),
            Pais = lector.Texto("Pais")
        });

    public List<Transportista> ListarTransportistas() =>
        Consultar("usp_Transportistas_Listar", null, lector => new Transportista
        {
            TransportistaID = lector.Entero("TransportistaID"),
            CompaniaNombre = lector.TextoObligatorio("CompaniaNombre"),
            Telefono = lector.Texto("Telefono")
        });
}
