namespace Neptuno.Entidades;

/// <summary>
/// Representa una fila de la tabla dbo.Proveedores.
/// </summary>
public class Proveedor
{
    public int ProveedorID { get; set; }
    public string CompaniaNombre { get; set; } = string.Empty;
    public string? NombreContacto { get; set; }
    public string? CargoContacto { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }
    public string? Fax { get; set; }

    public override string ToString() => CompaniaNombre;
}
