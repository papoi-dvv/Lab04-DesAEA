using System.Collections.ObjectModel;
using Neptuno.Datos;
using Neptuno.Entidades;
using Neptuno.WPF.MVVM;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Mantenimiento de pedidos. Es una pantalla maestro-detalle: arriba la
/// cabecera del pedido y abajo sus líneas de productos.
///
/// Cabecera y detalle se graban por separado a propósito: un pedido nuevo
/// necesita existir (y tener PedidoID) antes de poder agregarle líneas,
/// porque DetallePedidos tiene una clave foránea hacia Pedidos.
/// </summary>
public class PedidosViewModel : MantenimientoViewModel<Pedido>
{
    private readonly PedidoRepositorio _repositorio = new();
    private readonly CatalogoRepositorio _catalogos = new();
    private readonly ProductoRepositorio _productos = new();

    private Producto? _productoParaAgregar;
    private short _cantidadParaAgregar = 1;
    private decimal _precioParaAgregar;
    private decimal _descuentoParaAgregar;
    private DetallePedido? _detalleSeleccionado;

    public PedidosViewModel(IServicioDialogo dialogo) : base(dialogo)
    {
        AgregarDetalleCommand = new RelayCommand(AgregarDetalle, PuedeAgregarDetalle);
        EliminarDetalleCommand = new RelayCommand(EliminarDetalle,
            () => !EnEdicion && DetalleSeleccionado is not null);

        CargarCatalogos();
        Refrescar();
    }

    // ---------- Catálogos para los ComboBox ----------

    public ObservableCollection<Cliente> Clientes { get; } = new();
    public ObservableCollection<Empleado> Empleados { get; } = new();
    public ObservableCollection<Transportista> Transportistas { get; } = new();
    public ObservableCollection<Producto> ProductosDisponibles { get; } = new();

    // ---------- Detalle ----------

    public ObservableCollection<DetallePedido> Detalle { get; } = new();

    public DetallePedido? DetalleSeleccionado
    {
        get => _detalleSeleccionado;
        set => Asignar(ref _detalleSeleccionado, value);
    }

    /// <summary>
    /// Producto elegido en el formulario de "agregar línea". Al seleccionarlo
    /// se propone su precio de lista, que el usuario todavía puede cambiar.
    /// </summary>
    public Producto? ProductoParaAgregar
    {
        get => _productoParaAgregar;
        set
        {
            if (Asignar(ref _productoParaAgregar, value) && value is not null)
            {
                PrecioParaAgregar = value.PrecioUnidad;
            }
        }
    }

    public short CantidadParaAgregar
    {
        get => _cantidadParaAgregar;
        set => Asignar(ref _cantidadParaAgregar, value);
    }

    public decimal PrecioParaAgregar
    {
        get => _precioParaAgregar;
        set => Asignar(ref _precioParaAgregar, value);
    }

    /// <summary>Descuento expresado de 0 a 1 (0.05 = 5 %).</summary>
    public decimal DescuentoParaAgregar
    {
        get => _descuentoParaAgregar;
        set => Asignar(ref _descuentoParaAgregar, value);
    }

    /// <summary>Suma de las líneas del pedido seleccionado.</summary>
    public decimal TotalDetalle => Detalle.Sum(d => d.Subtotal);

    public RelayCommand AgregarDetalleCommand { get; }
    public RelayCommand EliminarDetalleCommand { get; }

    // ---------- Carga ----------

    private void CargarCatalogos()
    {
        EjecutarProtegido(() =>
        {
            Clientes.Clear();
            foreach (var cliente in _catalogos.ListarClientes())
            {
                Clientes.Add(cliente);
            }

            Empleados.Clear();
            foreach (var empleado in _catalogos.ListarEmpleados())
            {
                Empleados.Add(empleado);
            }

            Transportistas.Clear();
            foreach (var transportista in _catalogos.ListarTransportistas())
            {
                Transportistas.Add(transportista);
            }

            ProductosDisponibles.Clear();
            foreach (var producto in _productos.Listar())
            {
                ProductosDisponibles.Add(producto);
            }
        });
    }

    /// <summary>
    /// Cada vez que cambia el pedido seleccionado se recarga su detalle.
    /// </summary>
    protected override void AlCambiarSeleccion() => RecargarDetalle();

    private void RecargarDetalle()
    {
        Detalle.Clear();

        if (Seleccionado is not null && Seleccionado.PedidoID > 0)
        {
            EjecutarProtegido(() =>
            {
                foreach (var linea in _repositorio.ListarDetalle(Seleccionado.PedidoID))
                {
                    Detalle.Add(linea);
                }
            });
        }

        DetalleSeleccionado = null;
        OnPropertyChanged(nameof(TotalDetalle));
    }

    // ---------- Operaciones del detalle ----------

    private bool PuedeAgregarDetalle() =>
        !EnEdicion
        && Seleccionado is not null
        && Seleccionado.PedidoID > 0
        && ProductoParaAgregar is not null
        && CantidadParaAgregar > 0;

    private void AgregarDetalle()
    {
        if (Seleccionado is null || ProductoParaAgregar is null)
        {
            return;
        }

        if (DescuentoParaAgregar is < 0 or > 1)
        {
            Dialogo.Advertir("El descuento debe estar entre 0 y 1 (por ejemplo 0.05 para 5 %).",
                "Descuento inválido");
            return;
        }

        var linea = new DetallePedido
        {
            PedidoID = Seleccionado.PedidoID,
            ProductoID = ProductoParaAgregar.ProductoID,
            PrecioUnidad = PrecioParaAgregar,
            Cantidad = CantidadParaAgregar,
            Descuento = DescuentoParaAgregar
        };

        EjecutarProtegido(() =>
        {
            _repositorio.InsertarDetalle(linea);
            RecargarDetalle();
            RefrescarTotalEnGrilla();

            Mensaje = $"Se agregó «{ProductoParaAgregar.NombreProducto}» al pedido #{Seleccionado.PedidoID}.";

            // Se deja listo el formulario para la siguiente línea.
            ProductoParaAgregar = null;
            CantidadParaAgregar = 1;
            PrecioParaAgregar = 0;
            DescuentoParaAgregar = 0;
        });
    }

    private void EliminarDetalle()
    {
        if (DetalleSeleccionado is null)
        {
            return;
        }

        var linea = DetalleSeleccionado;
        if (!Dialogo.Confirmar($"¿Quitar «{linea.NombreProducto}» del pedido?", "Confirmar"))
        {
            return;
        }

        EjecutarProtegido(() =>
        {
            _repositorio.EliminarDetalle(linea.PedidoID, linea.ProductoID);
            RecargarDetalle();
            RefrescarTotalEnGrilla();
            Mensaje = $"Se quitó «{linea.NombreProducto}» del pedido.";
        });
    }

    /// <summary>
    /// El total del pedido lo calcula el procedimiento almacenado sumando el
    /// detalle, así que tras tocar una línea hay que releer la cabecera para
    /// que la grilla muestre el importe correcto.
    /// </summary>
    private void RefrescarTotalEnGrilla()
    {
        if (Seleccionado is null)
        {
            return;
        }

        var actualizado = _repositorio.ObtenerPorId(Seleccionado.PedidoID);
        if (actualizado is not null)
        {
            Seleccionado.Total = actualizado.Total;
        }

        // Se reemplaza la fila en la colección para que la grilla la repinte,
        // ya que Pedido es un DTO simple y no notifica cambios por sí mismo.
        var indice = Items.IndexOf(Seleccionado);
        if (indice >= 0 && actualizado is not null)
        {
            Items[indice] = actualizado;
            Seleccionado = actualizado;
        }
    }

    // ---------- Contrato del mantenimiento ----------

    protected override List<Pedido> CargarDesdeRepositorio() => _repositorio.Listar();

    protected override int InsertarEnRepositorio(Pedido item) => _repositorio.Insertar(item);

    protected override void ActualizarEnRepositorio(Pedido item) => _repositorio.Actualizar(item);

    protected override void EliminarDelRepositorio(Pedido item) => _repositorio.Eliminar(item.PedidoID);

    protected override Pedido Clonar(Pedido item) => new()
    {
        PedidoID = item.PedidoID,
        ClienteID = item.ClienteID,
        NombreCliente = item.NombreCliente,
        EmpleadoID = item.EmpleadoID,
        NombreEmpleado = item.NombreEmpleado,
        FechaPedido = item.FechaPedido,
        FechaRequerida = item.FechaRequerida,
        FechaEnvio = item.FechaEnvio,
        TransportistaID = item.TransportistaID,
        NombreTransportista = item.NombreTransportista,
        Destinatario = item.Destinatario,
        CiudadDestino = item.CiudadDestino,
        PaisDestino = item.PaisDestino,
        Total = item.Total
    };

    protected override string Describir(Pedido item) =>
        $"Pedido #{item.PedidoID} — {item.NombreCliente ?? "sin cliente"}";

    protected override int ObtenerId(Pedido item) => item.PedidoID;

    protected override Pedido CrearNuevo() => new()
    {
        FechaPedido = DateTime.Today,
        FechaRequerida = DateTime.Today.AddDays(10),
        PaisDestino = "Perú"
    };

    protected override string? Validar(Pedido item)
    {
        if (item.FechaRequerida is not null && item.FechaRequerida < item.FechaPedido)
        {
            return "La fecha requerida no puede ser anterior a la fecha del pedido.";
        }

        if (item.FechaEnvio is not null && item.FechaEnvio < item.FechaPedido)
        {
            return "La fecha de envío no puede ser anterior a la fecha del pedido.";
        }

        return null;
    }
}
