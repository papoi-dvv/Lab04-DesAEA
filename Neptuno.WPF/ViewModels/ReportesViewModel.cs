using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;
using Neptuno.Datos;
using Neptuno.Entidades;
using Neptuno.WPF.MVVM;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Reporte de detalles de pedidos filtrado por intervalo de fechas. Los datos
/// salen de usp_Reporte_DetallePedidos_PorFechas, que hace el INNER JOIN entre
/// DetallePedidos y Pedidos que pide el laboratorio.
/// </summary>
public class ReportesViewModel : ViewModelBase
{
    private readonly ReporteRepositorio _repositorio = new();
    private readonly IServicioDialogo _dialogo;

    private DateTime? _fechaInicio;
    private DateTime? _fechaFin;
    private string? _mensaje;
    private bool _ocupado;

    public ReportesViewModel(IServicioDialogo dialogo)
    {
        _dialogo = dialogo;

        // Se abre sin filtros para que la pantalla muestre datos de entrada;
        // el usuario acota el rango después. Proponer el mes en curso dejaría
        // la grilla vacía cuando los pedidos son de meses anteriores.
        _fechaInicio = null;
        _fechaFin = null;

        GenerarCommand = new RelayCommand(Generar, () => !Ocupado);
        LimpiarCommand = new RelayCommand(Limpiar, () => !Ocupado);
        ExportarCsvCommand = new RelayCommand(ExportarCsv, () => Resultados.Count > 0);

        Generar();
    }

    public ObservableCollection<ReporteDetallePedido> Resultados { get; } = new();

    public DateTime? FechaInicio
    {
        get => _fechaInicio;
        set => Asignar(ref _fechaInicio, value);
    }

    public DateTime? FechaFin
    {
        get => _fechaFin;
        set => Asignar(ref _fechaFin, value);
    }

    public string? Mensaje
    {
        get => _mensaje;
        private set => Asignar(ref _mensaje, value);
    }

    public bool Ocupado
    {
        get => _ocupado;
        private set => Asignar(ref _ocupado, value);
    }

    /// <summary>Importe total de las líneas mostradas.</summary>
    public decimal TotalGeneral => Resultados.Sum(r => r.Subtotal);

    /// <summary>Cantidad de unidades vendidas en el período.</summary>
    public int UnidadesTotales => Resultados.Sum(r => (int)r.Cantidad);

    /// <summary>Número de pedidos distintos dentro del período.</summary>
    public int PedidosDistintos => Resultados.Select(r => r.PedidoID).Distinct().Count();

    public RelayCommand GenerarCommand { get; }
    public RelayCommand LimpiarCommand { get; }
    public RelayCommand ExportarCsvCommand { get; }

    private void Generar()
    {
        if (FechaInicio is not null && FechaFin is not null && FechaInicio > FechaFin)
        {
            _dialogo.Advertir("La fecha inicial no puede ser mayor que la fecha final.",
                "Rango inválido");
            return;
        }

        try
        {
            Ocupado = true;

            Resultados.Clear();
            foreach (var fila in _repositorio.DetallePedidosPorFechas(FechaInicio, FechaFin))
            {
                Resultados.Add(fila);
            }

            Mensaje = Resultados.Count == 0
                ? "No hay detalles de pedidos en el rango indicado."
                : $"{Resultados.Count} línea(s) en {PedidosDistintos} pedido(s).";
        }
        catch (NeptunoException ex)
        {
            Mensaje = ex.Message;
            _dialogo.Advertir(ex.Message, "No se pudo generar el reporte");
        }
        finally
        {
            Ocupado = false;
            NotificarTotales();
        }
    }

    /// <summary>
    /// Quita los filtros de fecha y muestra todo el histórico.
    /// </summary>
    private void Limpiar()
    {
        FechaInicio = null;
        FechaFin = null;
        Generar();
    }

    private void ExportarCsv()
    {
        var dialogo = new SaveFileDialog
        {
            Title = "Exportar reporte",
            Filter = "Archivo CSV (*.csv)|*.csv",
            FileName = $"ReporteDetallePedidos_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (dialogo.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var contenido = new StringBuilder();
            contenido.AppendLine("PedidoID;FechaPedido;Cliente;Empleado;Producto;Categoria;PrecioUnidad;Cantidad;Descuento;Subtotal;CiudadDestino");

            foreach (var fila in Resultados)
            {
                contenido.AppendLine(string.Join(';',
                    fila.PedidoID,
                    fila.FechaPedido.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Escapar(fila.NombreCliente),
                    Escapar(fila.NombreEmpleado),
                    Escapar(fila.NombreProducto),
                    Escapar(fila.NombreCategoria),
                    fila.PrecioUnidad.ToString("0.00", CultureInfo.InvariantCulture),
                    fila.Cantidad,
                    fila.Descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    fila.Subtotal.ToString("0.00", CultureInfo.InvariantCulture),
                    Escapar(fila.CiudadDestino)));
            }

            // UTF-8 con BOM para que Excel respete los acentos al abrirlo.
            File.WriteAllText(dialogo.FileName, contenido.ToString(), new UTF8Encoding(true));
            _dialogo.Informar($"Reporte exportado a:\n{dialogo.FileName}", "Exportación completa");
        }
        catch (Exception ex)
        {
            _dialogo.Advertir($"No se pudo exportar: {ex.Message}", "Error");
        }
    }

    /// <summary>Neutraliza el separador dentro de un campo de texto.</summary>
    private static string Escapar(string? texto) => texto?.Replace(';', ',') ?? string.Empty;

    private void NotificarTotales()
    {
        OnPropertyChanged(nameof(TotalGeneral));
        OnPropertyChanged(nameof(UnidadesTotales));
        OnPropertyChanged(nameof(PedidosDistintos));
    }
}
