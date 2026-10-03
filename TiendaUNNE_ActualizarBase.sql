/* =============================================================================
   TiendaUNNE - Actualizar una base de datos EXISTENTE a la versión actual
   -----------------------------------------------------------------------------
   Para quien ya tenía la base creada con una versión anterior del proyecto (por
   ejemplo, después de un git pull). Junta TODAS las migraciones en un solo script,
   en el orden correcto, así no hay que saber cuáles faltan:

     1. Columna stock_minimo en Producto
     2. El login pasa a ser por DNI (se quita nombre_usuario de Usuario)
     3. El DNI tiene que tener exactamente 8 dígitos
     4. Datos iniciales de la caja (caja, medios de pago, tipo de comprobante)

   NO borra ni modifica datos: cada paso se fija primero si ya está aplicado. Se
   puede correr las veces que haga falta; en una base que ya está al día no hace
   nada. Orden recomendado después de este script:

     TiendaUNNE_ActualizarBase.sql  ->  TiendaUNNE_StoredProcedures.sql
     (opcional) TiendaUNNE_DatosDePrueba.sql

   NO usar TiendaUNNE_CreateTables.sql para actualizar: ese script borra todas las
   tablas. Es solo para crear una base desde cero.
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Frenos: si la base no se puede actualizar con este script, avisa y no toca nada
-- (SET NOEXEC ON hace que se saltee el resto; cerrar la ventana de consulta lo
-- deshace).
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Persona', 'U') IS NULL OR OBJECT_ID('dbo.Producto', 'U') IS NULL
BEGIN
    RAISERROR('La base no tiene las tablas del sistema. Para crearla de cero ejecutá TiendaUNNE_CreateTables.sql.', 16, 1);
    SET NOEXEC ON;
END
GO

IF COL_LENGTH('dbo.Producto', 'stock_actual') IS NOT NULL
BEGIN
    RAISERROR('Esta base viene de la primera versión del proyecto (Producto con stock_actual, codigo, precio_costo...) y no se puede actualizar con este script. Recreala con TiendaUNNE_CreateTables.sql, que BORRA todo, y después ejecutá TiendaUNNE_StoredProcedures.sql.', 16, 1);
    SET NOEXEC ON;
END
GO

-------------------------------------------------------------------------------
-- 1. Producto.stock_minimo
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.Producto', 'stock_minimo') IS NULL
BEGIN
    ALTER TABLE dbo.Producto
        ADD stock_minimo DECIMAL(12,3) NOT NULL
            CONSTRAINT DF_Producto_stock_minimo DEFAULT (0);

    PRINT '1. Columna stock_minimo agregada.';
END
ELSE
BEGIN
    PRINT '1. La columna stock_minimo ya existía.';
END
GO

IF OBJECT_ID('dbo.CK_Producto_stock_minimo', 'C') IS NULL
BEGIN
    ALTER TABLE dbo.Producto
        ADD CONSTRAINT CK_Producto_stock_minimo CHECK (stock_minimo >= 0);

    PRINT '1. Restricción CK_Producto_stock_minimo agregada.';
END
GO

-------------------------------------------------------------------------------
-- 2. Login por DNI: Usuario ya no tiene nombre_usuario
--    (los nombres de usuario que hubiera cargados se pierden; los usuarios
--     siguen existiendo y ingresan con su DNI)
-------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.objects
           WHERE name = 'UQ_Usuario_nombre'
             AND parent_object_id = OBJECT_ID('dbo.Usuario'))
BEGIN
    ALTER TABLE dbo.Usuario DROP CONSTRAINT UQ_Usuario_nombre;
    PRINT '2. Restricción UQ_Usuario_nombre eliminada.';
END
GO

IF COL_LENGTH('dbo.Usuario', 'nombre_usuario') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Usuario DROP COLUMN nombre_usuario;
    PRINT '2. Columna nombre_usuario eliminada.';
END
ELSE
BEGIN
    PRINT '2. La columna nombre_usuario ya no existía.';
END
GO

-------------------------------------------------------------------------------
-- 3. DNI de exactamente 8 dígitos
--    Si hay DNIs que no cumplen, avisa y NO agrega la restricción: hay que
--    corregirlos a mano y volver a correr este script.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.CK_Persona_dni_cuit', 'C') IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Persona
               WHERE dni_cuit LIKE '%[^0-9]%' OR LEN(dni_cuit) <> 8)
    BEGIN
        PRINT '3. ATENCIÓN: hay personas con un DNI que no tiene exactamente 8 dígitos.';
        PRINT '   Corregilas a mano y volvé a correr este script; la restricción NO se agregó.';

        SELECT id_persona, dni_cuit
        FROM   dbo.Persona
        WHERE  dni_cuit LIKE '%[^0-9]%' OR LEN(dni_cuit) <> 8;
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Persona
            ADD CONSTRAINT CK_Persona_dni_cuit CHECK (dni_cuit NOT LIKE '%[^0-9]%' AND LEN(dni_cuit) = 8);

        PRINT '3. Restricción CK_Persona_dni_cuit agregada.';
    END
END
ELSE
BEGIN
    PRINT '3. La restricción CK_Persona_dni_cuit ya existía.';
END
GO

-------------------------------------------------------------------------------
-- 4. Datos iniciales de la caja (cada fila se inserta solo si todavía no está)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Caja WHERE nombre = N'Caja 1')
    INSERT INTO dbo.Caja (nombre, descripcion)
    VALUES (N'Caja 1', N'Caja principal del local');

INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, es_efectivo)
SELECT m.nombre, m.requiere_referencia, m.es_efectivo
FROM (VALUES
        (N'Efectivo',           0, 1),
        (N'Tarjeta de débito',  1, 0),
        (N'Tarjeta de crédito', 1, 0),
        (N'Transferencia',      1, 0)
     ) AS m (nombre, requiere_referencia, es_efectivo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Medio_pago x WHERE x.nombre = m.nombre);

IF NOT EXISTS (SELECT 1 FROM dbo.Tipo_comprobante WHERE codigo = N'TKT')
    INSERT INTO dbo.Tipo_comprobante (codigo, nombre, letra, signo)
    VALUES (N'TKT', N'Ticket', NULL, 1);
GO

IF OBJECT_ID('dbo.CK_Persona_dni_cuit', 'C') IS NULL
    PRINT 'Quedó pendiente el paso 3 (ver el aviso de arriba). Lo demás está actualizado.';
ELSE
    PRINT 'Base de datos TiendaUNNE actualizada: está al día con la versión actual.';
GO
