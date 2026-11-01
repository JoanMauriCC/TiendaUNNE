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
