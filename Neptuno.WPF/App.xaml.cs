using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using Neptuno.Datos;

namespace Neptuno.WPF;

/// <summary>
/// Arranque de la aplicación. Antes de mostrar la ventana principal se
/// comprueba que la base de datos responda, para dar un mensaje claro en vez
/// de que la primera pantalla falle sin explicación.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ConfigurarCultura();

        if (!VerificarBaseDeDatos())
        {
            Shutdown();
            return;
        }

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    /// <summary>
    /// Fija la cultura es-PE para que fechas y montos se muestren y se
    /// escriban en el formato local, tanto en el código como en el binding.
    /// </summary>
    private static void ConfigurarCultura()
    {
        var cultura = new CultureInfo("es-PE");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;
        Thread.CurrentThread.CurrentCulture = cultura;
        Thread.CurrentThread.CurrentUICulture = cultura;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultura.IetfLanguageTag)));
    }

    private static bool VerificarBaseDeDatos()
    {
        try
        {
            ConexionBD.ProbarConexion();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "No se pudo conectar con la base de datos NeptunoDB.\n\n" +
                $"Detalle: {ex.Message}\n\n" +
                "Verifique que:\n" +
                "  1. El servicio de SQL Server esté iniciado.\n" +
                "  2. Se hayan ejecutado NeptunoDB.sql y ProcedimientosAlmacenados.sql.\n" +
                "  3. La cadena de conexión del App.config apunte a su instancia.\n\n" +
                $"Cadena en uso: {ConexionBD.CadenaConexion}",
                "Error de conexión",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
    }
}
