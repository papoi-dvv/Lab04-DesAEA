using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Neptuno.Datos;

/// <summary>
/// Punto único donde se resuelve la cadena de conexión y se crean las
/// conexiones a NeptunoDB. La cadena se lee del App.config del proyecto
/// que arranca (Neptuno.WPF); si no se encuentra se usa un valor por
/// defecto apuntando a SQL Server Express local.
/// </summary>
public static class ConexionBD
{
    private const string NombreCadena = "NeptunoDB";

    private const string CadenaPorDefecto =
        @"Server=.\SQLEXPRESS;Database=NeptunoDB;Integrated Security=True;TrustServerCertificate=True;";

    private static string? _cadenaConexion;

    /// <summary>
    /// Cadena de conexión activa. Se resuelve una sola vez y se cachea.
    /// </summary>
    public static string CadenaConexion
    {
        get => _cadenaConexion ??= ResolverCadena();
        set => _cadenaConexion = value;
    }

    private static string ResolverCadena()
    {
        var configurada = ConfigurationManager.ConnectionStrings[NombreCadena]?.ConnectionString;
        return string.IsNullOrWhiteSpace(configurada) ? CadenaPorDefecto : configurada;
    }

    /// <summary>
    /// Crea una conexión nueva sin abrir. El llamador es responsable de
    /// abrirla y liberarla (normalmente con using).
    /// </summary>
    public static SqlConnection CrearConexion() => new(CadenaConexion);

    /// <summary>
    /// Abre una conexión y ejecuta una consulta trivial. Se usa al arrancar
    /// la aplicación para avisar de inmediato si la base de datos no está
    /// disponible, en vez de fallar en la primera pantalla que se abra.
    /// </summary>
    public static void ProbarConexion()
    {
        using var conexion = CrearConexion();
        conexion.Open();
        using var comando = new SqlCommand("SELECT 1;", conexion);
        comando.ExecuteScalar();
    }
}
