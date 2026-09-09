using System.Data;
using Microsoft.Data.SqlClient;

namespace Neptuno.Datos;

/// <summary>
/// Base común de todos los repositorios. Concentra el patrón repetitivo de
/// ADO .NET (abrir conexión, armar el SqlCommand como procedimiento
/// almacenado, ejecutar, liberar) para que cada repositorio se limite a
/// declarar sus parámetros y a mapear el resultado.
/// </summary>
public abstract class RepositorioBase
{
    /// <summary>
    /// Ejecuta un procedimiento que devuelve filas y mapea cada una con
    /// <paramref name="mapear"/>.
    /// </summary>
    protected List<T> Consultar<T>(
        string procedimiento,
        Action<SqlParameterCollection>? parametros,
        Func<SqlDataReader, T> mapear)
    {
        var resultados = new List<T>();

        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = CrearComando(procedimiento, conexion, parametros);

            conexion.Open();
            using var lector = comando.ExecuteReader();
            while (lector.Read())
            {
                resultados.Add(mapear(lector));
            }
        }
        catch (SqlException ex)
        {
            throw Traducir(ex, procedimiento);
        }

        return resultados;
    }

    /// <summary>
    /// Ejecuta un procedimiento que no devuelve filas (INSERT, UPDATE,
    /// DELETE) y retorna el número de filas afectadas.
    /// </summary>
    protected int Ejecutar(string procedimiento, Action<SqlParameterCollection>? parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = CrearComando(procedimiento, conexion, parametros);

            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex, procedimiento);
        }
    }

    /// <summary>
    /// Ejecuta un procedimiento de alta que devuelve el identificador nuevo
    /// a través de un parámetro OUTPUT.
    /// </summary>
    protected int EjecutarConIdentidad(
        string procedimiento,
        string nombreParametroSalida,
        Action<SqlParameterCollection> parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = CrearComando(procedimiento, conexion, parametros);

            var salida = comando.Parameters.Add(nombreParametroSalida, SqlDbType.Int);
            salida.Direction = ParameterDirection.Output;

            conexion.Open();
            comando.ExecuteNonQuery();

            return salida.Value is DBNull or null ? 0 : Convert.ToInt32(salida.Value);
        }
        catch (SqlException ex)
        {
            throw Traducir(ex, procedimiento);
        }
    }

    private static SqlCommand CrearComando(
        string procedimiento,
        SqlConnection conexion,
        Action<SqlParameterCollection>? parametros)
    {
        var comando = new SqlCommand(procedimiento, conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        parametros?.Invoke(comando.Parameters);
        return comando;
    }

    /// <summary>
    /// Convierte los THROW de los procedimientos almacenados en
    /// <see cref="NeptunoException"/> y deja pasar el resto como error real
    /// de base de datos, con un mensaje entendible para el usuario.
    /// </summary>
    private static Exception Traducir(SqlException ex, string procedimiento)
    {
        if (NeptunoException.EsErrorDeNegocio(ex))
        {
            return new NeptunoException(ex.Message, ex.Number, ex);
        }

        return new NeptunoException(
            $"Error de base de datos al ejecutar {procedimiento}: {ex.Message}",
            ex.Number,
            ex);
    }
}
