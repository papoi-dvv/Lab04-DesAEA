using Microsoft.Data.SqlClient;

namespace Neptuno.Datos;

/// <summary>
/// Error de negocio lanzado por un procedimiento almacenado mediante THROW
/// con un número mayor o igual a 50000 (por ejemplo, intentar borrar una
/// categoría que todavía tiene productos).
///
/// Se traduce la SqlException a este tipo para que la capa de presentación
/// pueda distinguir "el usuario hizo algo inválido" de "la base de datos
/// falló", y mostrar el mensaje del procedimiento tal cual.
/// </summary>
public class NeptunoException : Exception
{
    public int NumeroError { get; }

    public NeptunoException(string mensaje, int numeroError, Exception? interna = null)
        : base(mensaje, interna)
    {
        NumeroError = numeroError;
    }

    /// <summary>
    /// Indica si la excepción proviene de una validación propia de los
    /// procedimientos almacenados y no de un fallo del motor.
    /// </summary>
    public static bool EsErrorDeNegocio(SqlException ex) => ex.Number >= 50000;
}
