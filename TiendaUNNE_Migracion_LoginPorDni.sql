/* =============================================================================
   TiendaUNNE - Migración: el login pasa a ser por DNI/CUIT
   -----------------------------------------------------------------------------
   Quita de dbo.Usuario la columna nombre_usuario y su restricción UNIQUE, en una
   base YA CREADA. Es el equivalente al cambio hecho en TiendaUNNE_CreateTables.sql,
   donde Usuario ya no tiene nombre propio: se ingresa con el dni_cuit de Persona.

   NO ejecutar TiendaUNNE_CreateTables.sql para obtener este cambio: ese script hace
   DROP de todas las tablas y se perderían usuarios, productos y ventas.

   Cuidado: los nombres de usuario que ya estuvieran cargados se pierden. Los
   usuarios siguen existiendo y pasan a ingresar con su DNI/CUIT.

   Se puede correr más de una vez: si la columna ya no existe, no hace nada.
   ============================================================================= */

USE TiendaUNNE;
GO

IF EXISTS (SELECT 1 FROM sys.objects
           WHERE name = 'UQ_Usuario_nombre'
             AND parent_object_id = OBJECT_ID('dbo.Usuario'))
BEGIN
    ALTER TABLE dbo.Usuario DROP CONSTRAINT UQ_Usuario_nombre;
    PRINT 'Restricción UQ_Usuario_nombre eliminada.';
END
GO

IF COL_LENGTH('dbo.Usuario', 'nombre_usuario') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Usuario DROP COLUMN nombre_usuario;
    PRINT 'Columna nombre_usuario eliminada.';
END
ELSE
BEGIN
    PRINT 'La columna nombre_usuario ya no existía: no se hizo ningún cambio.';
END
GO
