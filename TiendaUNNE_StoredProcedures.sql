/* =============================================================================
   TiendaUNNE - Stored Procedures
   -----------------------------------------------------------------------------
   Procedimientos almacenados que usa la aplicación.

   Se puede ejecutar sobre una base YA CREADA y con datos: no borra tablas ni
   modifica filas, solo (re)crea los procedimientos. Se puede correr más de una
   vez sin problema.
   ============================================================================= */

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
-- sp_AnularVenta
-- Anula una venta ya cobrada, dentro del turno de caja que está abierto.
--
--   @id_venta        venta a anular
--   @id_caja_sesion  turno abierto desde el que se anula
--   @id_usuario      quién anula
--   @motivo          por qué se anula (queda guardado en la venta)
--   @resultado       (salida) 0 = anulada
--                             1 = la venta no existe
--                             2 = ya estaba anulada
--                             3 = es de otro turno de caja (no se puede desde este)
--
-- Si se anula: la venta pasa a estado ANULADA, el stock de cada producto vuelve al
-- depósito y se registra un egreso de caja por cada pago, con el mismo medio de pago.
-- Los reportes y el arqueo ya ignoran las ventas anuladas. Se llama dentro de una
-- transacción de la aplicación, que graba también la auditoría.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_AnularVenta', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AnularVenta;
GO

CREATE PROCEDURE dbo.sp_AnularVenta
    @id_venta        INT,
    @id_caja_sesion  INT,
    @id_usuario      INT,
    @motivo          NVARCHAR(300),
    @resultado       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @estado NVARCHAR(10), @sesion_venta INT;

    -- Se bloquea la venta: dos personas no pueden anularla a la vez.
    SELECT @estado = estado, @sesion_venta = id_caja_sesion
    FROM   dbo.Venta_cabecera WITH (UPDLOCK, HOLDLOCK)
    WHERE  id_venta = @id_venta;

    IF @estado IS NULL      BEGIN SET @resultado = 1; RETURN; END
    IF @estado = 'ANULADA'  BEGIN SET @resultado = 2; RETURN; END
    IF @sesion_venta <> @id_caja_sesion BEGIN SET @resultado = 3; RETURN; END

    UPDATE dbo.Venta_cabecera
    SET    estado = 'ANULADA', observaciones = @motivo
    WHERE  id_venta = @id_venta;

    -- El stock vuelve al depósito.
    UPDATE p
    SET    p.stock = p.stock + d.cantidad
    FROM   dbo.Producto p
    INNER JOIN (SELECT id_producto, SUM(cantidad) AS cantidad
                FROM   dbo.Venta_detalle
                WHERE  id_venta = @id_venta
                GROUP BY id_producto) d ON d.id_producto = p.id_producto;

    -- La plata se devuelve por el mismo medio de pago con el que se cobró.
    INSERT INTO dbo.Movimiento_caja
        (id_caja_sesion, id_usuario, id_medio_pago, id_venta, tipo, concepto, monto, observaciones)
    SELECT @id_caja_sesion, @id_usuario, vp.id_medio_pago, @id_venta, 'EGRESO',
           N'Anulación de venta', vp.importe, @motivo
    FROM   dbo.Venta_pago vp
    WHERE  vp.id_venta = @id_venta AND vp.importe > 0;

    SET @resultado = 0;
END
GO
PRINT 'Stored procedures de TiendaUNNE creados correctamente.';
GO
