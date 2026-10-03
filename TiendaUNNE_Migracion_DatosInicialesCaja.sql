/* =============================================================================
   TiendaUNNE - Migración: datos iniciales de la caja
   -----------------------------------------------------------------------------
   Carga lo mínimo que necesita la caja para operar: la caja, los medios de pago
   y el tipo de comprobante "Ticket". Es lo mismo que ahora inserta
   TiendaUNNE_CreateTables.sql en una base nueva; este script es para una base
   que YA EXISTE.

   No borra ni modifica nada, y se puede correr más de una vez: cada fila se
   inserta solo si todavía no está (se compara por nombre o código).
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

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

PRINT 'Datos iniciales de la caja verificados correctamente.';
GO
