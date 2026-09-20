/* =============================================================================
   TiendaUNNE - Migración: columna stock_minimo en Producto
   -----------------------------------------------------------------------------
   Agrega la columna stock_minimo (umbral de "stock bajo") a una base YA CREADA,
   sin borrar nada. Es el equivalente al cambio que se hizo en
   TiendaUNNE_CreateTables.sql, pero para bases que ya tienen datos cargados.

   NO ejecutar TiendaUNNE_CreateTables.sql para obtener esta columna: ese script
   hace DROP de todas las tablas y se perderían usuarios, productos y ventas.

   Se puede correr más de una vez: si la columna ya existe, no hace nada.
   ============================================================================= */

USE TiendaUNNE;
GO

IF COL_LENGTH('dbo.Producto', 'stock_minimo') IS NULL
BEGIN
    ALTER TABLE dbo.Producto
        ADD stock_minimo DECIMAL(12,3) NOT NULL
            CONSTRAINT DF_Producto_stock_minimo DEFAULT (0);

    PRINT 'Columna stock_minimo agregada.';
END
ELSE
BEGIN
    PRINT 'La columna stock_minimo ya existía: no se hizo ningún cambio.';
END
GO

IF OBJECT_ID('dbo.CK_Producto_stock_minimo', 'C') IS NULL
BEGIN
    ALTER TABLE dbo.Producto
        ADD CONSTRAINT CK_Producto_stock_minimo CHECK (stock_minimo >= 0);

    PRINT 'Restricción CK_Producto_stock_minimo agregada.';
END
GO
