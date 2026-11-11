/* =============================================================================
   TiendaUNNE - 1. Crear la base de datos y las tablas
   Taller de Programación II - UNNE
   -----------------------------------------------------------------------------
   Motor: SQL Server 2016 o superior (Express alcanza).

   Qué hace:
     - Crea la base TiendaUNNE (si no existe).
     - Crea las 16 tablas con sus claves, restricciones e índices.
     - Carga los 3 perfiles de usuario (Administrador, Supervisor y Cajero).

   Es SEGURO: no borra nada. Cada tabla se crea solo si todavía no existe, así que
   se puede correr más de una vez sin perder datos.

   Orden de ejecución de los scripts de la entrega:
     1_CrearBaseYTablas.sql           (este)
     2_ProcedimientosAlmacenados.sql
     3_DatosIniciales.sql
     4_UsuariosPorPerfil.sql
     5_DatosDeEjemplo.sql             (opcional)

   Las 16 tablas, en orden de creación (respeta las claves foráneas):
      1. Persona            9. Caja
      2. Perfil            10. Caja_sesion
      3. Usuario           11. Tipo_comprobante
      4. Cliente           12. Medio_pago
      5. Auditoria         13. Venta_cabecera
      6. Categoria         14. Venta_detalle
      7. Producto          15. Venta_pago
      8. Reserva_stock     16. Movimiento_caja
   ============================================================================= */

-- Necesario para los índices filtrados (UQ_Cliente_cuit, UQ_CajaSesion_abierta).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-------------------------------------------------------------------------------
-- 0. Base de datos
-------------------------------------------------------------------------------
IF DB_ID('TiendaUNNE') IS NULL
    CREATE DATABASE TiendaUNNE;
GO

USE TiendaUNNE;
GO

-------------------------------------------------------------------------------
-- 1. Persona  (tabla padre de Usuario y Cliente)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Persona', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Persona
    (
        id_persona        INT            IDENTITY(1,1) NOT NULL,
        dni_cuit          NVARCHAR(20)   NOT NULL,   -- documento: hoy se usa solo el DNI
        nombre            NVARCHAR(100)  NOT NULL,
        apellido          NVARCHAR(100)  NOT NULL,
        direccion         NVARCHAR(200)  NULL,
        telefono          NVARCHAR(30)   NULL,
        email             NVARCHAR(150)  NULL,
        fecha_nacimiento  DATE           NULL,
        activo            BIT            NOT NULL CONSTRAINT DF_Persona_activo DEFAULT (1),

        CONSTRAINT PK_Persona PRIMARY KEY (id_persona),
        CONSTRAINT UQ_Persona_dni_cuit UNIQUE (dni_cuit),
        -- El DNI son exactamente 8 dígitos numéricos.
        CONSTRAINT CK_Persona_dni_cuit CHECK (dni_cuit NOT LIKE '%[^0-9]%' AND LEN(dni_cuit) = 8)
    );
END
GO

-------------------------------------------------------------------------------
-- 2. Perfil  (roles de acceso)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Perfil', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfil
    (
        id_perfil     INT            IDENTITY(1,1) NOT NULL,
        nombre        NVARCHAR(50)   NOT NULL,
        descripcion   NVARCHAR(200)  NULL,
        activo        BIT            NOT NULL CONSTRAINT DF_Perfil_activo DEFAULT (1),

        CONSTRAINT PK_Perfil PRIMARY KEY (id_perfil),
        CONSTRAINT UQ_Perfil_nombre UNIQUE (nombre)
    );
END
GO

-------------------------------------------------------------------------------
-- 3. Usuario  (1 a 1 con Persona). Se ingresa con el DNI de la Persona.
--    La contraseña no se guarda: se guarda su hash PBKDF2 y el salt.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Usuario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuario
    (
        id_usuario        INT             IDENTITY(1,1) NOT NULL,
        id_persona        INT             NOT NULL,
        id_perfil         INT             NOT NULL,
        hash_password     VARBINARY(256)  NOT NULL,
        salt              VARBINARY(128)  NULL,
        debe_cambiar_pass BIT             NOT NULL CONSTRAINT DF_Usuario_cambiar_pass DEFAULT (0),
        intentos_fallidos TINYINT         NOT NULL CONSTRAINT DF_Usuario_intentos DEFAULT (0),
        bloqueado         BIT             NOT NULL CONSTRAINT DF_Usuario_bloqueado DEFAULT (0),
        ultimo_acceso     DATETIME2(0)    NULL,
        fecha_alta        DATETIME2(0)    NOT NULL CONSTRAINT DF_Usuario_fecha_alta DEFAULT (SYSDATETIME()),
        activo            BIT             NOT NULL CONSTRAINT DF_Usuario_activo DEFAULT (1),

        CONSTRAINT PK_Usuario PRIMARY KEY (id_usuario),
        CONSTRAINT UQ_Usuario_persona UNIQUE (id_persona),          -- relación 1 a 1
        CONSTRAINT FK_Usuario_Persona FOREIGN KEY (id_persona) REFERENCES dbo.Persona (id_persona),
        CONSTRAINT FK_Usuario_Perfil  FOREIGN KEY (id_perfil)  REFERENCES dbo.Perfil  (id_perfil)
    );
END
GO

-------------------------------------------------------------------------------
-- 4. Cliente  (1 a 1 con Persona)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Cliente', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cliente
    (
        id_cliente        INT             IDENTITY(1,1) NOT NULL,
        id_persona        INT             NOT NULL,
        razon_social      NVARCHAR(150)   NULL,
        cuit              NVARCHAR(11)    NULL,         -- hace falta para la Factura A
        condicion_iva     NVARCHAR(30)    NOT NULL CONSTRAINT DF_Cliente_condicion_iva DEFAULT ('Consumidor Final'),
        limite_credito    DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Cliente_limite_credito DEFAULT (0),
        saldo_cuenta      DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Cliente_saldo_cuenta DEFAULT (0),
        observaciones     NVARCHAR(300)   NULL,
        fecha_alta        DATETIME2(0)    NOT NULL CONSTRAINT DF_Cliente_fecha_alta DEFAULT (SYSDATETIME()),
        activo            BIT             NOT NULL CONSTRAINT DF_Cliente_activo DEFAULT (1),

        CONSTRAINT PK_Cliente PRIMARY KEY (id_cliente),
        CONSTRAINT UQ_Cliente_persona UNIQUE (id_persona),          -- relación 1 a 1
        CONSTRAINT FK_Cliente_Persona FOREIGN KEY (id_persona) REFERENCES dbo.Persona (id_persona),
        CONSTRAINT CK_Cliente_limite_credito CHECK (limite_credito >= 0),
        CONSTRAINT CK_Cliente_cuit CHECK (cuit IS NULL OR (cuit NOT LIKE '%[^0-9]%' AND LEN(cuit) = 11)),
        CONSTRAINT CK_Cliente_condicion_iva CHECK (condicion_iva IN (N'Consumidor Final', N'Responsable Inscripto', N'Monotributista', N'Exento'))
    );

    -- Un CUIT no se repite entre clientes (los NULL no cuentan).
    CREATE UNIQUE INDEX UQ_Cliente_cuit ON dbo.Cliente (cuit) WHERE cuit IS NOT NULL;
END
GO

-------------------------------------------------------------------------------
-- 5. Auditoria  (registro de altas, bajas y modificaciones)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Auditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria
    (
        id_auditoria            BIGINT          IDENTITY(1,1) NOT NULL,
        id_usuario              INT             NULL,               -- quién ejecutó la acción
        fecha_hora              DATETIME2(3)    NOT NULL CONSTRAINT DF_Auditoria_fecha_hora DEFAULT (SYSDATETIME()),
        accion                  NVARCHAR(20)    NOT NULL,           -- ALTA / BAJA / MODIFICACION
        tabla_afectada          NVARCHAR(100)   NOT NULL,
        id_registro_afectado    INT             NULL,
        valor_anterior          NVARCHAR(MAX)   NULL,
        valor_nuevo             NVARCHAR(MAX)   NULL,

        CONSTRAINT PK_Auditoria PRIMARY KEY (id_auditoria),
        CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT CK_Auditoria_accion CHECK (accion IN ('ALTA','BAJA','MODIFICACION'))
    );

    CREATE INDEX IX_Auditoria_fecha   ON dbo.Auditoria (fecha_hora DESC);
    CREATE INDEX IX_Auditoria_usuario ON dbo.Auditoria (id_usuario);
END
GO

-------------------------------------------------------------------------------
-- 6. Categoria
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Categoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categoria
    (
        id_categoria   INT            IDENTITY(1,1) NOT NULL,
        nombre         NVARCHAR(100)  NOT NULL,
        descripcion    NVARCHAR(200)  NULL,
        activo         BIT            NOT NULL CONSTRAINT DF_Categoria_activo DEFAULT (1),

        CONSTRAINT PK_Categoria PRIMARY KEY (id_categoria),
        CONSTRAINT UQ_Categoria_nombre UNIQUE (nombre)
    );
END
GO

-------------------------------------------------------------------------------
-- 7. Producto
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Producto', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Producto
    (
        id_producto     INT             IDENTITY(1,1) NOT NULL,
        id_categoria    INT             NOT NULL,
        nombre          NVARCHAR(150)   NOT NULL,
        descripcion     NVARCHAR(500)   NULL,
        precio_venta    DECIMAL(12,2)   NOT NULL CONSTRAINT DF_Producto_precio_venta DEFAULT (0),   -- precio final, con IVA incluido
        stock           DECIMAL(12,3)   NOT NULL CONSTRAINT DF_Producto_stock DEFAULT (0),
        stock_minimo    DECIMAL(12,3)   NOT NULL CONSTRAINT DF_Producto_stock_minimo DEFAULT (0),   -- umbral de "stock bajo"
        activo          BIT             NOT NULL CONSTRAINT DF_Producto_activo DEFAULT (1),

        CONSTRAINT PK_Producto PRIMARY KEY (id_producto),
        CONSTRAINT FK_Producto_Categoria FOREIGN KEY (id_categoria) REFERENCES dbo.Categoria (id_categoria),
        CONSTRAINT CK_Producto_precio_venta CHECK (precio_venta >= 0),
        CONSTRAINT CK_Producto_stock_minimo CHECK (stock_minimo >= 0)
    );
END
GO

-------------------------------------------------------------------------------
-- 8. Reserva_stock  (stock apartado por los tickets en curso; vence solo)
--    Una fila por ticket (token) y producto. Stock libre = stock - reservas vigentes.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Reserva_stock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reserva_stock
    (
        id_reserva   INT              IDENTITY(1,1) NOT NULL,
        token        UNIQUEIDENTIFIER NOT NULL,                 -- identifica el ticket en curso
        id_producto  INT              NOT NULL,
        id_usuario   INT              NOT NULL,                 -- quién tiene el ticket abierto
        cantidad     DECIMAL(12,3)    NOT NULL,
        creada       DATETIME2(0)     NOT NULL CONSTRAINT DF_Reserva_stock_creada DEFAULT (GETDATE()),
        vence        DATETIME2(0)     NOT NULL,

        CONSTRAINT PK_Reserva_stock PRIMARY KEY (id_reserva),
        CONSTRAINT UQ_Reserva_stock_token_producto UNIQUE (token, id_producto),
        CONSTRAINT FK_Reserva_stock_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Producto (id_producto),
        CONSTRAINT FK_Reserva_stock_Usuario  FOREIGN KEY (id_usuario)  REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT CK_Reserva_stock_cantidad CHECK (cantidad > 0)
    );

    CREATE INDEX IX_Reserva_stock_producto ON dbo.Reserva_stock (id_producto, vence);
END
GO

-------------------------------------------------------------------------------
-- 9. Caja  (puestos físicos de caja)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Caja', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Caja
    (
        id_caja      INT            IDENTITY(1,1) NOT NULL,
        nombre       NVARCHAR(50)   NOT NULL,
        descripcion  NVARCHAR(200)  NULL,
        activo       BIT            NOT NULL CONSTRAINT DF_Caja_activo DEFAULT (1),

        CONSTRAINT PK_Caja PRIMARY KEY (id_caja),
        CONSTRAINT UQ_Caja_nombre UNIQUE (nombre)
    );
END
GO

-------------------------------------------------------------------------------
-- 10. Caja_sesion  (apertura y cierre de caja por turno)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Caja_sesion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Caja_sesion
    (
        id_caja_sesion          INT            IDENTITY(1,1) NOT NULL,
        id_caja                 INT            NOT NULL,
        id_usuario_apertura     INT            NOT NULL,
        id_usuario_cierre       INT            NULL,
        fecha_apertura          DATETIME2(0)   NOT NULL CONSTRAINT DF_CajaSesion_fecha_apertura DEFAULT (SYSDATETIME()),
        fecha_cierre            DATETIME2(0)   NULL,
        monto_inicial           DECIMAL(12,2)  NOT NULL CONSTRAINT DF_CajaSesion_monto_inicial DEFAULT (0),
        monto_final_sistema     DECIMAL(12,2)  NULL,
        monto_final_declarado   DECIMAL(12,2)  NULL,
        diferencia              DECIMAL(12,2)  NULL,
        estado                  NVARCHAR(10)   NOT NULL CONSTRAINT DF_CajaSesion_estado DEFAULT ('ABIERTA'),
        observaciones           NVARCHAR(300)  NULL,

        CONSTRAINT PK_Caja_sesion PRIMARY KEY (id_caja_sesion),
        CONSTRAINT FK_CajaSesion_Caja            FOREIGN KEY (id_caja)             REFERENCES dbo.Caja (id_caja),
        CONSTRAINT FK_CajaSesion_UsuarioApertura FOREIGN KEY (id_usuario_apertura) REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT FK_CajaSesion_UsuarioCierre   FOREIGN KEY (id_usuario_cierre)   REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT CK_CajaSesion_estado CHECK (estado IN ('ABIERTA','CERRADA')),
        CONSTRAINT CK_CajaSesion_fechas CHECK (fecha_cierre IS NULL OR fecha_cierre >= fecha_apertura)
    );

    -- Una sola sesión ABIERTA por caja.
    CREATE UNIQUE INDEX UQ_CajaSesion_abierta
        ON dbo.Caja_sesion (id_caja)
        WHERE estado = 'ABIERTA';
END
GO

-------------------------------------------------------------------------------
-- 11. Tipo_comprobante  (Ticket, Facturas A y B, Notas de crédito)
--     "signo" vale 1 en una venta y -1 en una nota de crédito: así los reportes
--     y el arqueo de caja restan las devoluciones.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Tipo_comprobante', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tipo_comprobante
    (
        id_tipo_comprobante   INT            IDENTITY(1,1) NOT NULL,
        codigo                NVARCHAR(5)    NOT NULL,
        nombre                NVARCHAR(50)   NOT NULL,
        letra                 CHAR(1)        NULL,
        signo                 SMALLINT       NOT NULL CONSTRAINT DF_TipoComp_signo DEFAULT (1),
        afecta_stock          BIT            NOT NULL CONSTRAINT DF_TipoComp_afecta_stock DEFAULT (1),
        afecta_caja           BIT            NOT NULL CONSTRAINT DF_TipoComp_afecta_caja DEFAULT (1),
        activo                BIT            NOT NULL CONSTRAINT DF_TipoComp_activo DEFAULT (1),

        CONSTRAINT PK_Tipo_comprobante PRIMARY KEY (id_tipo_comprobante),
        CONSTRAINT UQ_TipoComp_codigo UNIQUE (codigo),
        CONSTRAINT CK_TipoComp_signo CHECK (signo IN (-1, 1))
    );
END
GO

-------------------------------------------------------------------------------
-- 12. Medio_pago  (Efectivo, tarjetas, transferencia)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Medio_pago', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Medio_pago
    (
        id_medio_pago        INT            IDENTITY(1,1) NOT NULL,
        nombre               NVARCHAR(50)   NOT NULL,
        requiere_referencia  BIT            NOT NULL CONSTRAINT DF_MedioPago_requiere_ref DEFAULT (0),
        recargo_porcentaje   DECIMAL(5,2)   NOT NULL CONSTRAINT DF_MedioPago_recargo DEFAULT (0),
        es_efectivo          BIT            NOT NULL CONSTRAINT DF_MedioPago_es_efectivo DEFAULT (0),
        activo               BIT            NOT NULL CONSTRAINT DF_MedioPago_activo DEFAULT (1),

        CONSTRAINT PK_Medio_pago PRIMARY KEY (id_medio_pago),
        CONSTRAINT UQ_MedioPago_nombre UNIQUE (nombre),
        CONSTRAINT CK_MedioPago_recargo CHECK (recargo_porcentaje >= 0)
    );
END
GO

-------------------------------------------------------------------------------
-- 13. Venta_cabecera  (una fila por comprobante: venta o nota de crédito)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Venta_cabecera', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venta_cabecera
    (
        id_venta              INT             IDENTITY(1,1) NOT NULL,
        id_tipo_comprobante   INT             NOT NULL,
        punto_venta           INT             NOT NULL CONSTRAINT DF_VentaCab_punto_venta DEFAULT (1),
        numero_comprobante    BIGINT          NOT NULL,
        id_cliente            INT             NULL,           -- NULL = consumidor final
        id_usuario            INT             NOT NULL,
        id_caja_sesion        INT             NOT NULL,
        fecha_hora            DATETIME2(0)    NOT NULL CONSTRAINT DF_VentaCab_fecha_hora DEFAULT (SYSDATETIME()),
        subtotal              DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaCab_subtotal DEFAULT (0),
        descuento             DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaCab_descuento DEFAULT (0),
        importe_neto          DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaCab_neto DEFAULT (0),
        importe_iva           DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaCab_iva DEFAULT (0),
        total                 DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaCab_total DEFAULT (0),
        estado                NVARCHAR(10)    NOT NULL CONSTRAINT DF_VentaCab_estado DEFAULT ('EMITIDA'),
        id_venta_anulada      INT             NULL,           -- en una nota de crédito: la venta que corrige
        observaciones         NVARCHAR(300)   NULL,

        CONSTRAINT PK_Venta_cabecera PRIMARY KEY (id_venta),
        CONSTRAINT UQ_VentaCab_numero UNIQUE (id_tipo_comprobante, punto_venta, numero_comprobante),
        CONSTRAINT FK_VentaCab_TipoComprobante FOREIGN KEY (id_tipo_comprobante) REFERENCES dbo.Tipo_comprobante (id_tipo_comprobante),
        CONSTRAINT FK_VentaCab_Cliente         FOREIGN KEY (id_cliente)          REFERENCES dbo.Cliente (id_cliente),
        CONSTRAINT FK_VentaCab_Usuario         FOREIGN KEY (id_usuario)          REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT FK_VentaCab_CajaSesion      FOREIGN KEY (id_caja_sesion)      REFERENCES dbo.Caja_sesion (id_caja_sesion),
        CONSTRAINT FK_VentaCab_VentaAnulada    FOREIGN KEY (id_venta_anulada)    REFERENCES dbo.Venta_cabecera (id_venta),
        CONSTRAINT CK_VentaCab_estado CHECK (estado IN ('EMITIDA','ANULADA')),
        CONSTRAINT CK_VentaCab_total CHECK (total >= 0)
    );

    CREATE INDEX IX_VentaCab_fecha   ON dbo.Venta_cabecera (fecha_hora DESC);
    CREATE INDEX IX_VentaCab_cliente ON dbo.Venta_cabecera (id_cliente);
    CREATE INDEX IX_VentaCab_sesion  ON dbo.Venta_cabecera (id_caja_sesion);
END
GO

-------------------------------------------------------------------------------
-- 14. Venta_detalle  (renglones del comprobante)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Venta_detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venta_detalle
    (
        id_venta_detalle       INT             IDENTITY(1,1) NOT NULL,
        id_venta               INT             NOT NULL,
        id_producto            INT             NOT NULL,
        nro_linea              INT             NOT NULL,
        descripcion            NVARCHAR(150)   NOT NULL,      -- descripción "congelada" al momento de la venta
        cantidad               DECIMAL(12,3)   NOT NULL,
        precio_unitario        DECIMAL(12,2)   NOT NULL,
        descuento_porcentaje   DECIMAL(5,2)    NOT NULL CONSTRAINT DF_VentaDet_desc_pct DEFAULT (0),
        alicuota_iva           DECIMAL(5,2)    NOT NULL CONSTRAINT DF_VentaDet_alicuota_iva DEFAULT (21.00),
        importe_iva            DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaDet_importe_iva DEFAULT (0),
        subtotal               DECIMAL(12,2)   NOT NULL,

        CONSTRAINT PK_Venta_detalle PRIMARY KEY (id_venta_detalle),
        CONSTRAINT UQ_VentaDet_linea UNIQUE (id_venta, nro_linea),
        CONSTRAINT FK_VentaDet_Venta    FOREIGN KEY (id_venta)    REFERENCES dbo.Venta_cabecera (id_venta) ON DELETE CASCADE,
        CONSTRAINT FK_VentaDet_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Producto (id_producto),
        CONSTRAINT CK_VentaDet_cantidad CHECK (cantidad > 0),
        CONSTRAINT CK_VentaDet_precio   CHECK (precio_unitario >= 0),
        CONSTRAINT CK_VentaDet_desc_pct CHECK (descuento_porcentaje >= 0 AND descuento_porcentaje <= 100)
    );

    CREATE INDEX IX_VentaDet_producto ON dbo.Venta_detalle (id_producto);
END
GO

-------------------------------------------------------------------------------
-- 15. Venta_pago  (formas de pago aplicadas al comprobante)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Venta_pago', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venta_pago
    (
        id_venta_pago    INT             IDENTITY(1,1) NOT NULL,
        id_venta         INT             NOT NULL,
        id_medio_pago    INT             NOT NULL,
        importe          DECIMAL(12,2)   NOT NULL,
        recargo          DECIMAL(12,2)   NOT NULL CONSTRAINT DF_VentaPago_recargo DEFAULT (0),
        referencia       NVARCHAR(100)   NULL,          -- nro de cupón, transferencia o autorización
        cuotas           TINYINT         NULL,
        fecha_hora       DATETIME2(0)    NOT NULL CONSTRAINT DF_VentaPago_fecha_hora DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_Venta_pago PRIMARY KEY (id_venta_pago),
        CONSTRAINT FK_VentaPago_Venta     FOREIGN KEY (id_venta)      REFERENCES dbo.Venta_cabecera (id_venta) ON DELETE CASCADE,
        CONSTRAINT FK_VentaPago_MedioPago FOREIGN KEY (id_medio_pago) REFERENCES dbo.Medio_pago (id_medio_pago),
        CONSTRAINT CK_VentaPago_importe CHECK (importe > 0)
    );

    CREATE INDEX IX_VentaPago_venta ON dbo.Venta_pago (id_venta);
END
GO

-------------------------------------------------------------------------------
-- 16. Movimiento_caja  (ingresos y egresos del turno de caja)
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.Movimiento_caja', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Movimiento_caja
    (
        id_movimiento     BIGINT          IDENTITY(1,1) NOT NULL,
        id_caja_sesion    INT             NOT NULL,
        id_usuario        INT             NOT NULL,
        id_medio_pago     INT             NULL,
        id_venta          INT             NULL,          -- movimiento originado por una venta
        fecha_hora        DATETIME2(0)    NOT NULL CONSTRAINT DF_MovCaja_fecha_hora DEFAULT (SYSDATETIME()),
        tipo              NVARCHAR(10)    NOT NULL,
        concepto          NVARCHAR(150)   NOT NULL,
        monto             DECIMAL(12,2)   NOT NULL,
        observaciones     NVARCHAR(300)   NULL,

        CONSTRAINT PK_Movimiento_caja PRIMARY KEY (id_movimiento),
        CONSTRAINT FK_MovCaja_CajaSesion FOREIGN KEY (id_caja_sesion) REFERENCES dbo.Caja_sesion (id_caja_sesion),
        CONSTRAINT FK_MovCaja_Usuario    FOREIGN KEY (id_usuario)     REFERENCES dbo.Usuario (id_usuario),
        CONSTRAINT FK_MovCaja_MedioPago  FOREIGN KEY (id_medio_pago)  REFERENCES dbo.Medio_pago (id_medio_pago),
        CONSTRAINT FK_MovCaja_Venta      FOREIGN KEY (id_venta)       REFERENCES dbo.Venta_cabecera (id_venta),
        CONSTRAINT CK_MovCaja_tipo  CHECK (tipo IN ('INGRESO','EGRESO')),
        CONSTRAINT CK_MovCaja_monto CHECK (monto > 0)
    );

    CREATE INDEX IX_MovCaja_sesion ON dbo.Movimiento_caja (id_caja_sesion);
END
GO

-------------------------------------------------------------------------------
-- Datos fijos: los 3 perfiles de usuario (se cargan solo si faltan)
-------------------------------------------------------------------------------
INSERT INTO dbo.Perfil (nombre, descripcion)
SELECT v.nombre, v.descripcion
FROM (VALUES
    (N'Administrador', N'Acceso total al sistema'),
    (N'Supervisor',    N'Supervisión de caja, ventas y reportes'),
    (N'Cajero',        N'Operación de caja y registro de ventas')
) AS v(nombre, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Perfil p WHERE p.nombre = v.nombre);
GO

PRINT 'Listo: base TiendaUNNE con sus 16 tablas y los 3 perfiles.';
GO
