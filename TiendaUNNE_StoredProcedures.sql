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
PRINT 'Stored procedures de TiendaUNNE creados correctamente.';
GO
