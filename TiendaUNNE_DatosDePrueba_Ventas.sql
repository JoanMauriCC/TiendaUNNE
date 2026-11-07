/* =============================================================================
   TiendaUNNE - Datos de prueba: 120 ventas repartidas en los últimos 60 días
   -----------------------------------------------------------------------------
   Sirve para ver los reportes (Ventas, Productos, Recaudación y Clientes) y sus
   gráficos con datos, sin tener que cobrar a mano.

   Qué carga:
     - 120 ventas emitidas entre hoy y hace 60 días (algunas son de hoy).
     - Cada venta tiene entre 1 y 3 productos, un medio de pago y su ingreso de caja.
     - Casi 7 de cada 10 son a un cliente de prueba (los clientes de
       TiendaUNNE_DatosDePrueba_Clientes.sql); el resto, a consumidor final.
     - Comprobantes: Ticket, Factura B y Factura A (la A solo a Responsables Inscriptos),
       con el IVA discriminado y numeración correlativa, como lo hace la Caja.
     - Un turno de caja CERRADO por cada día con ventas, con su arqueo en cero de diferencia.

   Antes de correrlo tiene que haber: la aplicación ejecutada una vez (carga la caja,
   los medios de pago y los comprobantes), productos y los 12 clientes de prueba.

   Qué NO hace: no toca el stock de los productos ni la caja que tengas abierta.
   Todo lo que carga queda marcado con «DATOS DE PRUEBA»; para sacarlo, correr
   TiendaUNNE_DatosDePrueba_Ventas_Borrar.sql.

   Si ya hay ventas de prueba cargadas, no vuelve a cargar (no duplica).
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @marca NVARCHAR(30) = N'DATOS DE PRUEBA';

IF EXISTS (SELECT 1 FROM dbo.Venta_cabecera WHERE observaciones = @marca)
BEGIN
    PRINT 'Ya hay ventas de prueba cargadas. Para volver a empezar, corré primero TiendaUNNE_DatosDePrueba_Ventas_Borrar.sql.';
    RETURN;
END

-------------------------------------------------------------------------------
-- Lo que tiene que existir antes
-------------------------------------------------------------------------------
DECLARE @id_caja    INT = (SELECT TOP 1 id_caja FROM dbo.Caja WHERE activo = 1 ORDER BY id_caja);
DECLARE @id_usuario INT = (SELECT TOP 1 id_usuario FROM dbo.Usuario WHERE activo = 1 ORDER BY id_usuario);

IF @id_caja IS NULL OR @id_usuario IS NULL
    OR (SELECT COUNT(*) FROM dbo.Tipo_comprobante WHERE codigo IN (N'TKT', N'FB', N'FA')) < 3
    OR NOT EXISTS (SELECT 1 FROM dbo.Medio_pago WHERE activo = 1)
BEGIN
    PRINT 'Falta la caja, los medios de pago o los tipos de comprobante. Abrí la aplicación una vez (los carga sola) y volvé a correr este script.';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.Producto WHERE activo = 1 AND precio_venta > 0)
BEGIN
    PRINT 'No hay productos con precio. Cargalos primero (por ejemplo, con TiendaUNNE_DatosDePrueba.sql).';
    RETURN;
END

IF (SELECT COUNT(*)
    FROM dbo.Cliente c INNER JOIN dbo.Persona p ON p.id_persona = c.id_persona
    WHERE p.dni_cuit BETWEEN N'41000001' AND N'41000012') < 12
BEGIN
    PRINT 'Faltan los clientes de prueba. Corré primero TiendaUNNE_DatosDePrueba_Clientes.sql.';
    RETURN;
END

BEGIN TRY
BEGIN TRANSACTION;

-------------------------------------------------------------------------------
-- Clientes de prueba, ordenados de "el que más compra" al que menos: así el
-- ranking del reporte tiene diferencias y no sale todo parejo.
-------------------------------------------------------------------------------
SELECT  o.orden, c.id_cliente, c.condicion_iva
INTO    #Cli
FROM        dbo.Cliente c
INNER JOIN  dbo.Persona p ON p.id_persona = c.id_persona
INNER JOIN  (VALUES (N'41000005', 1), (N'41000010', 2), (N'41000006', 3), (N'41000003', 4),
                    (N'41000001', 5), (N'41000008', 6), (N'41000004', 7), (N'41000011', 8),
                    (N'41000002', 9), (N'41000007', 10), (N'41000009', 11), (N'41000012', 12)
            ) o(dni, orden) ON o.dni = p.dni_cuit;

-------------------------------------------------------------------------------
-- Las ventas: cuándo, a quién y con qué comprobante
-------------------------------------------------------------------------------
DECLARE @ahora       DATETIME2(0) = SYSDATETIME();
DECLARE @hoy         DATETIME2(0) = CAST(CAST(@ahora AS DATE) AS DATETIME2(0));
DECLARE @seg_hoy     INT = DATEDIFF(SECOND, @hoy, @ahora);
DECLARE @cantidad    INT = 120;

;WITH n AS (
    SELECT TOP (@cantidad) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS id
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
SELECT  id,
        ABS(CHECKSUM(NEWID())) % 61 AS dias_atras,          -- 0 (hoy) a 60 días atrás
        CAST(NULL AS DATETIME2(0))  AS fecha_hora,
        CAST(NULL AS INT)           AS id_cliente,
        CAST(NULL AS NVARCHAR(30))  AS condicion,
        CAST(NULL AS NVARCHAR(5))   AS codigo,
        CAST(NULL AS BIGINT)        AS numero,
        CAST(NULL AS INT)           AS id_sesion,
        CAST(NULL AS INT)           AS id_venta,
        CAST(0 AS DECIMAL(12,2))    AS total,
        CAST(0 AS DECIMAL(12,2))    AS neto,
        CAST(0 AS DECIMAL(12,2))    AS iva
INTO    #V
FROM    n;

-- Si todavía es muy temprano, las ventas "de hoy" pasan a ayer: no se inventan ventas del futuro.
IF @seg_hoy < 900
    UPDATE #V SET dias_atras = 1 WHERE dias_atras = 0;

-- Hora: de 9 a 20 h los días anteriores; hoy, entre las 00:01 y unos minutos antes de ahora.
UPDATE #V
SET fecha_hora = CASE
        WHEN dias_atras = 0
            THEN DATEADD(SECOND, 60 + ABS(CHECKSUM(NEWID())) % (@seg_hoy - 300), @hoy)
        ELSE DATEADD(SECOND, 9 * 3600 + ABS(CHECKSUM(NEWID())) % (11 * 3600),
                             DATEADD(DAY, -dias_atras, @hoy))
    END;

-- Cliente: 3 de cada 10 son a consumidor final; el resto, con preferencia por los primeros de la lista.
-- El sorteo se guarda primero en #Sorteo: con NEWID() dentro de una subconsulta, el motor la
-- reevalúa en cada fila del JOIN y los resultados no coinciden.
SELECT id,
       1 + CAST(12.0 * POWER((ABS(CHECKSUM(NEWID())) % 10000) / 10000.0, 1.3) AS INT) AS orden,
       ABS(CHECKSUM(NEWID())) % 100 AS dado
INTO   #Sorteo
FROM   #V;

UPDATE v
SET    id_cliente = c.id_cliente,
       condicion  = c.condicion_iva
FROM   #V v
INNER JOIN #Sorteo x ON x.id = v.id AND x.dado >= 30
INNER JOIN #Cli c ON c.orden = CASE WHEN x.orden > 12 THEN 12 ELSE x.orden END;

DROP TABLE #Sorteo;

-- Comprobante según el cliente: Responsable Inscripto -> Factura A o Ticket; el resto -> Factura B o Ticket.
UPDATE #V
SET codigo = CASE
        WHEN condicion = N'Responsable Inscripto'
            THEN CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 65 THEN N'FA' ELSE N'TKT' END
        ELSE CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 45 THEN N'FB' ELSE N'TKT' END
    END;

-------------------------------------------------------------------------------
-- Los renglones: 1 a 3 productos por venta, 1 a 3 unidades de cada uno
-------------------------------------------------------------------------------
SELECT  v.id AS id_v, l.n AS nro_linea, p.id_producto, p.nombre, p.precio_venta AS precio, q.cant,
        CAST(0 AS DECIMAL(12,2)) AS subtotal,
        CAST(0 AS DECIMAL(5,2))  AS alicuota,
        CAST(0 AS DECIMAL(12,2)) AS importe_iva
INTO    #D
FROM    #V v
CROSS JOIN (VALUES (1), (2), (3)) l(n)
CROSS APPLY (SELECT TOP 1 id_producto, nombre, precio_venta
             FROM   dbo.Producto
             WHERE  activo = 1 AND precio_venta > 0
             ORDER BY CHECKSUM(NEWID(), v.id, l.n)) p
CROSS APPLY (SELECT 1 + ABS(CHECKSUM(NEWID(), v.id, l.n)) % 3 AS cant) q
WHERE   l.n = 1
   OR   ABS(CHECKSUM(NEWID(), v.id, l.n)) % 100 < CASE l.n WHEN 2 THEN 55 ELSE 25 END;

UPDATE d SET subtotal = ROUND(d.precio * d.cant, 2) FROM #D d;

-- IVA del 21 % en las facturas (los precios ya lo incluyen): neto redondeado e IVA por diferencia,
-- igual que hace la aplicación al cobrar. El ticket no discrimina IVA.
UPDATE d
SET    alicuota    = 21,
       importe_iva = d.subtotal - ROUND(d.subtotal / 1.21, 2)
FROM   #D d
INNER JOIN #V v ON v.id = d.id_v
WHERE  v.codigo IN (N'FA', N'FB');

UPDATE v
SET    total = t.total,
       iva   = t.iva,
       neto  = t.total - t.iva
FROM   #V v
INNER JOIN (SELECT id_v, SUM(subtotal) AS total, SUM(importe_iva) AS iva FROM #D GROUP BY id_v) t ON t.id_v = v.id;

-------------------------------------------------------------------------------
-- Numeración correlativa por tipo de comprobante (a partir del último que exista)
-------------------------------------------------------------------------------
UPDATE v
SET    numero = b.base + x.rn
FROM   #V v
INNER JOIN (SELECT id, ROW_NUMBER() OVER (PARTITION BY codigo ORDER BY fecha_hora, id) AS rn FROM #V) x ON x.id = v.id
INNER JOIN (SELECT t.codigo, ISNULL(MAX(vc.numero_comprobante), 0) AS base
            FROM   dbo.Tipo_comprobante t
            LEFT JOIN dbo.Venta_cabecera vc
                   ON vc.id_tipo_comprobante = t.id_tipo_comprobante AND vc.punto_venta = @id_caja
            GROUP BY t.codigo) b ON b.codigo = v.codigo;

-------------------------------------------------------------------------------
-- Un turno de caja cerrado por cada día con ventas
-------------------------------------------------------------------------------
INSERT INTO dbo.Caja_sesion
    (id_caja, id_usuario_apertura, id_usuario_cierre, fecha_apertura, fecha_cierre,
     monto_inicial, estado, observaciones)
SELECT  @id_caja, @id_usuario, @id_usuario,
        DATEADD(SECOND, -60, MIN(fecha_hora)),
        DATEADD(SECOND,  60, MAX(fecha_hora)),
        5000, N'CERRADA', @marca
FROM    #V
GROUP BY dias_atras;

UPDATE v
SET    id_sesion = s.id_caja_sesion
FROM   #V v
INNER JOIN dbo.Caja_sesion s
        ON s.observaciones = @marca AND CAST(s.fecha_apertura AS DATE) = CAST(v.fecha_hora AS DATE);

-------------------------------------------------------------------------------
-- Cabecera, renglones, pagos y movimientos de caja
-------------------------------------------------------------------------------
INSERT INTO dbo.Venta_cabecera
    (id_tipo_comprobante, punto_venta, numero_comprobante, id_cliente, id_usuario, id_caja_sesion,
     fecha_hora, subtotal, descuento, importe_neto, importe_iva, total, estado, observaciones)
SELECT  t.id_tipo_comprobante, @id_caja, v.numero, v.id_cliente, @id_usuario, v.id_sesion,
        v.fecha_hora, v.total, 0, v.neto, v.iva, v.total, N'EMITIDA', @marca
FROM    #V v
INNER JOIN dbo.Tipo_comprobante t ON t.codigo = v.codigo;

UPDATE v
SET    id_venta = vc.id_venta
FROM   #V v
INNER JOIN dbo.Tipo_comprobante t ON t.codigo = v.codigo
INNER JOIN dbo.Venta_cabecera vc
        ON vc.id_tipo_comprobante = t.id_tipo_comprobante
       AND vc.punto_venta = @id_caja AND vc.numero_comprobante = v.numero;

INSERT INTO dbo.Venta_detalle
    (id_venta, id_producto, nro_linea, descripcion, cantidad, precio_unitario,
     alicuota_iva, importe_iva, subtotal)
SELECT  v.id_venta, d.id_producto, d.nro_linea, d.nombre, d.cant, d.precio,
        d.alicuota, d.importe_iva, d.subtotal
FROM    #D d
INNER JOIN #V v ON v.id = d.id_v;

-- Un solo medio de pago por venta, elegido al azar entre los activos.
SELECT  v.id AS id_v, mp.id_medio_pago, mp.es_efectivo
INTO    #P
FROM    #V v
CROSS APPLY (SELECT TOP 1 id_medio_pago, es_efectivo
             FROM   dbo.Medio_pago
             WHERE  activo = 1
             ORDER BY CHECKSUM(NEWID(), v.id)) mp;

INSERT INTO dbo.Venta_pago (id_venta, id_medio_pago, importe, referencia, fecha_hora)
SELECT  v.id_venta, p.id_medio_pago, v.total,
        CASE WHEN p.es_efectivo = 1 THEN NULL ELSE N'PRUEBA-' + CAST(v.numero AS NVARCHAR(20)) END,
        v.fecha_hora
FROM    #V v
INNER JOIN #P p ON p.id_v = v.id;

INSERT INTO dbo.Movimiento_caja
    (id_caja_sesion, id_usuario, id_medio_pago, id_venta, fecha_hora, tipo, concepto, monto)
SELECT  v.id_sesion, @id_usuario, p.id_medio_pago, v.id_venta, v.fecha_hora,
        N'INGRESO', N'Cobro de venta', v.total
FROM    #V v
INNER JOIN #P p ON p.id_v = v.id;

-- Cierre de cada turno: el efectivo esperado es lo que se declaró, sin diferencia.
UPDATE s
SET    monto_final_sistema   = s.monto_inicial + ISNULL(e.efectivo, 0),
       monto_final_declarado = s.monto_inicial + ISNULL(e.efectivo, 0),
       diferencia            = 0
FROM   dbo.Caja_sesion s
LEFT JOIN (SELECT vc.id_caja_sesion, SUM(vp.importe) AS efectivo
           FROM   dbo.Venta_pago vp
           INNER JOIN dbo.Venta_cabecera vc ON vc.id_venta = vp.id_venta
           INNER JOIN dbo.Medio_pago     mp ON mp.id_medio_pago = vp.id_medio_pago
           WHERE  mp.es_efectivo = 1 AND vc.estado = N'EMITIDA'
           GROUP BY vc.id_caja_sesion) e ON e.id_caja_sesion = s.id_caja_sesion
WHERE  s.observaciones = @marca;

COMMIT TRANSACTION;

PRINT 'Listo: ventas de prueba cargadas.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'No se pudo cargar (no quedó nada a medias): ' + ERROR_MESSAGE();
END CATCH;
GO

-- Resumen de lo cargado
SELECT  t.nombre AS Comprobante, COUNT(*) AS Ventas, SUM(vc.total) AS Total,
        MIN(vc.fecha_hora) AS [Primera], MAX(vc.fecha_hora) AS [Última]
FROM    dbo.Venta_cabecera vc
INNER JOIN dbo.Tipo_comprobante t ON t.id_tipo_comprobante = vc.id_tipo_comprobante
WHERE   vc.observaciones = N'DATOS DE PRUEBA'
GROUP BY t.nombre
ORDER BY t.nombre;
GO
