using System.Windows.Input;

namespace Neptuno.WPF.MVVM;

/// <summary>
/// Implementación genérica de ICommand que envuelve un delegado. Permite
/// enlazar botones del XAML a métodos del ViewModel sin escribir una clase
/// por cada acción.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _ejecutar;
    private readonly Predicate<object?>? _puedeEjecutar;

    public RelayCommand(Action<object?> ejecutar, Predicate<object?>? puedeEjecutar = null)
    {
        _ejecutar = ejecutar ?? throw new ArgumentNullException(nameof(ejecutar));
        _puedeEjecutar = puedeEjecutar;
    }

    public RelayCommand(Action ejecutar, Func<bool>? puedeEjecutar = null)
        : this(_ => ejecutar(), puedeEjecutar is null ? null : _ => puedeEjecutar())
    {
    }

    /// <summary>
    /// Se apoya en CommandManager para que WPF reevalúe automáticamente
    /// CanExecute cuando cambia el foco o el estado de la interfaz.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parametro) => _puedeEjecutar?.Invoke(parametro) ?? true;

    public void Execute(object? parametro) => _ejecutar(parametro);
}
