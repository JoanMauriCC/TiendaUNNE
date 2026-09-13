/* =============================================================================
   TiendaUNNE - Datos iniciales del módulo de Caja
   -----------------------------------------------------------------------------
   Carga la caja, el tipo de comprobante y los medios de pago que el módulo de
   Caja necesita para poder vender. Estas tres tablas se crean vacías en
   TiendaUNNE_CreateTables.sql, así que sin esto la pantalla de Caja no puede
   registrar ninguna venta.

   Se puede ejecutar sobre una base YA CREADA y con datos: no borra nada y
   se puede correr más de una vez sin duplicar (cada INSERT chequea antes).

   NO ejecutar TiendaUNNE_CreateTables.sql para esto: ese script hace DROP de
   todas las tablas y se perderían los usuarios, productos y ventas cargados.
   ============================================================================= */

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Caja  (el puesto físico donde se cobra)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Caja WHERE nombre = N'Caja 1')
    INSERT INTO dbo.Caja (nombre, descripcion)
    VALUES (N'Caja 1', N'Caja principal del local');
GO

-------------------------------------------------------------------------------
-- Tipo_comprobante
-- signo = 1 (suma), afecta_stock = 1 (descuenta), afecta_caja = 1 (ingresa plata)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Tipo_comprobante WHERE codigo = N'TKT')
    INSERT INTO dbo.Tipo_comprobante (codigo, nombre, letra, signo, afecta_stock, afecta_caja)
    VALUES (N'TKT', N'Ticket', NULL, 1, 1, 1);
GO

-------------------------------------------------------------------------------
-- Medio_pago
-- es_efectivo marca cuáles suman al efectivo en el cajón: solo eso se cuenta
-- en el arqueo al cerrar la caja.
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago WHERE nombre = N'Efectivo')
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, recargo_porcentaje, es_efectivo)
    VALUES (N'Efectivo', 0, 0, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago WHERE nombre = N'Tarjeta de débito')
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, recargo_porcentaje, es_efectivo)
    VALUES (N'Tarjeta de débito', 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago WHERE nombre = N'Tarjeta de crédito')
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, recargo_porcentaje, es_efectivo)
    VALUES (N'Tarjeta de crédito', 1, 0, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago WHERE nombre = N'Transferencia')
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, recargo_porcentaje, es_efectivo)
    VALUES (N'Transferencia', 1, 0, 0);
GO

PRINT 'Datos iniciales del módulo de Caja cargados correctamente.';
GO
