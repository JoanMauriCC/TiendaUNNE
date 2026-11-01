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
PRINT 'Stored procedures de TiendaUNNE creados correctamente.';
GO
