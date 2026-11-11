/* =============================================================================
   TiendaUNNE - 4. Usuarios por perfil
   Taller de Programación II - UNNE
   -----------------------------------------------------------------------------
   Ejecutar DESPUÉS de 1_CrearBaseYTablas.sql (necesita los perfiles).

   Crea un usuario de ejemplo por cada perfil, ya activos, sin bloqueo y SIN
   obligación de cambiar la contraseña, para poder entrar y probar cada rol:

     Perfil          DNI (usuario)   Contraseña
     Administrador   30000001        Admin.2026
     Supervisor      30000002        Supervisor.2026
     Cajero          30000003        Cajero.2026

   La contraseña NO se guarda: se guarda su hash PBKDF2 (SHA-256, 150.000
   iteraciones, salt de 16 bytes, hash de 32 bytes), el mismo que calcula la
   clase Negocio\PasswordHasher.cs de la aplicación. Los valores de abajo se
   generaron con esa misma clase y se comprobaron con su método Verificar.

   Como la tabla Usuario queda con datos, la aplicación NO crea su administrador
   automático (DNI 00000000): entrar con los usuarios de arriba.

   Se puede correr más de una vez: si el DNI ya existe, no se vuelve a cargar.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO
-------------------------------------------------------------------------------
-- Administrador: DNI 30000001   (contraseña: Admin.2026)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Persona WHERE dni_cuit = N'30000001')
    INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, email)
    VALUES (N'30000001', N'Administrador', N'Demo', N'administrador@tiendaunne.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u
               INNER JOIN dbo.Persona p ON p.id_persona = u.id_persona
               WHERE p.dni_cuit = N'30000001')
    INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt, debe_cambiar_pass, intentos_fallidos, bloqueado, activo)
    SELECT p.id_persona,
           (SELECT id_perfil FROM dbo.Perfil WHERE nombre = N'Administrador'),
           0x10751E680962A868D75D702B08EF0E1F2A1840282CF376F960BBD06DB8E708B5,
           0x08CF6FC3B13DDE77CC2D53AAFD6ED7CA,
           0, 0, 0, 1
    FROM   dbo.Persona p
    WHERE  p.dni_cuit = N'30000001';
GO

-------------------------------------------------------------------------------
-- Supervisor: DNI 30000002   (contraseña: Supervisor.2026)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Persona WHERE dni_cuit = N'30000002')
    INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, email)
    VALUES (N'30000002', N'Supervisor', N'Demo', N'supervisor@tiendaunne.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u
               INNER JOIN dbo.Persona p ON p.id_persona = u.id_persona
               WHERE p.dni_cuit = N'30000002')
    INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt, debe_cambiar_pass, intentos_fallidos, bloqueado, activo)
    SELECT p.id_persona,
           (SELECT id_perfil FROM dbo.Perfil WHERE nombre = N'Supervisor'),
           0x2EF19F346C78C1D4BAF23075C7F6B859F7B5B46D7848C22146D67107DBADDA8A,
           0x1884D65FE11D5AB6DC00EFDACC7F3FC2,
           0, 0, 0, 1
    FROM   dbo.Persona p
    WHERE  p.dni_cuit = N'30000002';
GO

-------------------------------------------------------------------------------
-- Cajero: DNI 30000003   (contraseña: Cajero.2026)
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Persona WHERE dni_cuit = N'30000003')
    INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, email)
    VALUES (N'30000003', N'Cajero', N'Demo', N'cajero@tiendaunne.com');

IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u
               INNER JOIN dbo.Persona p ON p.id_persona = u.id_persona
               WHERE p.dni_cuit = N'30000003')
    INSERT INTO dbo.Usuario (id_persona, id_perfil, hash_password, salt, debe_cambiar_pass, intentos_fallidos, bloqueado, activo)
    SELECT p.id_persona,
           (SELECT id_perfil FROM dbo.Perfil WHERE nombre = N'Cajero'),
           0x3EA8A004EEEBE44F062DFDC48DBF9851A2F4D60C5C73CA45FD669F1FF3EEFDFF,
           0x980FEF379C302773329DA854AFB1BFAC,
           0, 0, 0, 1
    FROM   dbo.Persona p
    WHERE  p.dni_cuit = N'30000003';
GO

-------------------------------------------------------------------------------
-- Comprobación: tienen que salir los 3 usuarios, activos y sin bloqueo
-------------------------------------------------------------------------------
SELECT p.dni_cuit AS DNI, pf.nombre AS Perfil, p.apellido + N', ' + p.nombre AS Persona,
       u.activo AS Activo, u.bloqueado AS Bloqueado, u.debe_cambiar_pass AS DebeCambiarPass
FROM        dbo.Usuario u
INNER JOIN  dbo.Persona p  ON p.id_persona = u.id_persona
INNER JOIN  dbo.Perfil  pf ON pf.id_perfil = u.id_perfil
WHERE p.dni_cuit IN (N'30000001', N'30000002', N'30000003')
ORDER BY p.dni_cuit;
GO

PRINT 'Usuarios por perfil cargados correctamente.';
GO