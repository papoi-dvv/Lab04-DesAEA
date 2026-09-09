using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Neptuno.WPF.Convertidores;

/// <summary>
/// Invierte un booleano. Se usa para habilitar la grilla y la barra de
/// búsqueda solamente cuando NO se está editando.
/// </summary>
public class InversorBooleano : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool booleano && !booleano;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool booleano && !booleano;
}

/// <summary>
/// Muestra el elemento cuando el valor es true.
/// </summary>
public class BooleanoAVisibilidad : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool booleano && booleano ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility visibilidad && visibilidad == Visibility.Visible;
}

/// <summary>
/// Muestra el elemento cuando el valor es false. Sirve para el panel de
/// ayuda que sólo aparece mientras el formulario está cerrado.
/// </summary>
public class BooleanoAVisibilidadInverso : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool booleano && booleano ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility visibilidad && visibilidad != Visibility.Visible;
}

/// <summary>
/// Convierte el descuento almacenado (0 a 1) en un porcentaje legible.
/// </summary>
public class DescuentoAPorcentaje : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is decimal descuento ? $"{descuento * 100:0.#} %" : "0 %";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
