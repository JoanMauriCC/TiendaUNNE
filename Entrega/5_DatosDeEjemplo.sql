/* =============================================================================
   TiendaUNNE - 5. Datos de ejemplo (OPCIONAL)
   Taller de Programación II - UNNE
   -----------------------------------------------------------------------------
   Ejecutar DESPUÉS de los scripts 1 a 4. No hace falta para que el sistema
   funcione: sirve para poder probar la Caja apenas se abre el programa.

   Carga:
     - 4 categorías.
     - 13 productos de una tienda universitaria, con su precio final (IVA incluido),
       stock y stock mínimo. Uno tiene el stock en el mínimo, para ver el aviso de
       "stock bajo".
     - 6 clientes de ejemplo, con distintas condiciones frente al IVA:
         Consumidor Final  -> Ticket o Factura B
         Monotributista    -> Ticket o Factura B
         Exento            -> Ticket o Factura B
         Responsable Inscripto (con CUIT) -> Ticket o Factura A
       Los CUIT tienen dígito de control válido, así que la aplicación los acepta.

   No carga ventas. Se puede correr más de una vez: cada fila se inserta solo si
   todavía no existe (se compara por nombre o por DNI). Los datos entran directo
   a la base, no por la aplicación, así que no dejan registros en Auditoría.
   ============================================================================= */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Categorías
-------------------------------------------------------------------------------
INSERT INTO dbo.Categoria (nombre, descripcion)
SELECT v.nombre, v.descripcion
FROM (VALUES
    (N'Indumentaria',    N'Remeras, buzos, camperas y gorras con la marca UNNE'),
    (N'Accesorios',      N'Mochilas y complementos'),
    (N'Librería',        N'Útiles y artículos de estudio'),
    (N'Termos y mates',  N'Todo para el mate')
) AS v(nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categoria c WHERE c.nombre = v.nombre);
GO

-------------------------------------------------------------------------------
-- Productos
-- Columnas: categoría, nombre, descripción, precio final (IVA incluido), stock, stock mínimo
-------------------------------------------------------------------------------
INSERT INTO dbo.Producto (id_categoria, nombre, descripcion, precio_venta, stock, stock_minimo)
SELECT c.id_categoria, v.nombre, v.descripcion, v.precio, v.stock, v.minimo
FROM (VALUES
    -- Indumentaria
    (N'Indumentaria',   N'Remera estampada UNNE',               N'Algodón, estampa frontal. Varios talles.',          15900.00, 40.000,  8.000),
    (N'Indumentaria',   N'Buzo canguro con capucha UNNE',       N'Friza con bolsillo canguro.',                       28900.00, 25.000,  5.000),
    (N'Indumentaria',   N'Campera rompeviento UNNE',            N'Liviana e impermeable. Stock en el mínimo.',       45900.00,  4.000,  4.000),
    (N'Indumentaria',   N'Gorra UNNE',                          N'Visera curva, regulable.',                           7200.00, 60.000, 10.000),

    -- Accesorios
    (N'Accesorios',     N'Mochila urbana UNNE',                 N'Con compartimento para notebook.',                  38500.00, 12.000,  3.000),
    (N'Accesorios',     N'Medias deportivas (par)',             N'Algodón, talle único.',                              2900.00, 90.000, 20.000),

    -- Librería
    (N'Librería',       N'Cartuchera de tela UNNE',             N'Doble cierre.',                                      5400.00, 50.000, 10.000),
    (N'Librería',       N'Cuaderno universitario UNNE',         N'80 hojas rayadas, tapa dura.',                       3200.00, 120.000, 25.000),
    (N'Librería',       N'Calculadora científica',              N'240 funciones.',                                    18900.00, 15.000,  3.000),
    (N'Librería',       N'Biromes azules (pack x10)',           N'Trazo medio.',                                       2500.00, 80.000, 15.000),

    -- Termos y mates
    (N'Termos y mates', N'Termo de acero inoxidable 1L',        N'Mantiene el frío y el calor.',                      24500.00, 30.000,  6.000),
    (N'Termos y mates', N'Mate de calabaza forrado en cuero',   N'Con base de goma.',                                 14800.00, 20.000,  4.000),
    (N'Termos y mates', N'Bombilla de acero inoxidable',        N'Pico plano, desarmable.',                            3200.00, 45.000, 10.000)
) AS v(categoria, nombre, descripcion, precio, stock, minimo)
INNER JOIN dbo.Categoria c ON c.nombre = v.categoria
WHERE NOT EXISTS (SELECT 1 FROM dbo.Producto p WHERE p.nombre = v.nombre);
GO

-------------------------------------------------------------------------------
-- Clientes de ejemplo (el DNI es único: si ya existe, no se vuelve a cargar)
-------------------------------------------------------------------------------
-- 1) Las personas
INSERT INTO dbo.Persona (dni_cuit, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
SELECT v.dni, v.nombre, v.apellido, v.direccion, v.telefono, v.email, v.fecha_nacimiento
FROM (VALUES
    (N'41000001', N'Lucía',     N'Fernández', N'Junín 1250, Corrientes',          N'3794 11-2233', N'lucia.fernandez@mail.com',   CAST('1999-03-14' AS DATE)),
    (N'41000003', N'Valentina', N'Acosta',    N'Córdoba 980, Corrientes',         N'3794 33-4455', N'valentina.acosta@mail.com',  CAST('1995-11-23' AS DATE)),
    (N'41000005', N'Camila',    N'Domínguez', N'Pellegrini 455, Corrientes',      N'3794 55-6677', N'camila.dominguez@mail.com',  CAST('1988-09-08' AS DATE)),
    (N'41000006', N'Franco',    N'Gutiérrez', N'Av. Costanera 3300, Corrientes',  N'3794 66-7788', N'franco.gutierrez@mail.com',  CAST('1985-01-19' AS DATE)),
    (N'41000008', N'Nicolás',   N'Vera',      N'Av. Sarmiento 1500, Resistencia', N'3624 88-9900', N'nicolas.vera@mail.com',      CAST('1990-06-27' AS DATE)),
    (N'41000010', N'Tomás',     N'Ledesma',   N'Av. 3 de Abril 1800, Corrientes', N'3794 10-1122', N'tomas.ledesma@mail.com',     CAST('1983-08-16' AS DATE))
) AS v(dni, nombre, apellido, direccion, telefono, email, fecha_nacimiento)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Persona p WHERE p.dni_cuit = v.dni);
GO

-- 2) Los clientes (una persona tiene a lo sumo un cliente: si ya lo es, no se repite)
INSERT INTO dbo.Cliente (id_persona, razon_social, cuit, condicion_iva)
SELECT p.id_persona, v.razon_social, v.cuit, v.condicion_iva
FROM (VALUES
    (N'41000001', NULL,                              NULL,            N'Consumidor Final'),
    (N'41000003', NULL,                              N'27410000038',  N'Monotributista'),
    (N'41000005', N'Papelería Central de Domínguez', N'27410000054',  N'Responsable Inscripto'),
    (N'41000006', N'Gutiérrez Distribuciones',       N'20410000068',  N'Responsable Inscripto'),
    (N'41000008', N'Vera Servicios Informáticos',    N'20410000084',  N'Exento'),
    (N'41000010', N'Copias y Librería Ledesma',      N'20410000106',  N'Responsable Inscripto')
) AS v(dni, razon_social, cuit, condicion_iva)
INNER JOIN dbo.Persona p ON p.dni_cuit = v.dni
WHERE NOT EXISTS (SELECT 1 FROM dbo.Cliente c WHERE c.id_persona = p.id_persona);
GO

-------------------------------------------------------------------------------
-- Comprobación: tienen que salir 4 categorías, 13 productos y 6 clientes
-------------------------------------------------------------------------------
SELECT (SELECT COUNT(*) FROM dbo.Categoria) AS Categorias,
       (SELECT COUNT(*) FROM dbo.Producto)  AS Productos,
       (SELECT COUNT(*) FROM dbo.Cliente)   AS Clientes;
GO

PRINT 'Datos de ejemplo cargados correctamente.';
GO
