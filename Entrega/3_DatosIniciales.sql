/* =============================================================================
   TiendaUNNE - 3. Datos iniciales
   Taller de Programación II - UNNE
   -----------------------------------------------------------------------------
   Ejecutar DESPUÉS de 1_CrearBaseYTablas.sql (y del 2).

   Carga lo mínimo para que el sistema pueda vender:
     - La caja (Caja 1).
     - Los medios de pago: Efectivo, Tarjeta de débito, Tarjeta de crédito y
       Transferencia.
     - Los tipos de comprobante: Ticket, Factura B, Factura A, Nota de crédito A
       y Nota de crédito B (las notas llevan signo -1: restan en reportes y caja).

   Es lo mismo que la aplicación carga sola al arrancar (si falta). Cada fila se
   inserta solo si todavía no existe, así que se puede correr más de una vez, o
   abrir la aplicación después, sin duplicar nada.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Caja
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Caja)
    INSERT INTO dbo.Caja (nombre, descripcion)
    VALUES (N'Caja 1', N'Caja principal del local');
GO

-------------------------------------------------------------------------------
-- Medios de pago
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Medio_pago)
    INSERT INTO dbo.Medio_pago (nombre, requiere_referencia, es_efectivo) VALUES
        (N'Efectivo',           0, 1),
        (N'Tarjeta de débito',  1, 0),
        (N'Tarjeta de crédito', 1, 0),
        (N'Transferencia',      1, 0);
GO

-------------------------------------------------------------------------------
-- Tipos de comprobante (se compara por código)
-------------------------------------------------------------------------------
INSERT INTO dbo.Tipo_comprobante (codigo, nombre, letra, signo)
SELECT v.codigo, v.nombre, v.letra, v.signo
FROM (VALUES
    (N'TKT', N'Ticket',            CAST(NULL AS CHAR(1)),  CAST( 1 AS SMALLINT)),
    (N'FB',  N'Factura B',         CAST('B'  AS CHAR(1)),  CAST( 1 AS SMALLINT)),
    (N'FA',  N'Factura A',         CAST('A'  AS CHAR(1)),  CAST( 1 AS SMALLINT)),
    (N'NCA', N'Nota de crédito A', CAST('A'  AS CHAR(1)),  CAST(-1 AS SMALLINT)),
    (N'NCB', N'Nota de crédito B', CAST('B'  AS CHAR(1)),  CAST(-1 AS SMALLINT))
) AS v(codigo, nombre, letra, signo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tipo_comprobante t WHERE t.codigo = v.codigo);
GO

-------------------------------------------------------------------------------
-- Comprobación: tienen que salir 1 caja, 4 medios de pago y 5 comprobantes
-------------------------------------------------------------------------------
SELECT (SELECT COUNT(*) FROM dbo.Caja)             AS Cajas,
       (SELECT COUNT(*) FROM dbo.Medio_pago)       AS MediosDePago,
       (SELECT COUNT(*) FROM dbo.Tipo_comprobante) AS TiposDeComprobante;
GO

PRINT 'Datos iniciales cargados correctamente.';
GO
