using System.Collections.ObjectModel;
using Neptuno.Datos;
using Neptuno.Entidades;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Mantenimiento de productos. Además del CRUD carga las listas de
/// categorías y proveedores para los ComboBox del formulario.
/// </summary>
public class ProductosViewModel : MantenimientoViewModel<Producto>
{
    private readonly ProductoRepositorio _repositorio = new();
    private readonly CategoriaRepositorio _categorias = new();
    private readonly ProveedorRepositorio _proveedores = new();

    public ProductosViewModel(IServicioDialogo dialogo) : base(dialogo)
    {
        CargarCatalogos();
        Refrescar();
    }

    public ObservableCollection<Categoria> Categorias { get; } = new();
    public ObservableCollection<Proveedor> Proveedores { get; } = new();

    /// <summary>
    /// Las listas de apoyo se recargan al entrar a la pantalla y cada vez que
    /// se refresca, por si el usuario dio de alta una categoría o proveedor
    /// en otra pestaña durante la misma sesión.
    /// </summary>
    private void CargarCatalogos()
    {
        EjecutarProtegido(() =>
        {
            Categorias.Clear();
            foreach (var categoria in _categorias.Listar())
            {
                Categorias.Add(categoria);
            }

            Proveedores.Clear();
            foreach (var proveedor in _proveedores.Listar())
            {
                Proveedores.Add(proveedor);
            }
        });
    }

    public void RecargarTodo()
    {
        CargarCatalogos();
        Refrescar();
    }

    protected override List<Producto> CargarDesdeRepositorio() => _repositorio.Listar();

    protected override int InsertarEnRepositorio(Producto item) => _repositorio.Insertar(item);

    protected override void ActualizarEnRepositorio(Producto item) => _repositorio.Actualizar(item);

    protected override void EliminarDelRepositorio(Producto item) => _repositorio.Eliminar(item.ProductoID);

    protected override Producto Clonar(Producto item) => new()
    {
        ProductoID = item.ProductoID,
        NombreProducto = item.NombreProducto,
        ProveedorID = item.ProveedorID,
        NombreProveedor = item.NombreProveedor,
        CategoriaID = item.CategoriaID,
        NombreCategoria = item.NombreCategoria,
        CantidadPorUnidad = item.CantidadPorUnidad,
        PrecioUnidad = item.PrecioUnidad,
        UnidadesEnExistencia = item.UnidadesEnExistencia,
        UnidadesEnPedido = item.UnidadesEnPedido,
        NivelDeReorden = item.NivelDeReorden,
        Descontinuado = item.Descontinuado
    };

    protected override string Describir(Producto item) => item.NombreProducto;

    protected override int ObtenerId(Producto item) => item.ProductoID;

    protected override string? Validar(Producto item)
    {
        if (string.IsNullOrWhiteSpace(item.NombreProducto))
        {
            return "El nombre del producto es obligatorio.";
        }

        if (item.NombreProducto.Length > 60)
        {
            return "El nombre del producto no puede superar los 60 caracteres.";
        }

        if (item.PrecioUnidad < 0)
        {
            return "El precio unitario no puede ser negativo.";
        }

        if (item.UnidadesEnExistencia < 0 || item.UnidadesEnPedido < 0 || item.NivelDeReorden < 0)
        {
            return "Las unidades y el nivel de reorden no pueden ser negativos.";
        }

        return null;
    }
}
