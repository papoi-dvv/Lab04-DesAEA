namespace Neptuno.Entidades;

/// <summary>
/// Entidades de apoyo que alimentan los ComboBox del mantenimiento de pedidos.
/// No tienen mantenimiento propio en este laboratorio.
/// </summary>
public class Cliente
{
    public int ClienteID { get; set; }
    public string Empresa { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }

    public override string ToString() => Empresa;
}

public class Empleado
{
    public int EmpleadoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }

    public override string ToString() => NombreCompleto;
}

public class Transportista
{
    public int TransportistaID { get; set; }
    public string CompaniaNombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }

    public override string ToString() => CompaniaNombre;
}
