/* =============================================================================
   TiendaUNNE - Migración: datos fiscales del cliente
   -----------------------------------------------------------------------------
   Agrega a Cliente el CUIT (hace falta para la Factura A) y acota la condición
   de IVA a las opciones válidas. No borra ni modifica filas. Se puede correr las
   veces que haga falta: cada paso se fija primero si ya está aplicado.
   La aplicación la ejecuta sola al arrancar.
   ============================================================================= */

USE TiendaUNNE;
GO

IF COL_LENGTH('dbo.Cliente', 'cuit') IS NULL
    ALTER TABLE dbo.Cliente ADD cuit NVARCHAR(11) NULL;
GO

IF OBJECT_ID('dbo.CK_Cliente_cuit', 'C') IS NULL
    ALTER TABLE dbo.Cliente ADD CONSTRAINT CK_Cliente_cuit
        CHECK (cuit IS NULL OR (cuit NOT LIKE '%[^0-9]%' AND LEN(cuit) = 11));
GO

-- Un CUIT no se repite entre clientes (los NULL no cuentan).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Cliente_cuit' AND object_id = OBJECT_ID('dbo.Cliente'))
    CREATE UNIQUE INDEX UQ_Cliente_cuit ON dbo.Cliente (cuit) WHERE cuit IS NOT NULL;
GO

IF OBJECT_ID('dbo.CK_Cliente_condicion_iva', 'C') IS NULL
    ALTER TABLE dbo.Cliente WITH NOCHECK ADD CONSTRAINT CK_Cliente_condicion_iva
        CHECK (condicion_iva IN (N'Consumidor Final', N'Responsable Inscripto', N'Monotributista', N'Exento'));
GO