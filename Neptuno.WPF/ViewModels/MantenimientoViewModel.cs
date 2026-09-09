using System.Collections.ObjectModel;
using Neptuno.Datos;
using Neptuno.WPF.MVVM;
using Neptuno.WPF.Servicios;

namespace Neptuno.WPF.ViewModels;

/// <summary>
/// Lógica común a los tres mantenimientos del laboratorio (productos,
/// categorías y proveedores). Todos comparten el mismo ciclo:
///
///     listar → seleccionar → nuevo/editar → guardar o cancelar → eliminar
///
/// Las clases derivadas sólo indican cómo hablar con su repositorio y cómo
/// clonar y validar su entidad.
/// </summary>
public abstract class MantenimientoViewModel<T> : ViewModelBase where T : class, new()
{
    protected readonly IServicioDialogo Dialogo;

    private T? _seleccionado;
    private T? _editable;
    private bool _enEdicion;
    private bool _esNuevo;
    private string? _mensaje;
    private bool _ocupado;

    protected MantenimientoViewModel(IServicioDialogo dialogo)
    {
        Dialogo = dialogo;

        NuevoCommand = new RelayCommand(Nuevo, () => !EnEdicion);
        EditarCommand = new RelayCommand(Editar, () => !EnEdicion && Seleccionado is not null);
        GuardarCommand = new RelayCommand(Guardar, () => EnEdicion);
        CancelarCommand = new RelayCommand(Cancelar, () => EnEdicion);
        EliminarCommand = new RelayCommand(Eliminar, () => !EnEdicion && Seleccionado is not null);
        RefrescarCommand = new RelayCommand(Refrescar, () => !EnEdicion);
    }

    // ---------- Estado expuesto a la vista ----------

    public ObservableCollection<T> Items { get; } = new();

    /// <summary>Fila marcada en la grilla.</summary>
    public T? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (Asignar(ref _seleccionado, value))
            {
                AlCambiarSeleccion();
            }
        }
    }

    /// <summary>
    /// Gancho para las pantallas maestro-detalle: se dispara cuando cambia la
    /// fila seleccionada, para poder recargar el detalle asociado.
    /// </summary>
    protected virtual void AlCambiarSeleccion()
    {
    }

    /// <summary>
    /// Copia sobre la que trabaja el formulario. Se edita esta y no el objeto
    /// de la grilla, para que Cancelar pueda descartar los cambios sin dejar
    /// la lista con datos a medio escribir.
    /// </summary>
    public T? Editable
    {
        get => _editable;
        private set => Asignar(ref _editable, value);
    }

    /// <summary>True mientras el formulario está abierto.</summary>
    public bool EnEdicion
    {
        get => _enEdicion;
        private set => Asignar(ref _enEdicion, value);
    }

    /// <summary>Mensaje de estado o de error que se muestra al pie.</summary>
    public string? Mensaje
    {
        get => _mensaje;
        protected set => Asignar(ref _mensaje, value);
    }

    public bool Ocupado
    {
        get => _ocupado;
        private set => Asignar(ref _ocupado, value);
    }

    public RelayCommand NuevoCommand { get; }
    public RelayCommand EditarCommand { get; }
    public RelayCommand GuardarCommand { get; }
    public RelayCommand CancelarCommand { get; }
    public RelayCommand EliminarCommand { get; }
    public RelayCommand RefrescarCommand { get; }

    // ---------- Puntos de extensión ----------

    protected abstract List<T> CargarDesdeRepositorio();
    protected abstract int InsertarEnRepositorio(T item);
    protected abstract void ActualizarEnRepositorio(T item);
    protected abstract void EliminarDelRepositorio(T item);

    /// <summary>Copia profunda usada al abrir el formulario de edición.</summary>
    protected abstract T Clonar(T item);

    /// <summary>Texto que identifica al registro en los mensajes.</summary>
    protected abstract string Describir(T item);

    /// <summary>Identificador del registro; 0 significa "todavía no existe".</summary>
    protected abstract int ObtenerId(T item);

    /// <summary>
    /// Validación de interfaz, previa al viaje a la base de datos. Devuelve
    /// null si todo está correcto o el mensaje de error si no.
    /// </summary>
    protected virtual string? Validar(T item) => null;

    /// <summary>Ajustes al crear un registro nuevo (valores por defecto).</summary>
    protected virtual T CrearNuevo() => new();

    // ---------- Operaciones ----------

    public void Refrescar()
    {
        EjecutarProtegido(() =>
        {
            var idPrevio = Seleccionado is null ? 0 : ObtenerId(Seleccionado);

            Items.Clear();
            foreach (var item in CargarDesdeRepositorio())
            {
                Items.Add(item);
            }

            // Se intenta devolver la selección a la misma fila que estaba marcada.
            Seleccionado = idPrevio == 0
                ? Items.FirstOrDefault()
                : Items.FirstOrDefault(x => ObtenerId(x) == idPrevio) ?? Items.FirstOrDefault();

            Mensaje = $"{Items.Count} registro(s).";
        });
    }

    private void Nuevo()
    {
        Editable = CrearNuevo();
        _esNuevo = true;
        EnEdicion = true;
        Mensaje = "Nuevo registro.";
    }

    private void Editar()
    {
        if (Seleccionado is null)
        {
            return;
        }

        Editable = Clonar(Seleccionado);
        _esNuevo = false;
        EnEdicion = true;
        Mensaje = $"Editando: {Describir(Seleccionado)}";
    }

    private void Guardar()
    {
        if (Editable is null)
        {
            return;
        }

        var error = Validar(Editable);
        if (error is not null)
        {
            Dialogo.Advertir(error, "Datos incompletos");
            return;
        }

        EjecutarProtegido(() =>
        {
            if (_esNuevo)
            {
                var nuevoId = InsertarEnRepositorio(Editable);
                CerrarFormulario();
                Refrescar();
                Seleccionado = Items.FirstOrDefault(x => ObtenerId(x) == nuevoId) ?? Seleccionado;
                Mensaje = "Registro creado correctamente.";
            }
            else
            {
                ActualizarEnRepositorio(Editable);
                CerrarFormulario();
                Refrescar();
                Mensaje = "Cambios guardados correctamente.";
            }
        });
    }

    private void Cancelar()
    {
        CerrarFormulario();
        Mensaje = "Edición cancelada.";
    }

    private void Eliminar()
    {
        if (Seleccionado is null)
        {
            return;
        }

        var descripcion = Describir(Seleccionado);
        if (!Dialogo.Confirmar($"¿Eliminar «{descripcion}»?", "Confirmar eliminación"))
        {
            return;
        }

        EjecutarProtegido(() =>
        {
            EliminarDelRepositorio(Seleccionado);
            Refrescar();
            Mensaje = $"«{descripcion}» fue eliminado.";
        });
    }

    private void CerrarFormulario()
    {
        EnEdicion = false;
        Editable = null;
        _esNuevo = false;
    }

    /// <summary>
    /// Envuelve las operaciones contra la base de datos: marca el ViewModel
    /// como ocupado y convierte los errores en un mensaje para el usuario en
    /// vez de dejar que revienten la aplicación.
    /// </summary>
    protected void EjecutarProtegido(Action operacion)
    {
        try
        {
            Ocupado = true;
            operacion();
        }
        catch (NeptunoException ex)
        {
            Mensaje = ex.Message;
            Dialogo.Advertir(ex.Message, "No se pudo completar la operación");
        }
        catch (Exception ex)
        {
            Mensaje = ex.Message;
            Dialogo.Advertir($"Error inesperado: {ex.Message}", "Error");
        }
        finally
        {
            Ocupado = false;
        }
    }
}
