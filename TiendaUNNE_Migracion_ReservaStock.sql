-------------------------------------------------------------------------------
-- Migración: reserva de stock para los tickets en curso.
-- Para bases ya creadas. Se puede ejecutar más de una vez sin problema.
-------------------------------------------------------------------------------
USE TiendaUNNE;
GO

IF OBJECT_ID('dbo.Reserva_stock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reserva_stock
    (
        id_reserva   INT              IDENTITY(1,1) NOT NULL,
        token        UNIQUEIDENTIFIER NOT NULL,
        id_producto  INT              NOT NULL,
        id_usuario   INT              NOT NULL,
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

PRINT 'Reserva_stock lista.';
GO
