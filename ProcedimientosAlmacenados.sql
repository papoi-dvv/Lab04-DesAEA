/* ============================================================
   NeptunoDB - Procedimientos Almacenados
   ------------------------------------------------------------
   Laboratorio 04 - ADO .NET
   Motor: SQL Server (T-SQL)

   Contenido:
     1. CRUD de Categorías
     2. CRUD de Proveedores  (+ búsqueda por NombreContacto y Ciudad)
     3. CRUD de Productos
     4. CRUD de Pedidos      (+ mantenimiento de DetallePedidos)
     5. Reporte de detalles de pedidos por intervalo de fechas
     6. Catálogos de apoyo para los combos de la interfaz
   ============================================================ */

USE NeptunoDB;
GO

/* ============================================================
   1. CATEGORÍAS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Categorias_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  CategoriaID,
            NombreCategoria,
            Descripcion
    FROM    dbo.Categorias
    ORDER BY NombreCategoria;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categorias_ObtenerPorId
    @CategoriaID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  CategoriaID,
            NombreCategoria,
            Descripcion
    FROM    dbo.Categorias
    WHERE   CategoriaID = @CategoriaID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categorias_Insertar
    @NombreCategoria NVARCHAR(30),
    @Descripcion     NVARCHAR(200) = NULL,
    @CategoriaID     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF LTRIM(RTRIM(ISNULL(@NombreCategoria, N''))) = N''
        THROW 50001, N'El nombre de la categoría es obligatorio.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Categorias WHERE NombreCategoria = @NombreCategoria)
        THROW 50002, N'Ya existe una categoría con ese nombre.', 1;

    INSERT INTO dbo.Categorias (NombreCategoria, Descripcion)
    VALUES (@NombreCategoria, @Descripcion);

    SET @CategoriaID = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categorias_Actualizar
    @CategoriaID     INT,
    @NombreCategoria NVARCHAR(30),
    @Descripcion     NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaID = @CategoriaID)
        THROW 50003, N'La categoría indicada no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Categorias
               WHERE NombreCategoria = @NombreCategoria AND CategoriaID <> @CategoriaID)
        THROW 50002, N'Ya existe otra categoría con ese nombre.', 1;

    UPDATE  dbo.Categorias
    SET     NombreCategoria = @NombreCategoria,
            Descripcion     = @Descripcion
    WHERE   CategoriaID     = @CategoriaID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Categorias_Eliminar
    @CategoriaID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaID = @CategoriaID)
        THROW 50003, N'La categoría indicada no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Productos WHERE CategoriaID = @CategoriaID)
        THROW 50004, N'No se puede eliminar: la categoría tiene productos asociados.', 1;

    DELETE FROM dbo.Categorias WHERE CategoriaID = @CategoriaID;
END
GO


/* ============================================================
   2. PROVEEDORES
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ProveedorID, CompaniaNombre, NombreContacto, CargoContacto,
            Direccion, Ciudad, CodigoPostal, Pais, Telefono, Fax
    FROM    dbo.Proveedores
    ORDER BY CompaniaNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_ObtenerPorId
    @ProveedorID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ProveedorID, CompaniaNombre, NombreContacto, CargoContacto,
            Direccion, Ciudad, CodigoPostal, Pais, Telefono, Fax
    FROM    dbo.Proveedores
    WHERE   ProveedorID = @ProveedorID;
END
GO

/* ------------------------------------------------------------
   Listado de proveedores buscando por NombreContacto y Ciudad.
   Ambos filtros son opcionales: si llegan NULL o vacíos se
   ignoran, de modo que el mismo SP sirve para búsqueda por un
   solo criterio, por ambos combinados, o para listar todo.
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_Buscar
    @NombreContacto NVARCHAR(40) = NULL,
    @Ciudad         NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @NombreContacto = NULLIF(LTRIM(RTRIM(@NombreContacto)), N'');
    SET @Ciudad         = NULLIF(LTRIM(RTRIM(@Ciudad)), N'');

    SELECT  ProveedorID, CompaniaNombre, NombreContacto, CargoContacto,
            Direccion, Ciudad, CodigoPostal, Pais, Telefono, Fax
    FROM    dbo.Proveedores
    WHERE  (@NombreContacto IS NULL OR NombreContacto LIKE N'%' + @NombreContacto + N'%')
      AND  (@Ciudad         IS NULL OR Ciudad         LIKE N'%' + @Ciudad         + N'%')
    ORDER BY CompaniaNombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_Insertar
    @CompaniaNombre NVARCHAR(60),
    @NombreContacto NVARCHAR(40) = NULL,
    @CargoContacto  NVARCHAR(40) = NULL,
    @Direccion      NVARCHAR(80) = NULL,
    @Ciudad         NVARCHAR(30) = NULL,
    @CodigoPostal   NVARCHAR(10) = NULL,
    @Pais           NVARCHAR(30) = NULL,
    @Telefono       NVARCHAR(24) = NULL,
    @Fax            NVARCHAR(24) = NULL,
    @ProveedorID    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF LTRIM(RTRIM(ISNULL(@CompaniaNombre, N''))) = N''
        THROW 50010, N'El nombre de la compañía es obligatorio.', 1;

    INSERT INTO dbo.Proveedores
        (CompaniaNombre, NombreContacto, CargoContacto, Direccion,
         Ciudad, CodigoPostal, Pais, Telefono, Fax)
    VALUES
        (@CompaniaNombre, @NombreContacto, @CargoContacto, @Direccion,
         @Ciudad, @CodigoPostal, @Pais, @Telefono, @Fax);

    SET @ProveedorID = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_Actualizar
    @ProveedorID    INT,
    @CompaniaNombre NVARCHAR(60),
    @NombreContacto NVARCHAR(40) = NULL,
    @CargoContacto  NVARCHAR(40) = NULL,
    @Direccion      NVARCHAR(80) = NULL,
    @Ciudad         NVARCHAR(30) = NULL,
    @CodigoPostal   NVARCHAR(10) = NULL,
    @Pais           NVARCHAR(30) = NULL,
    @Telefono       NVARCHAR(24) = NULL,
    @Fax            NVARCHAR(24) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID)
        THROW 50011, N'El proveedor indicado no existe.', 1;

    UPDATE  dbo.Proveedores
    SET     CompaniaNombre = @CompaniaNombre,
            NombreContacto = @NombreContacto,
            CargoContacto  = @CargoContacto,
            Direccion      = @Direccion,
            Ciudad         = @Ciudad,
            CodigoPostal   = @CodigoPostal,
            Pais           = @Pais,
            Telefono       = @Telefono,
            Fax            = @Fax
    WHERE   ProveedorID    = @ProveedorID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Proveedores_Eliminar
    @ProveedorID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID)
        THROW 50011, N'El proveedor indicado no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Productos WHERE ProveedorID = @ProveedorID)
        THROW 50012, N'No se puede eliminar: el proveedor tiene productos asociados.', 1;

    DELETE FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID;
END
GO


/* ============================================================
   3. PRODUCTOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Productos_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  p.ProductoID,
            p.NombreProducto,
            p.ProveedorID,
            pr.CompaniaNombre  AS NombreProveedor,
            p.CategoriaID,
            c.NombreCategoria,
            p.CantidadPorUnidad,
            p.PrecioUnidad,
            p.UnidadesEnExistencia,
            p.UnidadesEnPedido,
            p.NivelDeReorden,
            p.Descontinuado
    FROM        dbo.Productos    p
    LEFT JOIN   dbo.Proveedores pr ON pr.ProveedorID = p.ProveedorID
    LEFT JOIN   dbo.Categorias   c ON c.CategoriaID  = p.CategoriaID
    ORDER BY p.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Productos_ObtenerPorId
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  p.ProductoID,
            p.NombreProducto,
            p.ProveedorID,
            pr.CompaniaNombre  AS NombreProveedor,
            p.CategoriaID,
            c.NombreCategoria,
            p.CantidadPorUnidad,
            p.PrecioUnidad,
            p.UnidadesEnExistencia,
            p.UnidadesEnPedido,
            p.NivelDeReorden,
            p.Descontinuado
    FROM        dbo.Productos    p
    LEFT JOIN   dbo.Proveedores pr ON pr.ProveedorID = p.ProveedorID
    LEFT JOIN   dbo.Categorias   c ON c.CategoriaID  = p.CategoriaID
    WHERE   p.ProductoID = @ProductoID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Productos_Insertar
    @NombreProducto       NVARCHAR(60),
    @ProveedorID          INT           = NULL,
    @CategoriaID          INT           = NULL,
    @CantidadPorUnidad    NVARCHAR(30)  = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT      = 0,
    @UnidadesEnPedido     SMALLINT      = 0,
    @NivelDeReorden       SMALLINT      = 0,
    @Descontinuado        BIT           = 0,
    @ProductoID           INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF LTRIM(RTRIM(ISNULL(@NombreProducto, N''))) = N''
        THROW 50020, N'El nombre del producto es obligatorio.', 1;

    IF @PrecioUnidad < 0
        THROW 50021, N'El precio unitario no puede ser negativo.', 1;

    IF @ProveedorID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Proveedores WHERE ProveedorID = @ProveedorID)
        THROW 50022, N'El proveedor indicado no existe.', 1;

    IF @CategoriaID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaID = @CategoriaID)
        THROW 50023, N'La categoría indicada no existe.', 1;

    INSERT INTO dbo.Productos
        (NombreProducto, ProveedorID, CategoriaID, CantidadPorUnidad, PrecioUnidad,
         UnidadesEnExistencia, UnidadesEnPedido, NivelDeReorden, Descontinuado)
    VALUES
        (@NombreProducto, @ProveedorID, @CategoriaID, @CantidadPorUnidad, @PrecioUnidad,
         @UnidadesEnExistencia, @UnidadesEnPedido, @NivelDeReorden, @Descontinuado);

    SET @ProductoID = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Productos_Actualizar
    @ProductoID           INT,
    @NombreProducto       NVARCHAR(60),
    @ProveedorID          INT           = NULL,
    @CategoriaID          INT           = NULL,
    @CantidadPorUnidad    NVARCHAR(30)  = NULL,
    @PrecioUnidad         DECIMAL(10,2) = 0,
    @UnidadesEnExistencia SMALLINT      = 0,
    @UnidadesEnPedido     SMALLINT      = 0,
    @NivelDeReorden       SMALLINT      = 0,
    @Descontinuado        BIT           = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoID = @ProductoID)
        THROW 50024, N'El producto indicado no existe.', 1;

    IF @PrecioUnidad < 0
        THROW 50021, N'El precio unitario no puede ser negativo.', 1;

    UPDATE  dbo.Productos
    SET     NombreProducto       = @NombreProducto,
            ProveedorID          = @ProveedorID,
            CategoriaID          = @CategoriaID,
            CantidadPorUnidad    = @CantidadPorUnidad,
            PrecioUnidad         = @PrecioUnidad,
            UnidadesEnExistencia = @UnidadesEnExistencia,
            UnidadesEnPedido     = @UnidadesEnPedido,
            NivelDeReorden       = @NivelDeReorden,
            Descontinuado        = @Descontinuado
    WHERE   ProductoID           = @ProductoID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Productos_Eliminar
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoID = @ProductoID)
        THROW 50024, N'El producto indicado no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.DetallePedidos WHERE ProductoID = @ProductoID)
        THROW 50025, N'No se puede eliminar: el producto figura en pedidos registrados.', 1;

    DELETE FROM dbo.Productos WHERE ProductoID = @ProductoID;
END
GO


/* ============================================================
   4. PEDIDOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Pedidos_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ped.PedidoID,
            ped.ClienteID,
            cli.Empresa                              AS NombreCliente,
            ped.EmpleadoID,
            emp.Nombre + N' ' + emp.Apellidos        AS NombreEmpleado,
            ped.FechaPedido,
            ped.FechaRequerida,
            ped.FechaEnvio,
            ped.TransportistaID,
            tra.CompaniaNombre                       AS NombreTransportista,
            ped.Destinatario,
            ped.CiudadDestino,
            ped.PaisDestino,
            ISNULL(det.Total, 0)                     AS Total
    FROM        dbo.Pedidos         ped
    LEFT JOIN   dbo.Clientes        cli ON cli.ClienteID       = ped.ClienteID
    LEFT JOIN   dbo.Empleados       emp ON emp.EmpleadoID      = ped.EmpleadoID
    LEFT JOIN   dbo.Transportistas  tra ON tra.TransportistaID = ped.TransportistaID
    OUTER APPLY (
        SELECT SUM(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento)) AS Total
        FROM   dbo.DetallePedidos d
        WHERE  d.PedidoID = ped.PedidoID
    ) det
    ORDER BY ped.FechaPedido DESC, ped.PedidoID DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedidos_ObtenerPorId
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  ped.PedidoID,
            ped.ClienteID,
            cli.Empresa                              AS NombreCliente,
            ped.EmpleadoID,
            emp.Nombre + N' ' + emp.Apellidos        AS NombreEmpleado,
            ped.FechaPedido,
            ped.FechaRequerida,
            ped.FechaEnvio,
            ped.TransportistaID,
            tra.CompaniaNombre                       AS NombreTransportista,
            ped.Destinatario,
            ped.CiudadDestino,
            ped.PaisDestino,
            ISNULL(det.Total, 0)                     AS Total
    FROM        dbo.Pedidos         ped
    LEFT JOIN   dbo.Clientes        cli ON cli.ClienteID       = ped.ClienteID
    LEFT JOIN   dbo.Empleados       emp ON emp.EmpleadoID      = ped.EmpleadoID
    LEFT JOIN   dbo.Transportistas  tra ON tra.TransportistaID = ped.TransportistaID
    OUTER APPLY (
        SELECT SUM(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento)) AS Total
        FROM   dbo.DetallePedidos d
        WHERE  d.PedidoID = ped.PedidoID
    ) det
    WHERE   ped.PedidoID = @PedidoID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedidos_Insertar
    @ClienteID       INT          = NULL,
    @EmpleadoID      INT          = NULL,
    @FechaPedido     DATE,
    @FechaRequerida  DATE         = NULL,
    @FechaEnvio      DATE         = NULL,
    @TransportistaID INT          = NULL,
    @Destinatario    NVARCHAR(60) = NULL,
    @CiudadDestino   NVARCHAR(30) = NULL,
    @PaisDestino     NVARCHAR(30) = NULL,
    @PedidoID        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaRequerida IS NOT NULL AND @FechaRequerida < @FechaPedido
        THROW 50030, N'La fecha requerida no puede ser anterior a la fecha del pedido.', 1;

    IF @FechaEnvio IS NOT NULL AND @FechaEnvio < @FechaPedido
        THROW 50031, N'La fecha de envío no puede ser anterior a la fecha del pedido.', 1;

    INSERT INTO dbo.Pedidos
        (ClienteID, EmpleadoID, FechaPedido, FechaRequerida, FechaEnvio,
         TransportistaID, Destinatario, CiudadDestino, PaisDestino)
    VALUES
        (@ClienteID, @EmpleadoID, @FechaPedido, @FechaRequerida, @FechaEnvio,
         @TransportistaID, @Destinatario, @CiudadDestino, @PaisDestino);

    SET @PedidoID = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Pedidos_Actualizar
    @PedidoID        INT,
    @ClienteID       INT          = NULL,
    @EmpleadoID      INT          = NULL,
    @FechaPedido     DATE,
    @FechaRequerida  DATE         = NULL,
    @FechaEnvio      DATE         = NULL,
    @TransportistaID INT          = NULL,
    @Destinatario    NVARCHAR(60) = NULL,
    @CiudadDestino   NVARCHAR(30) = NULL,
    @PaisDestino     NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID)
        THROW 50032, N'El pedido indicado no existe.', 1;

    IF @FechaRequerida IS NOT NULL AND @FechaRequerida < @FechaPedido
        THROW 50030, N'La fecha requerida no puede ser anterior a la fecha del pedido.', 1;

    IF @FechaEnvio IS NOT NULL AND @FechaEnvio < @FechaPedido
        THROW 50031, N'La fecha de envío no puede ser anterior a la fecha del pedido.', 1;

    UPDATE  dbo.Pedidos
    SET     ClienteID       = @ClienteID,
            EmpleadoID      = @EmpleadoID,
            FechaPedido     = @FechaPedido,
            FechaRequerida  = @FechaRequerida,
            FechaEnvio      = @FechaEnvio,
            TransportistaID = @TransportistaID,
            Destinatario    = @Destinatario,
            CiudadDestino   = @CiudadDestino,
            PaisDestino     = @PaisDestino
    WHERE   PedidoID        = @PedidoID;
END
GO

/* ------------------------------------------------------------
   Eliminar un pedido borra primero su detalle. Se hace dentro de
   una transacción para que la cabecera y el detalle desaparezcan
   juntos o no desaparezca nada.
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.usp_Pedidos_Eliminar
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID)
        THROW 50032, N'El pedido indicado no existe.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.DetallePedidos WHERE PedidoID = @PedidoID;
        DELETE FROM dbo.Pedidos        WHERE PedidoID = @PedidoID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


/* ============================================================
   4.1  DETALLE DE PEDIDOS
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedidos_ListarPorPedido
    @PedidoID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  d.PedidoID,
            d.ProductoID,
            p.NombreProducto,
            d.PrecioUnidad,
            d.Cantidad,
            d.Descuento,
            CAST(d.PrecioUnidad * d.Cantidad * (1 - d.Descuento) AS DECIMAL(12,2)) AS Subtotal
    FROM        dbo.DetallePedidos d
    INNER JOIN  dbo.Productos      p ON p.ProductoID = d.ProductoID
    WHERE   d.PedidoID = @PedidoID
    ORDER BY p.NombreProducto;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedidos_Insertar
    @PedidoID     INT,
    @ProductoID   INT,
    @PrecioUnidad DECIMAL(10,2),
    @Cantidad     SMALLINT,
    @Descuento    DECIMAL(4,2) = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Pedidos WHERE PedidoID = @PedidoID)
        THROW 50032, N'El pedido indicado no existe.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE ProductoID = @ProductoID)
        THROW 50024, N'El producto indicado no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.DetallePedidos
               WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID)
        THROW 50040, N'El producto ya está registrado en este pedido.', 1;

    IF @Cantidad <= 0
        THROW 50041, N'La cantidad debe ser mayor que cero.', 1;

    IF @Descuento < 0 OR @Descuento > 1
        THROW 50042, N'El descuento debe estar entre 0 y 1.', 1;

    INSERT INTO dbo.DetallePedidos (PedidoID, ProductoID, PrecioUnidad, Cantidad, Descuento)
    VALUES (@PedidoID, @ProductoID, @PrecioUnidad, @Cantidad, @Descuento);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedidos_Actualizar
    @PedidoID     INT,
    @ProductoID   INT,
    @PrecioUnidad DECIMAL(10,2),
    @Cantidad     SMALLINT,
    @Descuento    DECIMAL(4,2) = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.DetallePedidos
                   WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID)
        THROW 50043, N'La línea de detalle indicada no existe.', 1;

    IF @Cantidad <= 0
        THROW 50041, N'La cantidad debe ser mayor que cero.', 1;

    IF @Descuento < 0 OR @Descuento > 1
        THROW 50042, N'El descuento debe estar entre 0 y 1.', 1;

    UPDATE  dbo.DetallePedidos
    SET     PrecioUnidad = @PrecioUnidad,
            Cantidad     = @Cantidad,
            Descuento    = @Descuento
    WHERE   PedidoID     = @PedidoID
      AND   ProductoID   = @ProductoID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DetallePedidos_Eliminar
    @PedidoID   INT,
    @ProductoID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.DetallePedidos
                   WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID)
        THROW 50043, N'La línea de detalle indicada no existe.', 1;

    DELETE FROM dbo.DetallePedidos
    WHERE PedidoID = @PedidoID AND ProductoID = @ProductoID;
END
GO


/* ============================================================
   5. REPORTES
   ============================================================ */

/* ------------------------------------------------------------
   Listado de detalles de pedidos haciendo INNER JOIN con Pedidos,
   filtrando por un intervalo de fechas sobre FechaPedido.
   Ambos extremos son opcionales.
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.usp_Reporte_DetallePedidos_PorFechas
    @FechaInicio DATE = NULL,
    @FechaFin    DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio IS NOT NULL AND @FechaFin IS NOT NULL AND @FechaInicio > @FechaFin
        THROW 50050, N'La fecha inicial no puede ser mayor que la fecha final.', 1;

    SELECT  ped.PedidoID,
            ped.FechaPedido,
            ped.FechaRequerida,
            ped.FechaEnvio,
            cli.Empresa                              AS NombreCliente,
            emp.Nombre + N' ' + emp.Apellidos        AS NombreEmpleado,
            det.ProductoID,
            pro.NombreProducto,
            cat.NombreCategoria,
            det.PrecioUnidad,
            det.Cantidad,
            det.Descuento,
            CAST(det.PrecioUnidad * det.Cantidad * (1 - det.Descuento) AS DECIMAL(12,2)) AS Subtotal,
            ped.CiudadDestino,
            ped.PaisDestino
    FROM        dbo.DetallePedidos det
    INNER JOIN  dbo.Pedidos        ped ON ped.PedidoID   = det.PedidoID
    INNER JOIN  dbo.Productos      pro ON pro.ProductoID = det.ProductoID
    LEFT JOIN   dbo.Categorias     cat ON cat.CategoriaID = pro.CategoriaID
    LEFT JOIN   dbo.Clientes       cli ON cli.ClienteID  = ped.ClienteID
    LEFT JOIN   dbo.Empleados      emp ON emp.EmpleadoID = ped.EmpleadoID
    WHERE  (@FechaInicio IS NULL OR ped.FechaPedido >= @FechaInicio)
      AND  (@FechaFin    IS NULL OR ped.FechaPedido <= @FechaFin)
    ORDER BY ped.FechaPedido, ped.PedidoID, pro.NombreProducto;
END
GO


/* ============================================================
   6. CATÁLOGOS DE APOYO
   ------------------------------------------------------------
   Alimentan los ComboBox de la interfaz WPF.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.usp_Clientes_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ClienteID, Empresa, NombreContacto, Ciudad, Pais, Telefono
    FROM   dbo.Clientes
    ORDER BY Empresa;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Empleados_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EmpleadoID,
           Nombre,
           Apellidos,
           Nombre + N' ' + Apellidos AS NombreCompleto,
           Cargo,
           Ciudad,
           Pais
    FROM   dbo.Empleados
    ORDER BY Apellidos, Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Transportistas_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TransportistaID, CompaniaNombre, Telefono
    FROM   dbo.Transportistas
    ORDER BY CompaniaNombre;
END
GO


/* ============================================================
   VERIFICACIÓN
   ============================================================ */
SELECT  name AS ProcedimientoAlmacenado,
        create_date AS Creado
FROM    sys.procedures
WHERE   name LIKE 'usp[_]%'
ORDER BY name;
GO
