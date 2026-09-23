/* =============================================================================
   TiendaUNNE - Datos de PRUEBA: categorías y productos
   -----------------------------------------------------------------------------
   Carga categorías y productos de ejemplo para poder probar la pantalla de Caja
   (búsqueda de productos, cantidades, stock, cobro) sin cargarlos a mano uno por
   uno. NO es parte del sistema: es solo para desarrollo y pruebas.

   Casos que cubre, pensados para Caja:
     - productos con stock de sobra                         (venta normal)
     - productos con stock justo en el mínimo o por debajo  (aviso de "stock bajo")
     - un producto SIN stock                                (la caja tiene que frenarlo)
     - productos con stock decimal, vendidos por kilo       (cantidades con coma)
     - un producto y una categoría dados de baja            (no deben aparecer en Caja)

   Se puede ejecutar sobre una base con datos: no borra nada y se puede correr más
   de una vez sin duplicar (cada fila se inserta solo si su nombre no existe).
   Los datos entran directo a la base, no por la aplicación, así que no dejan
   registros en Auditoría.

   Al final hay un bloque comentado para borrar solo estos datos de prueba.
   ============================================================================= */

SET QUOTED_IDENTIFIER ON;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- Categorías
-------------------------------------------------------------------------------
DECLARE @categorias TABLE (nombre NVARCHAR(100), descripcion NVARCHAR(200), activo BIT);

INSERT INTO @categorias (nombre, descripcion, activo) VALUES
    (N'Indumentaria',   N'Prendas de vestir',                 1),
    (N'Accesorios',     N'Gorras, bolsos y más',              1),
    (N'Calzado',        N'Zapatillas y botas',                1),
    (N'Bebidas',        N'Gaseosas, aguas y jugos',           1),
    (N'Almacén',        N'Productos de almacén, algunos por kilo', 1),
    (N'Discontinuados', N'Categoría dada de baja (prueba)',   0);

INSERT INTO dbo.Categoria (nombre, descripcion, activo)
SELECT c.nombre, c.descripcion, c.activo
FROM   @categorias c
WHERE  NOT EXISTS (SELECT 1 FROM dbo.Categoria x WHERE x.nombre = c.nombre);
GO

-------------------------------------------------------------------------------
-- Productos
-- Columnas: categoría, nombre, precio de venta, stock, stock mínimo, activo
-------------------------------------------------------------------------------
DECLARE @productos TABLE (
    categoria NVARCHAR(100), nombre NVARCHAR(150), descripcion NVARCHAR(500),
    precio DECIMAL(12,2), stock DECIMAL(12,3), minimo DECIMAL(12,3), activo BIT);

INSERT INTO @productos (categoria, nombre, descripcion, precio, stock, minimo, activo) VALUES
    -- Indumentaria: stock de sobra, un caso justo en el mínimo y uno por debajo
    (N'Indumentaria', N'Remera lisa',          N'Dato de prueba. Algodón, varios talles.',  12500.00, 40.000,  5.000, 1),
    (N'Indumentaria', N'Remera estampada',     N'Dato de prueba. Estampa frontal.',         15900.00, 25.000,  5.000, 1),
    (N'Indumentaria', N'Buzo canguro',         N'Dato de prueba. Con capucha.',             28900.00, 15.000,  5.000, 1),
    (N'Indumentaria', N'Campera de abrigo',    N'Dato de prueba. Stock en el mínimo.',      54900.00,  3.000,  3.000, 1),
    (N'Indumentaria', N'Short deportivo',      N'Dato de prueba. Stock por debajo del mínimo.', 11200.00, 2.000, 4.000, 1),

    -- Accesorios
    (N'Accesorios',   N'Gorra',                N'Dato de prueba. Visera curva.',             7200.00, 63.000, 10.000, 1),
    (N'Accesorios',   N'Medias (par)',         N'Dato de prueba. Algodón.',                  2900.00, 120.000, 20.000, 1),
    (N'Accesorios',   N'Mochila urbana',       N'Dato de prueba. Producto SIN stock.',      38500.00,  0.000,  2.000, 1),

    -- Calzado
    (N'Calzado',      N'Zapatillas running',   N'Dato de prueba. Suela amortiguada.',       89900.00, 12.000,  3.000, 1),

    -- Bebidas
    (N'Bebidas',      N'Agua mineral 500 ml',  N'Dato de prueba.',                           1500.00, 200.000, 30.000, 1),
    (N'Bebidas',      N'Gaseosa cola 1,5 L',   N'Dato de prueba.',                           3400.00, 80.000, 15.000, 1),

    -- Almacén: stock decimal, vendido por kilo
    (N'Almacén',      N'Yerba mate (kg)',      N'Dato de prueba. Se vende por kilo.',        6800.00, 25.500,  5.000, 1),
    (N'Almacén',      N'Queso cremoso (kg)',   N'Dato de prueba. Se vende por kilo.',       12400.00, 12.750,  3.000, 1),

    -- Dado de baja: no tiene que aparecer al buscar en Caja
    (N'Indumentaria', N'Remera edición limitada', N'Dato de prueba. Producto dado de baja.', 19900.00, 8.000,  2.000, 0);

INSERT INTO dbo.Producto (id_categoria, nombre, descripcion, precio_venta, stock, stock_minimo, activo)
SELECT c.id_categoria, p.nombre, p.descripcion, p.precio, p.stock, p.minimo, p.activo
FROM        @productos p
INNER JOIN  dbo.Categoria c ON c.nombre = p.categoria
WHERE NOT EXISTS (SELECT 1 FROM dbo.Producto x WHERE x.nombre = p.nombre);
GO

PRINT 'Datos de prueba de categorías y productos cargados correctamente.';
GO

/* -----------------------------------------------------------------------------
   Para BORRAR solo estos datos de prueba más adelante, quitá los comentarios
   de este bloque y ejecutalo. Ojo: si ya cargaste ventas que usan estos
   productos, la base no va a dejar borrarlos.

   DELETE FROM dbo.Producto  WHERE descripcion LIKE N'Dato de prueba.%';
   DELETE FROM dbo.Categoria WHERE nombre IN
       (N'Indumentaria', N'Accesorios', N'Calzado', N'Bebidas', N'Almacén', N'Discontinuados')
     AND NOT EXISTS (SELECT 1 FROM dbo.Producto p WHERE p.id_categoria = dbo.Categoria.id_categoria);
   ----------------------------------------------------------------------------- */
