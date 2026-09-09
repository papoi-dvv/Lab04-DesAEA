using System.Data;
using Neptuno.Entidades;

namespace Neptuno.Datos;

/// <summary>
/// Acceso a datos de Categorías. Toda la interacción con la base pasa por
/// los procedimientos almacenados usp_Categorias_*.
/// </summary>
public class CategoriaRepositorio : RepositorioBase
{
    public List<Categoria> Listar() =>
        Consultar("usp_Categorias_Listar", null, Mapear);

    public Categoria? ObtenerPorId(int categoriaId) =>
        Consultar("usp_Categorias_ObtenerPorId",
            p => p.Add("@CategoriaID", SqlDbType.Int).Value = categoriaId,
            Mapear).FirstOrDefault();

    public int Insertar(Categoria categoria) =>
        EjecutarConIdentidad("usp_Categorias_Insertar", "@CategoriaID", p =>
        {
            p.Add("@NombreCategoria", SqlDbType.NVarChar, 30).Value = categoria.NombreCategoria;
            p.Add("@Descripcion", SqlDbType.NVarChar, 200).Value = categoria.Descripcion.ODbNull();
        });

    public void Actualizar(Categoria categoria) =>
        Ejecutar("usp_Categorias_Actualizar", p =>
        {
            p.Add("@CategoriaID", SqlDbType.Int).Value = categoria.CategoriaID;
            p.Add("@NombreCategoria", SqlDbType.NVarChar, 30).Value = categoria.NombreCategoria;
            p.Add("@Descripcion", SqlDbType.NVarChar, 200).Value = categoria.Descripcion.ODbNull();
        });

    public void Eliminar(int categoriaId) =>
        Ejecutar("usp_Categorias_Eliminar",
            p => p.Add("@CategoriaID", SqlDbType.Int).Value = categoriaId);

    private static Categoria Mapear(Microsoft.Data.SqlClient.SqlDataReader lector) => new()
    {
        CategoriaID = lector.Entero("CategoriaID"),
        NombreCategoria = lector.TextoObligatorio("NombreCategoria"),
        Descripcion = lector.Texto("Descripcion")
    };
}
