/* =============================================================================
   TiendaUNNE - Migración: el DNI debe tener exactamente 8 dígitos
   -----------------------------------------------------------------------------
   Antes se aceptaban 7 u 8 dígitos. Ahora la regla es: siempre 8, ni más ni
   menos. Este script agrega esa restricción a una base que YA EXISTE, sin
   borrar nada.

   Si hay algún dni_cuit que no tenga 8 dígitos, el script lo avisa y no
   agrega la restricción (agregarla rompería esas filas). Hay que corregir
   esos datos a mano y volver a correr el script.
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Aviso si hay datos que no cumplen la regla nueva
-------------------------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM dbo.Persona
    WHERE dni_cuit LIKE '%[^0-9]%' OR LEN(dni_cuit) <> 8
)
BEGIN
    PRINT 'ATENCIÓN: hay registros en Persona con un dni_cuit que no tiene exactamente 8 dígitos.';
    PRINT 'Corregilos a mano y volvé a correr este script; la restricción NO se agregó.';

    SELECT id_persona, dni_cuit
    FROM   dbo.Persona
    WHERE  dni_cuit LIKE '%[^0-9]%' OR LEN(dni_cuit) <> 8;
END
ELSE
BEGIN
    IF OBJECT_ID('dbo.CK_Persona_dni_cuit', 'C') IS NULL
    BEGIN
        ALTER TABLE dbo.Persona
        ADD CONSTRAINT CK_Persona_dni_cuit CHECK (dni_cuit NOT LIKE '%[^0-9]%' AND LEN(dni_cuit) = 8);

        PRINT 'Restricción CK_Persona_dni_cuit agregada correctamente.';
    END
    ELSE
    BEGIN
        PRINT 'La restricción CK_Persona_dni_cuit ya existía, no se tocó nada.';
    END
END
GO
