-- ============================================
-- SCRIPT PARA CREAR LA BASE DE DATOS Dbpedidos
-- SQL Server Express (SQL Management Studio)
-- ============================================

-- Crear la base de datos
CREATE DATABASE Dbpedidos;
GO

USE Dbpedidos;
GO

-- ============================================
-- TABLAS MAESTRAS
-- ============================================

-- CATEGORIA
CREATE TABLE categoria (
    idcategoria INT IDENTITY(1,1) NOT NULL,
    nombre      VARCHAR(MAX) NOT NULL,
    descripcion VARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_categoria PRIMARY KEY (idcategoria)
);
GO

-- PRESENTACION
CREATE TABLE presentacion (
    idpresentacion INT IDENTITY(1,1) NOT NULL,
    nombre          VARCHAR(MAX) NOT NULL,
    descripcion     VARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_presentacion PRIMARY KEY (idpresentacion)
);
GO

-- ARTICULO
CREATE TABLE articulo (
    idarticulo     INT IDENTITY(1,1) NOT NULL,
    codigo         VARCHAR(MAX) NOT NULL,
    nombre         VARCHAR(MAX) NOT NULL,
    descripcion    VARCHAR(MAX) NULL,
    imagen         VARBINARY(MAX) NULL,
    idcategoria    INT NOT NULL,
    idpresentacion INT NOT NULL,
    CONSTRAINT PK_articulo PRIMARY KEY (idarticulo),
    CONSTRAINT FK_articulo_categoria FOREIGN KEY (idcategoria) REFERENCES categoria(idcategoria),
    CONSTRAINT FK_articulo_presentacion FOREIGN KEY (idpresentacion) REFERENCES presentacion(idpresentacion)
);
GO

-- CLIENTE
CREATE TABLE cliente (
    idcliente        INT IDENTITY(1,1) NOT NULL,
    nombre           VARCHAR(MAX) NOT NULL,
    apellidos        VARCHAR(MAX) NULL,
    sexo             VARCHAR(MAX) NULL,
    fecha_nacimiento DATETIME2 NULL,
    tipo_documento   VARCHAR(MAX) NOT NULL,
    num_documento    VARCHAR(MAX) NOT NULL,
    direccion        VARCHAR(MAX) NULL,
    telefono         VARCHAR(MAX) NULL,
    email            VARCHAR(MAX) NULL,
    CONSTRAINT PK_cliente PRIMARY KEY (idcliente)
);
GO

-- PROVEEDOR
CREATE TABLE proveedor (
    idproveedor      INT IDENTITY(1,1) NOT NULL,
    razon_social     VARCHAR(MAX) NOT NULL,
    sector_comercial VARCHAR(MAX) NOT NULL,
    tipo_documento   VARCHAR(MAX) NOT NULL,
    num_documento    VARCHAR(MAX) NOT NULL,
    direccion        VARCHAR(MAX) NULL,
    telefono         VARCHAR(MAX) NULL,
    email            VARCHAR(MAX) NULL,
    url              VARCHAR(MAX) NULL,
    CONSTRAINT PK_proveedor PRIMARY KEY (idproveedor)
);
GO

-- TRABAJADOR
CREATE TABLE trabajador (
    idtrabajador   INT IDENTITY(1,1) NOT NULL,
    nombre         VARCHAR(MAX) NOT NULL,
    apellidos      VARCHAR(MAX) NOT NULL,
    sexo           VARCHAR(MAX) NOT NULL,
    fecha_nac      DATETIME2 NOT NULL,
    num_documento  VARCHAR(MAX) NOT NULL,
    direccion      VARCHAR(MAX) NULL,
    telefono       VARCHAR(MAX) NULL,
    email          VARCHAR(MAX) NULL,
    acceso         VARCHAR(MAX) NOT NULL,
    usuario        VARCHAR(MAX) NOT NULL,
    password       VARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_trabajador PRIMARY KEY (idtrabajador)
);
GO

-- ============================================
-- TABLAS DE MOVIMIENTO
-- ============================================

-- INGRESO
CREATE TABLE ingreso (
    idingreso        INT IDENTITY(1,1) NOT NULL,
    idtrabajador     INT NOT NULL,
    idproveedor      INT NOT NULL,
    fecha            DATETIME2 NOT NULL,
    tipo_comprobante VARCHAR(MAX) NOT NULL,
    serie            VARCHAR(10) NOT NULL,
    correlativo      VARCHAR(7) NOT NULL,
    igv              DECIMAL(18,2) NOT NULL,
    estado           VARCHAR(MAX) NOT NULL,
    CONSTRAINT PK_ingreso PRIMARY KEY (idingreso),
    CONSTRAINT FK_ingreso_trabajador FOREIGN KEY (idtrabajador) REFERENCES trabajador(idtrabajador),
    CONSTRAINT FK_ingreso_proveedor FOREIGN KEY (idproveedor) REFERENCES proveedor(idproveedor)
);
GO

-- DETALLE INGRESO
CREATE TABLE detalle_ingreso (
    iddetalle_ingreso INT IDENTITY(1,1) NOT NULL,
    idingreso         INT NOT NULL,
    idarticulo        INT NOT NULL,
    precio_compra     DECIMAL(18,2) NOT NULL,
    precio_venta      DECIMAL(18,2) NOT NULL,
    stock_inicial     INT NOT NULL,
    stock_actual      INT NOT NULL,
    fecha_produccion  DATETIME2 NOT NULL,
    fecha_vencimiento DATETIME2 NOT NULL,
    CONSTRAINT PK_detalle_ingreso PRIMARY KEY (iddetalle_ingreso),
    CONSTRAINT FK_detalleingreso_ingreso FOREIGN KEY (idingreso) REFERENCES ingreso(idingreso),
    CONSTRAINT FK_detalleingreso_articulo FOREIGN KEY (idarticulo) REFERENCES articulo(idarticulo)
);
GO

-- VENTA
CREATE TABLE venta (
    idventa          INT IDENTITY(1,1) NOT NULL,
    idcliente        INT NOT NULL,
    idtrabajador     INT NOT NULL,
    fecha            DATETIME2 NOT NULL,
    tipo_comprobante VARCHAR(MAX) NOT NULL,
    serie            VARCHAR(MAX) NOT NULL,
    correlativo      VARCHAR(MAX) NOT NULL,
    igv              DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_venta PRIMARY KEY (idventa),
    CONSTRAINT FK_venta_cliente FOREIGN KEY (idcliente) REFERENCES cliente(idcliente),
    CONSTRAINT FK_venta_trabajador FOREIGN KEY (idtrabajador) REFERENCES trabajador(idtrabajador)
);
GO

-- DETALLE VENTA
CREATE TABLE detalle_venta (
    iddetalle_venta   INT IDENTITY(1,1) NOT NULL,
    idventa           INT NOT NULL,
    iddetalle_ingreso INT NOT NULL,
    cantidad          INT NOT NULL,
    precio_venta      DECIMAL(18,2) NOT NULL,
    descuento         DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_detalle_venta PRIMARY KEY (iddetalle_venta),
    CONSTRAINT FK_detalleventa_venta FOREIGN KEY (idventa) REFERENCES venta(idventa),
    CONSTRAINT FK_detalleventa_detalleingreso FOREIGN KEY (iddetalle_ingreso) REFERENCES detalle_ingreso(iddetalle_ingreso)
);
GO

-- ============================================
-- TABLAS DE SEGURIDAD Y AUDITORIA
-- ============================================

-- USERS (autenticacion JWT)
CREATE TABLE Users (
    Id            INT IDENTITY(1,1) NOT NULL,
    UserName      VARCHAR(MAX) NOT NULL,
    PasswordHash  VARBINARY(MAX) NOT NULL,
    PasswordSalt  VARBINARY(MAX) NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id)
);
GO

-- AUDIT LOG
CREATE TABLE audit_log (
    id          INT IDENTITY(1,1) NOT NULL,
    entity_name VARCHAR(100) NOT NULL,
    entity_id   VARCHAR(50) NOT NULL,
    action      VARCHAR(20) NOT NULL,
    old_values  VARCHAR(MAX) NULL,
    new_values  VARCHAR(MAX) NULL,
    user_name   VARCHAR(100) NULL,
    ip_address  VARCHAR(50) NULL,
    timestamp   DATETIME2 NOT NULL,
    CONSTRAINT PK_audit_log PRIMARY KEY (id)
);
GO

-- ============================================
-- INDICES RECOMENDADOS
-- ============================================

CREATE INDEX IX_articulo_idcategoria ON articulo(idcategoria);
CREATE INDEX IX_articulo_idpresentacion ON articulo(idpresentacion);
CREATE INDEX IX_ingreso_idtrabajador ON ingreso(idtrabajador);
CREATE INDEX IX_ingreso_idproveedor ON ingreso(idproveedor);
CREATE INDEX IX_detalle_ingreso_idingreso ON detalle_ingreso(idingreso);
CREATE INDEX IX_detalle_ingreso_idarticulo ON detalle_ingreso(idarticulo);
CREATE INDEX IX_venta_idcliente ON venta(idcliente);
CREATE INDEX IX_venta_idtrabajador ON venta(idtrabajador);
CREATE INDEX IX_detalle_venta_idventa ON detalle_venta(idventa);
CREATE INDEX IX_detalle_venta_iddetalle_ingreso ON detalle_venta(iddetalle_ingreso);
CREATE INDEX IX_audit_log_entity_name ON audit_log(entity_name);
CREATE INDEX IX_audit_log_timestamp ON audit_log(timestamp DESC);
GO

PRINT 'Base de datos Dbpedidos creada exitosamente.';
GO
