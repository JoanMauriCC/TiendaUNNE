/* =============================================================================
   TiendaUNNE - Borra SOLO las ventas de prueba
   -----------------------------------------------------------------------------
   Elimina lo que cargó TiendaUNNE_DatosDePrueba_Ventas.sql: las ventas marcadas
   «DATOS DE PRUEBA» (con sus renglones y pagos), sus ingresos de caja y los turnos
   de caja de prueba. No toca ventas reales, el stock, los clientes ni la caja
   que tengas abierta.
   ============================================================================= */

USE TiendaUNNE;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Si alguien emitió notas de crédito sobre ventas de prueba, caen primero (apuntan a la venta
    -- original) y se les quita lo que devolvieron al stock: la venta de prueba nunca lo había descontado.
    SELECT id_venta INTO #NC FROM dbo.Venta_cabecera
    WHERE  id_venta_anulada IN (SELECT id_venta FROM dbo.Venta_cabecera WHERE observaciones = N'DATOS DE PRUEBA');

    UPDATE p SET p.stock = p.stock - x.cant
    FROM   dbo.Producto p
    INNER JOIN (SELECT d.id_producto, SUM(d.cantidad) AS cant
                FROM   dbo.Venta_detalle d INNER JOIN #NC n ON n.id_venta = d.id_venta
                GROUP BY d.id_producto) x ON x.id_producto = p.id_producto;

    DELETE FROM dbo.Movimiento_caja WHERE id_venta IN (SELECT id_venta FROM #NC);
    DELETE FROM dbo.Venta_cabecera  WHERE id_venta IN (SELECT id_venta FROM #NC);
    DROP TABLE #NC;
    DELETE FROM dbo.Movimiento_caja
    WHERE  id_venta IN (SELECT id_venta FROM dbo.Venta_cabecera WHERE observaciones = N'DATOS DE PRUEBA');

    -- Los renglones y los pagos se borran solos junto con su venta (ON DELETE CASCADE).
    DELETE FROM dbo.Venta_cabecera WHERE observaciones = N'DATOS DE PRUEBA';

    DELETE s
    FROM   dbo.Caja_sesion s
    WHERE  s.observaciones = N'DATOS DE PRUEBA'
      AND  NOT EXISTS (SELECT 1 FROM dbo.Venta_cabecera vc WHERE vc.id_caja_sesion = s.id_caja_sesion)
      AND  NOT EXISTS (SELECT 1 FROM dbo.Movimiento_caja m WHERE m.id_caja_sesion = s.id_caja_sesion);

    COMMIT TRANSACTION;
    PRINT 'Listo: las ventas de prueba fueron borradas.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'No se pudo borrar (no se tocó nada): ' + ERROR_MESSAGE();
END CATCH;
GO
