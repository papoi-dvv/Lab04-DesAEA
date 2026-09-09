using Neptuno.WPF.MVVM;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// ViewModel de la ventana principal. Sólo agrupa los ViewModels de cada
/// pestaña y expone el servicio de diálogo compartido.
/// </summary>
public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        IServicioDialogo dialogo = new ServicioDialogo();

        Productos = new ProductosViewModel(dialogo);
        Categorias = new CategoriasViewModel(dialogo);
        Proveedores = new ProveedoresViewModel(dialogo);
        Pedidos = new PedidosViewModel(dialogo);
        Reportes = new ReportesViewModel(dialogo);
    }

    public ProductosViewModel Productos { get; }
    public CategoriasViewModel Categorias { get; }
    public ProveedoresViewModel Proveedores { get; }
    public PedidosViewModel Pedidos { get; }
    public ReportesViewModel Reportes { get; }

    public string Titulo => "Neptuno — Laboratorio 04 · WPF + ADO .NET";
}
