namespace Neptuno.Entidades;

/// <summary>
/// Representa una fila de la tabla dbo.Productos. Los campos NombreProveedor
/// y NombreCategoria no existen en la tabla: llegan resueltos por el JOIN que
/// hacen los procedimientos de listado, para poder mostrarlos en la grilla.
/// </summary>
public class Producto
{
    public int ProductoID { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int? ProveedorID { get; set; }
    public string? NombreProveedor { get; set; }
    public int? CategoriaID { get; set; }
    public string? NombreCategoria { get; set; }
    public string? CantidadPorUnidad { get; set; }
    public decimal PrecioUnidad { get; set; }
    public short UnidadesEnExistencia { get; set; }
    public short UnidadesEnPedido { get; set; }
    public short NivelDeReorden { get; set; }
    public bool Descontinuado { get; set; }

    public override string ToString() => NombreProducto;
}
