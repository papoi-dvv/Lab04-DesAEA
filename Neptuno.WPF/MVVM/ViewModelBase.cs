using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neptuno.WPF.MVVM;

/// <summary>
/// Base de todos los ViewModels. Implementa INotifyPropertyChanged, que es
/// el mecanismo por el cual el binding de WPF se entera de que una propiedad
/// cambió y refresca la interfaz.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? nombrePropiedad = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));

    /// <summary>
    /// Asigna el campo sólo si el valor cambió y, en ese caso, notifica.
    /// Devuelve true si hubo cambio, para poder encadenar efectos.
    /// </summary>
    protected bool Asignar<T>(ref T campo, T valor, [CallerMemberName] string? nombrePropiedad = null)
    {
        if (EqualityComparer<T>.Default.Equals(campo, valor))
        {
            return false;
        }

        campo = valor;
        OnPropertyChanged(nombrePropiedad);
        return true;
    }
}
