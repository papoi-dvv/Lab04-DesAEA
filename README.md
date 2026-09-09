# Laboratorio 04 — WPF + ADO .NET sobre NeptunoDB

Curso: Desarrollo de Aplicaciones Empresariales Avanzado
Tema: Acceso a datos con ADO .NET y procedimientos almacenados, interfaz en WPF.

Aplicación de escritorio que implementa los mantenimientos de **productos,
categorías, proveedores y pedidos**, la **búsqueda de proveedores** por nombre
de contacto y ciudad, y el **reporte de detalles de pedidos** filtrado por
intervalo de fechas. Todo el acceso a datos pasa por procedimientos
almacenados; no hay SQL embebido en el código C#.

---

## 1. Requisitos

| Componente | Versión usada |
|---|---|
| .NET SDK | 10.0 |
| Visual Studio | 2026 (18.x) — cualquier edición con carga de trabajo *.NET Desktop* |
| SQL Server | Express 2022 (instancia `.\SQLEXPRESS`) |

Paquetes NuGet (se restauran solos al compilar):

- `Microsoft.Data.SqlClient` — proveedor ADO .NET
- `System.Configuration.ConfigurationManager` — lectura de la cadena de conexión

---

## 2. Puesta en marcha

### 2.1 Crear la base de datos

Ejecutar **en este orden** desde SSMS o desde la línea de comandos:

```bash
sqlcmd -S .\SQLEXPRESS -E -C -i NeptunoDB.sql -f 65001
```

```bash
sqlcmd -S .\SQLEXPRESS -E -C -i ProcedimientosAlmacenados.sql -f 65001
```

> El parámetro `-f 65001` indica la página de códigos UTF-8. Sin él los acentos
> se insertan mal (aparecen como `Per?` o `L?cteos`).
> El parámetro `-C` acepta el certificado autofirmado de SQL Server Express.

El primer script crea las 8 tablas y carga 5 registros de ejemplo en cada una.
El segundo crea los 29 procedimientos almacenados y lista su nombre al final
como verificación.

### 2.2 Configurar la conexión

Si SQL Server no está en `.\SQLEXPRESS`, editar únicamente el atributo `Server`
en `Neptuno.WPF/App.config`:

```xml
<add name="NeptunoDB"
     connectionString="Server=.\SQLEXPRESS;Database=NeptunoDB;Integrated Security=True;TrustServerCertificate=True;"
     providerName="Microsoft.Data.SqlClient" />
```

### 2.3 Ejecutar

Abrir `Neptuno.sln` en Visual Studio, dejar **Neptuno.WPF** como proyecto de
inicio y presionar F5. O bien:

```bash
dotnet run --project Neptuno.WPF
```

Al arrancar, la aplicación prueba la conexión y, si falla, muestra un mensaje
indicando qué revisar en vez de reventar en la primera pantalla.

---

## 3. Estructura de la solución

La solución está separada en tres capas, cada una con una responsabilidad
única y dependencias en un solo sentido:

```
Neptuno.WPF  ──►  Neptuno.Datos  ──►  Neptuno.Entidades
(presentación)     (acceso a datos)    (modelo)
```

### `Neptuno.Entidades`
Clases planas que representan las filas de cada tabla. No tienen lógica ni
dependen de nada: son el lenguaje común entre las otras dos capas.

### `Neptuno.Datos`
Un repositorio por entidad. Todos heredan de `RepositorioBase`, que concentra
el patrón repetitivo de ADO .NET (abrir conexión → armar `SqlCommand` como
`StoredProcedure` → ejecutar → liberar con `using`), de modo que cada
repositorio se limita a declarar sus parámetros y mapear el resultado.

| Archivo | Responsabilidad |
|---|---|
| `ConexionBD.cs` | Resuelve la cadena de conexión y crea conexiones |
| `RepositorioBase.cs` | `Consultar`, `Ejecutar`, `EjecutarConIdentidad` |
| `LectorExtensiones.cs` | Lectura de columnas nulas sin repetir `IsDBNull` |
| `NeptunoException.cs` | Traduce los `THROW` de los SP a errores de negocio |
| `CategoriaRepositorio.cs` | CRUD de categorías |
| `ProveedorRepositorio.cs` | CRUD + búsqueda por contacto y ciudad |
| `ProductoRepositorio.cs` | CRUD de productos |
| `PedidoRepositorio.cs` | CRUD de pedidos y de su detalle |
| `ReporteRepositorio.cs` | Reporte por intervalo de fechas |
| `CatalogoRepositorio.cs` | Listas de apoyo para los ComboBox |

### `Neptuno.WPF`
Interfaz con patrón **MVVM**. Las vistas no tienen lógica en el code-behind:
sólo `InitializeComponent()`. Todo el comportamiento vive en los ViewModels y
se conecta por *binding* y *commands*.

| Carpeta | Contenido |
|---|---|
| `MVVM/` | `ViewModelBase` (INotifyPropertyChanged) y `RelayCommand` (ICommand) |
| `ViewModels/` | Un ViewModel por pantalla + `MantenimientoViewModel<T>` genérico |
| `Vistas/` | Un `UserControl` por pestaña |
| `Servicios/` | `IServicioDialogo` — abstrae los `MessageBox` |
| `Convertidores/` | Convertidores de binding (booleanos, porcentajes) |
| `Recursos/` | `Estilos.xaml` — paleta, botones, grillas |

**`MantenimientoViewModel<T>`** merece mención aparte: los tres mantenimientos
simples (productos, categorías, proveedores) comparten exactamente el mismo
ciclo —listar, seleccionar, nuevo/editar, guardar o cancelar, eliminar— así que
esa lógica se escribió una sola vez en una clase base genérica. Cada ViewModel
concreto sólo declara cómo hablar con su repositorio y cómo validar su entidad.

---

## 4. Procedimientos almacenados

29 en total, todos con el prefijo `usp_`.

### CRUD

| Entidad | Procedimientos |
|---|---|
| Categorías | `usp_Categorias_Listar`, `_ObtenerPorId`, `_Insertar`, `_Actualizar`, `_Eliminar` |
| Proveedores | `usp_Proveedores_Listar`, `_ObtenerPorId`, `_Insertar`, `_Actualizar`, `_Eliminar` |
| Productos | `usp_Productos_Listar`, `_ObtenerPorId`, `_Insertar`, `_Actualizar`, `_Eliminar` |
| Pedidos | `usp_Pedidos_Listar`, `_ObtenerPorId`, `_Insertar`, `_Actualizar`, `_Eliminar` |
| Detalle | `usp_DetallePedidos_ListarPorPedido`, `_Insertar`, `_Actualizar`, `_Eliminar` |

### Consultas pedidas por el enunciado

**`usp_Proveedores_Buscar @NombreContacto, @Ciudad`**
Listado de proveedores filtrando por nombre de contacto y ciudad. Ambos
parámetros son opcionales y se combinan; un filtro vacío se envía como `NULL`
y el `WHERE` lo ignora:

```sql
WHERE  (@NombreContacto IS NULL OR NombreContacto LIKE N'%' + @NombreContacto + N'%')
  AND  (@Ciudad         IS NULL OR Ciudad         LIKE N'%' + @Ciudad         + N'%')
```

Así un mismo procedimiento sirve para buscar por un criterio, por ambos, o
para listar todo, en lugar de escribir tres variantes.

**`usp_Reporte_DetallePedidos_PorFechas @FechaInicio, @FechaFin`**
Detalles de pedidos con `INNER JOIN` contra `Pedidos`, filtrando por intervalo
de fechas sobre `FechaPedido`. Agrega por `LEFT JOIN` los nombres de producto,
categoría, cliente y empleado, y calcula el subtotal de cada línea.

### Catálogos de apoyo
`usp_Clientes_Listar`, `usp_Empleados_Listar`, `usp_Transportistas_Listar`
alimentan los ComboBox del mantenimiento de pedidos.

### Validaciones
Los procedimientos protegen la integridad con `THROW` y números ≥ 50000, que
la capa de datos convierte en `NeptunoException` y la interfaz muestra tal cual:

- No se puede borrar una categoría o proveedor con productos asociados.
- No se puede borrar un producto que figura en un pedido.
- No se admite el mismo producto dos veces en un pedido.
- Precio no negativo; cantidad mayor que cero; descuento entre 0 y 1.
- La fecha requerida y la de envío no pueden ser anteriores a la del pedido.
- La fecha inicial del reporte no puede ser mayor que la final.

Eliminar un pedido borra su detalle y su cabecera dentro de una **transacción**,
de modo que o desaparecen ambos o no desaparece nada.

---

## 5. Funcionalidades implementadas

| Requisito del enunciado | Dónde |
|---|---|
| Mantenimiento de productos | Pestaña **Productos** |
| Mantenimiento de categorías | Pestaña **Categorías** |
| Mantenimiento de proveedores | Pestaña **Proveedores** |
| Búsqueda de proveedores con filtros | Barra de búsqueda en **Proveedores** |
| Mantenimiento de pedidos | Pestaña **Pedidos** (maestro-detalle) |
| Reportes con filtros de fecha | Pestaña **Reportes** |

Detalles adicionales:

- **Pedidos** es una pantalla maestro-detalle. La cabecera y las líneas se
  graban por separado porque un pedido necesita existir (y tener `PedidoID`)
  antes de poder agregarle productos: `DetallePedidos` tiene una clave foránea
  hacia `Pedidos`.
- Al elegir un producto en el detalle se propone su precio de lista, que el
  usuario todavía puede modificar (el precio queda congelado en el pedido).
- El reporte muestra pedidos, unidades y total del período, y permite
  **exportar a CSV** en UTF-8 con BOM para que Excel respete los acentos.
- La cultura se fija en `es-PE`, de modo que fechas y montos se muestran y se
  escriben en formato local.

---

## 6. Nota sobre el script original

El archivo `NeptunoDB.sql` entregado con el enunciado tenía los caracteres
acentuados corrompidos: contenía 52 ocurrencias del carácter U+FFFD
(el rombo con signo de interrogación) donde debían ir `á é í ó ú ñ`. Ejecutado
tal cual, la base quedaba con `L?cteos`, `Per?`, `Ana Garc?a`, etc.

Se corrigieron las 28 palabras afectadas y el archivo se guardó en UTF-8 con
BOM. El original quedó como `NeptunoDB.original.bak` por si se necesita
comparar. El esquema, los datos y la estructura no se modificaron: sólo la
codificación de los textos.

---

## 7. Verificación

La capa de datos se validó con una prueba end-to-end que recorre altas,
lecturas, modificaciones y bajas de las cuatro entidades, comprueba los
cálculos de subtotales y confirma que cada regla de negocio rechace lo que
debe rechazar. Las 43 verificaciones pasaron y los datos de prueba se
eliminaron al terminar, dejando la base en su estado inicial.

---

## 8. Contenido de la entrega

```
Lab04/
├── Neptuno.sln
├── NeptunoDB.sql                    ← script de base de datos (corregido)
├── NeptunoDB.original.bak           ← original del enunciado, sin tocar
├── ProcedimientosAlmacenados.sql    ← los 29 procedimientos
├── README.md
├── Capturas/                        ← imágenes de las vistas
├── Neptuno.Entidades/
├── Neptuno.Datos/
└── Neptuno.WPF/
```
