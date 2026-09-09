using Neptuno.Datos;
using Neptuno.Entidades;
using Neptuno.WPF.MVVM;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Mantenimiento de proveedores más la búsqueda por nombre de contacto y
/// ciudad. Los filtros forman parte de la carga: el listado siempre pasa por
/// usp_Proveedores_Buscar, que con filtros vacíos devuelve todo.
/// </summary>
public class ProveedoresViewModel : MantenimientoViewModel<Proveedor>
{
    private readonly ProveedorRepositorio _repositorio = new();

    private string? _filtroNombreContacto;
    private string? _filtroCiudad;

    public ProveedoresViewModel(IServicioDialogo dialogo) : base(dialogo)
    {
        BuscarCommand = new RelayCommand(Buscar, () => !EnEdicion);
        LimpiarFiltrosCommand = new RelayCommand(LimpiarFiltros, () => !EnEdicion && HayFiltros);

        Refrescar();
    }

    public string? FiltroNombreContacto
    {
        get => _filtroNombreContacto;
        set
        {
            if (Asignar(ref _filtroNombreContacto, value))
            {
                OnPropertyChanged(nameof(HayFiltros));
            }
        }
    }

    public string? FiltroCiudad
    {
        get => _filtroCiudad;
        set
        {
            if (Asignar(ref _filtroCiudad, value))
            {
                OnPropertyChanged(nameof(HayFiltros));
            }
        }
    }

    public bool HayFiltros =>
        !string.IsNullOrWhiteSpace(FiltroNombreContacto) || !string.IsNullOrWhiteSpace(FiltroCiudad);

    public RelayCommand BuscarCommand { get; }
    public RelayCommand LimpiarFiltrosCommand { get; }

    private void Buscar()
    {
        Refrescar();

        Mensaje = HayFiltros
            ? $"{Items.Count} proveedor(es) coinciden con la búsqueda."
            : $"{Items.Count} proveedor(es).";
    }

    private void LimpiarFiltros()
    {
        FiltroNombreContacto = null;
        FiltroCiudad = null;
        Refrescar();
        Mensaje = "Filtros limpiados.";
    }

    protected override List<Proveedor> CargarDesdeRepositorio() =>
        _repositorio.Buscar(FiltroNombreContacto, FiltroCiudad);

    protected override int InsertarEnRepositorio(Proveedor item) => _repositorio.Insertar(item);

    protected override void ActualizarEnRepositorio(Proveedor item) => _repositorio.Actualizar(item);

    protected override void EliminarDelRepositorio(Proveedor item) => _repositorio.Eliminar(item.ProveedorID);

    protected override Proveedor Clonar(Proveedor item) => new()
    {
        ProveedorID = item.ProveedorID,
        CompaniaNombre = item.CompaniaNombre,
        NombreContacto = item.NombreContacto,
        CargoContacto = item.CargoContacto,
        Direccion = item.Direccion,
        Ciudad = item.Ciudad,
        CodigoPostal = item.CodigoPostal,
        Pais = item.Pais,
        Telefono = item.Telefono,
        Fax = item.Fax
    };

    protected override string Describir(Proveedor item) => item.CompaniaNombre;

    protected override int ObtenerId(Proveedor item) => item.ProveedorID;

    protected override Proveedor CrearNuevo() => new() { Pais = "Perú" };

    protected override string? Validar(Proveedor item)
    {
        if (string.IsNullOrWhiteSpace(item.CompaniaNombre))
        {
            return "El nombre de la compañía es obligatorio.";
        }

        if (item.CompaniaNombre.Length > 60)
        {
            return "El nombre de la compañía no puede superar los 60 caracteres.";
        }

        return null;
    }
}
