using System.Windows;

namespace Neptuno.WPF.Servicios;

/// <summary>
/// Abstracción de los cuadros de diálogo. Existe para que los ViewModels
/// puedan pedir una confirmación sin depender directamente de MessageBox,
/// que es una clase de interfaz gráfica.
/// </summary>
public interface IServicioDialogo
{
    bool Confirmar(string mensaje, string titulo = "Confirmar");
    void Informar(string mensaje, string titulo = "Información");
    void Advertir(string mensaje, string titulo = "Atención");
}

/// <summary>
/// Implementación real sobre MessageBox de WPF.
/// </summary>
public class ServicioDialogo : IServicioDialogo
{
    public bool Confirmar(string mensaje, string titulo = "Confirmar")
        => MessageBox.Show(mensaje, titulo, MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;

    public void Informar(string mensaje, string titulo = "Información")
        => MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Information);

    public void Advertir(string mensaje, string titulo = "Atención")
        => MessageBox.Show(mensaje, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
}
