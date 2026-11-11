/* =============================================================================
   TiendaUNNE - 2. Procedimientos almacenados (11)
   Taller de Programación II - UNNE
   -----------------------------------------------------------------------------
   Ejecutar DESPUÉS de 1_CrearBaseYTablas.sql.

   Los 11 procedimientos que usa la aplicación:
      1. sp_ExisteEmailPersona      indica si un email ya lo tiene otra persona
      2. sp_DarDeBajaCategoria      baja de una categoría sin productos activos
      3. sp_DarDeBajaProducto       baja de un producto que ningún ticket tiene reservado
      4. sp_ObtenerOCrearCliente    busca un cliente por DNI o lo crea
      5. sp_ReporteClientes         datos del reporte de clientes (neto de notas de crédito)
      6. sp_CrearCategoria          alta de categoría
      7. sp_ActualizarCategoria     edición de categoría
      8. sp_CrearProducto           alta de producto
      9. sp_ActualizarProducto      edición de producto
     10. sp_CrearUsuario            alta de persona + usuario
     11. sp_ActualizarUsuario       edición de persona + usuario

   No borra tablas ni modifica filas: solo (re)crea los procedimientos, así que se
   puede correr más de una vez. La aplicación también los instala sola cada vez que
   arranca, por eso este script es para poder verlos en SSMS antes de abrirla.
   ============================================================================= */

-- Se guardan con el procedimiento: hacen falta porque Cliente tiene un índice filtrado.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- sp_ExisteEmailPersona
-- Indica si el email ya lo tiene registrado otra persona.
--
--   @email               email a buscar (llega ya recortado desde Negocio)
--   @id_persona_excluir  persona que se está editando, para que su propio email
--                        no cuente como repetido. NULL en un alta.
--
-- Devuelve una fila con la columna "existe": 1 si está repetido, 0 si no.
-- La comparación no distingue mayúsculas porque la base usa collation
-- insensible a mayúsculas: "Juan@Mail.com" y "juan@mail.com" son el mismo.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_ExisteEmailPersona', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ExisteEmailPersona;
GO

CREATE PROCEDURE dbo.sp_ExisteEmailPersona
    @email               NVARCHAR(150),
    @id_persona_excluir  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(
             CASE WHEN EXISTS (
                 SELECT 1
                 FROM   dbo.Persona
                 WHERE  email = @email
                   AND  (@id_persona_excluir IS NULL OR id_persona <> @id_persona_excluir)
             )
             THEN 1 ELSE 0 END
           AS BIT) AS existe;
END
GO

-------------------------------------------------------------------------------
-- sp_DarDeBajaCategoria
-- Da de baja (baja lógica) una categoría, siempre que no tenga productos activos.
--
--   @id_categoria       categoría a dar de baja
--   @productos_activos  (salida) cuántos productos activos tiene la categoría
--   @filas              (salida) 1 si se dio de baja; 0 si no se tocó nada
--                       (tenía productos activos, ya estaba inactiva o no existe)
--
-- La cuenta de productos y el UPDATE van juntos: el bloqueo evita que otro puesto
-- agregue un producto a la categoría justo en el medio. La auditoría la graba la
-- aplicación en la misma transacción.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_DarDeBajaCategoria', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DarDeBajaCategoria;
GO

CREATE PROCEDURE dbo.sp_DarDeBajaCategoria
    @id_categoria       INT,
    @productos_activos  INT OUTPUT,
    @filas              INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @filas = 0;

    SELECT @productos_activos = COUNT(*)
    FROM   dbo.Producto WITH (UPDLOCK, HOLDLOCK)
    WHERE  id_categoria = @id_categoria AND activo = 1;

    IF @productos_activos > 0
        RETURN;

    UPDATE dbo.Categoria
    SET    activo = 0
    WHERE  id_categoria = @id_categoria AND activo = 1;

    SET @filas = @@ROWCOUNT;
END
GO
-------------------------------------------------------------------------------
-- sp_DarDeBajaProducto
-- Da de baja (baja lógica) un producto, siempre que ningún ticket en curso lo tenga
-- reservado.
--
--   @id_producto  producto a dar de baja
--   @reservado    (salida) unidades que hay reservadas ahora por tickets en curso
--   @filas        (salida) 1 si se dio de baja; 0 si no se tocó nada
--                 (estaba reservado, ya estaba inactivo o no existe)
--
-- Primero se bloquea la fila del producto, igual que hace la reserva de stock: así una
-- caja que justo está agregando el producto al ticket y esta baja no se pisan.
-- Las reservas vencidas no cuentan. La auditoría la graba la aplicación en la misma
-- transacción.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_DarDeBajaProducto', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DarDeBajaProducto;
GO

CREATE PROCEDURE dbo.sp_DarDeBajaProducto
    @id_producto  INT,
    @reservado    DECIMAL(12,3) OUTPUT,
    @filas        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @filas = 0;

    -- Bloquea el producto (si no existe, no hay nada que dar de baja).
    IF NOT EXISTS (SELECT 1 FROM dbo.Producto WITH (UPDLOCK, HOLDLOCK) WHERE id_producto = @id_producto)
    BEGIN
        SET @reservado = 0;
        RETURN;
    END

    SELECT @reservado = ISNULL(SUM(cantidad), 0)
    FROM   dbo.Reserva_stock
    WHERE  id_producto = @id_producto AND vence > GETDATE();

    IF @reservado > 0
        RETURN;

    UPDATE dbo.Producto
    SET    activo = 0
    WHERE  id_producto = @id_producto AND activo = 1;

    SET @filas = @@ROWCOUNT;
END
GO
-------------------------------------------------------------------------------
-- sp_ObtenerOCrearCliente
-- Devuelve el cliente con ese DNI; si no existe, lo crea.
--
--   Si la persona (DNI) no existe, se crea la Persona y el Cliente.
--   Si la persona ya existe (por ejemplo, es un usuario del sistema) pero todavía no
--   es cliente, solo se le agrega el Cliente: no se pisan sus datos.
--   Si ya es cliente, se devuelve el existente (y se reactiva si estaba dado de baja).
--
--   @id_cliente  (salida) id del cliente encontrado o creado
--   @creado      (salida) 1 si se creó ahora; 0 si ya existía
--
-- Se llama dentro de una transacción de la aplicación: la Persona y el Cliente se crean
-- juntos o no se crea ninguno, y la auditoría se graba en esa misma transacción.
-- Un CUIT repetido lo rechaza el índice único UQ_Cliente_cuit.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_ObtenerOCrearCliente', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ObtenerOCrearCliente;
GO

CREATE PROCEDURE dbo.sp_ObtenerOCrearCliente
    @dni            NVARCHAR(20),
    @nombre         NVARCHAR(100),
    @apellido       NVARCHAR(100),
    @email          NVARCHAR(150) = NULL,
    @telefono       NVARCHAR(30)  = NULL,
    @razon_social   NVARCHAR(150) = NULL,
    @cuit           NVARCHAR(11)  = NULL,
    @condicion_iva  NVARCHAR(30),
    @id_cliente     INT OUTPUT,
    @creado         BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @id_persona INT;

    SET @creado = 0;
    SET @id_cliente = NULL;

    SELECT @id_persona = id_persona
    FROM   dbo.Persona WITH (UPDLOCK, HOLDLOCK)
    WHERE  dni_cuit = @dni;

    IF @id_persona IS NOT NULL
    BEGIN
        SELECT @id_cliente = id_cliente
        FROM   dbo.Cliente WITH (UPDLOCK, HOLDLOCK)
        WHERE  id_persona = @id_persona;

        IF @id_cliente IS NOT NULL
        BEGIN
            UPDATE dbo.Cliente SET activo = 1 WHERE id_cliente = @id_cliente AND activo = 0;
            RETURN;
        END
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, email, telefono)
        VALUES (@dni, @nombre, @apellido, @email, @telefono);

        SET @id_persona = SCOPE_IDENTITY();
    END

    INSERT INTO dbo.Cliente (id_persona, razon_social, cuit, condicion_iva)
    VALUES (@id_persona, @razon_social, @cuit, @condicion_iva);

    SET @id_cliente = SCOPE_IDENTITY();
    SET @creado = 1;
END
GO
-------------------------------------------------------------------------------
-- sp_ReporteClientes
-- Datos del reporte de clientes entre dos fechas. El rango es [@desde, @hasta):
-- incluye @desde y no incluye @hasta. Solo cuentan las ventas emitidas (las anuladas
-- quedan afuera).
--
-- Devuelve dos resultados:
--   1) Una fila por cliente que compró: Cliente, Compras, Total gastado, Promedio por
--      compra y fecha de la Ultima compra; del que más gastó al que menos.
--   2) Una sola fila de totales: ClientesNuevos (dados de alta en el rango),
--      VentasSinCliente (consumidor final) y VentasTotales.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_ReporteClientes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ReporteClientes;
GO

CREATE PROCEDURE dbo.sp_ReporteClientes
    @desde  DATETIME2,
    @hasta  DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Cada tipo de comprobante tiene un "signo": 1 en una venta y -1 en una nota de crédito.
    -- Lo gastado es neto de devoluciones, y una nota de crédito no cuenta como compra.
    -- Solo figuran los clientes que compraron en el rango.
    SELECT  c.id_cliente                    AS IdCliente,
            p.apellido + N', ' + p.nombre   AS Cliente,
            COUNT(CASE WHEN tc.signo = 1 THEN 1 END)  AS Compras,
            SUM(vc.total * tc.signo)                  AS Total,
            ISNULL(SUM(vc.total * tc.signo)
                   / NULLIF(COUNT(CASE WHEN tc.signo = 1 THEN 1 END), 0), 0) AS Promedio,
            MAX(vc.fecha_hora)                        AS Ultima
    FROM        dbo.Venta_cabecera   vc
    INNER JOIN  dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
    INNER JOIN  dbo.Cliente          c  ON c.id_cliente = vc.id_cliente
    INNER JOIN  dbo.Persona          p  ON p.id_persona = c.id_persona
    WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta
    GROUP BY c.id_cliente, p.apellido, p.nombre
    HAVING COUNT(CASE WHEN tc.signo = 1 THEN 1 END) > 0
    ORDER BY SUM(vc.total * tc.signo) DESC, p.apellido, p.nombre;

    SELECT
        (SELECT COUNT(*) FROM dbo.Cliente
          WHERE fecha_alta >= @desde AND fecha_alta < @hasta)                      AS ClientesNuevos,
        (SELECT COUNT(*) FROM dbo.Venta_cabecera vc
           INNER JOIN dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
          WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta
            AND tc.signo = 1 AND vc.id_cliente IS NULL)                            AS VentasSinCliente,
        (SELECT COUNT(*) FROM dbo.Venta_cabecera vc
           INNER JOIN dbo.Tipo_comprobante tc ON tc.id_tipo_comprobante = vc.id_tipo_comprobante
          WHERE vc.estado = 'EMITIDA' AND vc.fecha_hora >= @desde AND vc.fecha_hora < @hasta
            AND tc.signo = 1)                                                      AS VentasTotales;
END
GO
-------------------------------------------------------------------------------
-- Altas y ediciones de Categoría, Producto y Usuario
--
-- Los seis procedimientos siguientes solo guardan los datos: la aplicación ya los
-- validó y normalizó (capa Negocio) y graba la auditoría en la misma transacción
-- desde la que los llama. Si un dato repite un valor único (nombre de categoría, DNI),
-- la propia base lo rechaza con su restricción UNIQUE y la aplicación lo traduce.
-------------------------------------------------------------------------------

-- ---- Categoría -------------------------------------------------------------
IF OBJECT_ID('dbo.sp_CrearCategoria', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CrearCategoria;
GO

CREATE PROCEDURE dbo.sp_CrearCategoria
    @nombre        NVARCHAR(100),
    @descripcion   NVARCHAR(200) = NULL,
    @id_categoria  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Categoria (nombre, descripcion)
    VALUES (@nombre, @descripcion);

    SET @id_categoria = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.sp_ActualizarCategoria', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ActualizarCategoria;
GO

CREATE PROCEDURE dbo.sp_ActualizarCategoria
    @id_categoria  INT,
    @nombre        NVARCHAR(100),
    @descripcion   NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Categoria
    SET    nombre = @nombre, descripcion = @descripcion
    WHERE  id_categoria = @id_categoria;
END
GO

-- ---- Producto --------------------------------------------------------------
IF OBJECT_ID('dbo.sp_CrearProducto', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CrearProducto;
GO

CREATE PROCEDURE dbo.sp_CrearProducto
    @id_categoria  INT,
    @nombre        NVARCHAR(150),
    @descripcion   NVARCHAR(500) = NULL,
    @precio_venta  DECIMAL(12,2),
    @stock         DECIMAL(12,3),
    @stock_minimo  DECIMAL(12,3),
    @id_producto   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Producto (id_categoria, nombre, descripcion, precio_venta, stock, stock_minimo)
    VALUES (@id_categoria, @nombre, @descripcion, @precio_venta, @stock, @stock_minimo);

    SET @id_producto = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.sp_ActualizarProducto', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ActualizarProducto;
GO

CREATE PROCEDURE dbo.sp_ActualizarProducto
    @id_producto   INT,
    @id_categoria  INT,
    @nombre        NVARCHAR(150),
    @descripcion   NVARCHAR(500) = NULL,
    @precio_venta  DECIMAL(12,2),
    @stock         DECIMAL(12,3),
    @stock_minimo  DECIMAL(12,3)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Producto
    SET    id_categoria = @id_categoria,
           nombre       = @nombre,
           descripcion  = @descripcion,
           precio_venta = @precio_venta,
           stock        = @stock,
           stock_minimo = @stock_minimo
    WHERE  id_producto = @id_producto;
END
GO

-- ---- Usuario (Persona + Usuario) -------------------------------------------
IF OBJECT_ID('dbo.sp_CrearUsuario', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CrearUsuario;
GO

-- Crea la persona y su usuario juntos. El hash y el salt llegan calculados: la contraseña
-- en claro nunca llega a la base.
CREATE PROCEDURE dbo.sp_CrearUsuario
    @dni               NVARCHAR(20),
    @nombre            NVARCHAR(100),
    @apellido          NVARCHAR(100),
    @direccion         NVARCHAR(200) = NULL,
    @telefono          NVARCHAR(30)  = NULL,
    @email             NVARCHAR(150) = NULL,
    @fecha_nacimiento  DATE          = NULL,
    @id_perfil         INT,
    @hash_password     VARBINARY(256),
    @salt              VARBINARY(128),
    @id_usuario        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @id_persona INT;

    INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
    VALUES (@dni, @nombre, @apellido, @direccion, @telefono, @email, @fecha_nacimiento);

    SET @id_persona = SCOPE_IDENTITY();

    INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt)
    VALUES (@id_persona, @id_perfil, @hash_password, @salt);

    SET @id_usuario = SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.sp_ActualizarUsuario', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ActualizarUsuario;
GO

-- Actualiza la persona y el perfil del usuario. Si @hash_password viene en NULL la
-- contraseña no se toca; si viene con valor, se cambia (y deja de estar pendiente de cambio).
CREATE PROCEDURE dbo.sp_ActualizarUsuario
    @id_usuario        INT,
    @id_persona        INT,
    @dni               NVARCHAR(20),
    @nombre            NVARCHAR(100),
    @apellido          NVARCHAR(100),
    @direccion         NVARCHAR(200) = NULL,
    @telefono          NVARCHAR(30)  = NULL,
    @email             NVARCHAR(150) = NULL,
    @fecha_nacimiento  DATE          = NULL,
    @id_perfil         INT,
    @hash_password     VARBINARY(256) = NULL,
    @salt              VARBINARY(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Persona
    SET    dni_cuit = @dni, nombre = @nombre, apellido = @apellido,
           direccion = @direccion, telefono = @telefono, email = @email,
           fecha_nacimiento = @fecha_nacimiento
    WHERE  id_persona = @id_persona;

    UPDATE dbo.Usuario
    SET    id_perfil = @id_perfil,
           hash_password = ISNULL(@hash_password, hash_password),
           salt = ISNULL(@salt, salt),
           debe_cambiar_pass = CASE WHEN @hash_password IS NULL THEN debe_cambiar_pass ELSE 0 END
    WHERE  id_usuario = @id_usuario;
END
GO
PRINT 'Stored procedures de TiendaUNNE creados correctamente.';
GO
