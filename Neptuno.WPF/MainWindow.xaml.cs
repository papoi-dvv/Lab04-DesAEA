using System.Windows;
using Neptuno.WPF.ViewModels;

namespace Neptuno.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // El MainViewModel construye los ViewModels de cada pestaña, que a su
        // vez cargan sus datos desde la base al instanciarse.
        DataContext = new MainViewModel();
    }
}
