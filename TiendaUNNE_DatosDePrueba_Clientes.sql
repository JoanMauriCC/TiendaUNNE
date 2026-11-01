/* =============================================================================
   TiendaUNNE - Datos de prueba: 12 clientes
   -----------------------------------------------------------------------------
   Carga clientes de ejemplo para probar la Caja, los comprobantes y el reporte
   de clientes. Hay de todas las condiciones frente al IVA:
     - Consumidor Final  (sin CUIT)            -> Ticket o Factura B
     - Monotributista    (con CUIT)            -> Ticket o Factura B
     - Exento            (con CUIT)            -> Ticket o Factura B
     - Responsable Inscripto (con CUIT)        -> Ticket o Factura A
   Los CUIT tienen dígito de control válido, así que el programa los acepta.
   Los DNI van del 41000001 al 41000012.

   Se puede correr más de una vez: no repite a nadie (se fija primero por DNI).
   No borra ni modifica nada de lo que ya hay.
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-- 1) Las personas (el DNI es único: si ya existe, no se vuelve a cargar)
INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
SELECT v.dni, v.nombre, v.apellido, v.direccion, v.telefono, v.email, v.fecha_nacimiento
FROM (VALUES
    (N'41000001', N'Lucía',     N'Fernández', N'Junín 1250, Corrientes',          N'3794 11-2233', N'lucia.fernandez@mail.com',   CAST('1999-03-14' AS DATE)),
    (N'41000002', N'Matías',    N'Romero',    N'Salta 640, Corrientes',           N'3794 22-3344', N'matias.romero@mail.com',     CAST('2001-07-02' AS DATE)),
    (N'41000003', N'Valentina', N'Acosta',    N'Córdoba 980, Corrientes',         N'3794 33-4455', N'valentina.acosta@mail.com',  CAST('1995-11-23' AS DATE)),
    (N'41000004', N'Joaquín',   N'Benítez',   N'San Martín 2100, Corrientes',     N'3794 44-5566', N'joaquin.benitez@mail.com',   CAST('1992-05-30' AS DATE)),
    (N'41000005', N'Camila',    N'Domínguez', N'Pellegrini 455, Corrientes',      N'3794 55-6677', N'camila.dominguez@mail.com',  CAST('1988-09-08' AS DATE)),
    (N'41000006', N'Franco',    N'Gutiérrez', N'Av. Costanera 3300, Corrientes',  N'3794 66-7788', N'franco.gutierrez@mail.com',  CAST('1985-01-19' AS DATE)),
    (N'41000007', N'Agustina',  N'Sosa',      N'Mendoza 770, Resistencia',        N'3624 77-8899', N'agustina.sosa@mail.com',     CAST('2000-12-05' AS DATE)),
    (N'41000008', N'Nicolás',   N'Vera',      N'Av. Sarmiento 1500, Resistencia', N'3624 88-9900', N'nicolas.vera@mail.com',      CAST('1990-06-27' AS DATE)),
    (N'41000009', N'Rocío',     N'Medina',    N'Entre Ríos 320, Corrientes',      N'3794 99-0011', N'rocio.medina@mail.com',      CAST('2002-02-11' AS DATE)),
    (N'41000010', N'Tomás',     N'Ledesma',   N'Av. 3 de Abril 1800, Corrientes', N'3794 10-1122', N'tomas.ledesma@mail.com',     CAST('1983-08-16' AS DATE)),
    (N'41000011', N'Sofía',     N'Ibarra',    N'Quintana 910, Corrientes',        N'3794 12-2334', N'sofia.ibarra@mail.com',      CAST('1997-04-21' AS DATE)),
    (N'41000012', N'Bruno',     N'Cabrera',   N'Bolívar 560, Corrientes',         N'3794 13-3445', N'bruno.cabrera@mail.com',     CAST('2003-10-09' AS DATE))
) AS v(dni, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Persona p WHERE p.dni_cuit = v.dni);
GO

-- 2) Los clientes (una persona tiene a lo sumo un cliente: si ya lo es, no se repite)
INSERT INTO dbo.Cliente (id_persona, razon_social, cuit, condicion_iva)
SELECT p.id_persona, v.razon_social, v.cuit, v.condicion_iva
FROM (VALUES
    (N'41000001', NULL,                                       NULL,            N'Consumidor Final'),
    (N'41000002', NULL,                                       NULL,            N'Consumidor Final'),
    (N'41000003', NULL,                                       N'27410000038',  N'Monotributista'),
    (N'41000004', NULL,                                       N'20410000041',  N'Monotributista'),
    (N'41000005', N'Papelería Central de Domínguez',          N'27410000054',  N'Responsable Inscripto'),
    (N'41000006', N'Gutiérrez Distribuciones',                N'20410000068',  N'Responsable Inscripto'),
    (N'41000007', NULL,                                       NULL,            N'Consumidor Final'),
    (N'41000008', N'Vera Servicios Informáticos',             N'20410000084',  N'Exento'),
    (N'41000009', NULL,                                       NULL,            N'Consumidor Final'),
    (N'41000010', N'Copias y Librería Ledesma',               N'20410000106',  N'Responsable Inscripto'),
    (N'41000011', NULL,                                       N'27410000119',  N'Monotributista'),
    (N'41000012', NULL,                                       NULL,            N'Consumidor Final')
) AS v(dni, razon_social, cuit, condicion_iva)
INNER JOIN dbo.Persona p ON p.dni_cuit = v.dni
WHERE NOT EXISTS (SELECT 1 FROM dbo.Cliente c WHERE c.id_persona = p.id_persona);
GO

-- 3) Comprobación: tienen que salir los 12
SELECT p.dni_cuit AS DNI, p.apellido + N', ' + p.nombre AS Cliente, c.condicion_iva AS [Condición IVA],
       c.cuit AS CUIT, c.razon_social AS [Razón social]
FROM dbo.Cliente c
INNER JOIN dbo.Persona p ON p.id_persona = c.id_persona
WHERE p.dni_cuit BETWEEN N'41000001' AND N'41000012'
ORDER BY p.dni_cuit;
GO
