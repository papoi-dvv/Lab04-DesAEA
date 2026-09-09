using Neptuno.Datos;
using Neptuno.Entidades;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Mantenimiento de categorías.
/// </summary>
public class CategoriasViewModel : MantenimientoViewModel<Categoria>
{
    private readonly CategoriaRepositorio _repositorio = new();

    public CategoriasViewModel(IServicioDialogo dialogo) : base(dialogo)
    {
        Refrescar();
    }

    protected override List<Categoria> CargarDesdeRepositorio() => _repositorio.Listar();

    protected override int InsertarEnRepositorio(Categoria item) => _repositorio.Insertar(item);

    protected override void ActualizarEnRepositorio(Categoria item) => _repositorio.Actualizar(item);

    protected override void EliminarDelRepositorio(Categoria item) => _repositorio.Eliminar(item.CategoriaID);

    protected override Categoria Clonar(Categoria item) => new()
    {
        CategoriaID = item.CategoriaID,
        NombreCategoria = item.NombreCategoria,
        Descripcion = item.Descripcion
    };

    protected override string Describir(Categoria item) => item.NombreCategoria;

    protected override int ObtenerId(Categoria item) => item.CategoriaID;

    protected override string? Validar(Categoria item)
    {
        if (string.IsNullOrWhiteSpace(item.NombreCategoria))
        {
            return "El nombre de la categoría es obligatorio.";
        }

        if (item.NombreCategoria.Length > 30)
        {
            return "El nombre de la categoría no puede superar los 30 caracteres.";
        }

        return null;
    }
}
