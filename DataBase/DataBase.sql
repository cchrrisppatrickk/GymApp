-- 1. CREACIÓN DE LA BASE DE DATOS


USE GymDB;
GO

-- ==============================================================================
-- FASE 1: ELIMINACIÓN DE TABLAS (Orden inverso por dependencias FK)
-- ==============================================================================

-- 1. Nivel más bajo (Tablas que dependen de muchas otras)
IF OBJECT_ID('VentasDetalle', 'U') IS NOT NULL DROP TABLE VentasDetalle;
IF OBJECT_ID('PagosMembresia', 'U') IS NOT NULL DROP TABLE PagosMembresia;
IF OBJECT_ID('Congelamientos', 'U') IS NOT NULL DROP TABLE Congelamientos;

-- 2. Nivel intermedio (Tablas que dependen de Usuarios/Membresías)
IF OBJECT_ID('VentasCabecera', 'U') IS NOT NULL DROP TABLE VentasCabecera;
IF OBJECT_ID('Asistencias', 'U') IS NOT NULL DROP TABLE Asistencias;
IF OBJECT_ID('Membresias', 'U') IS NOT NULL DROP TABLE Membresias;

-- 3. Nivel Crítico (Usuarios depende de Roles, pero otros dependían de Usuarios)
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;

-- 4. Nivel Independiente (Catálogos base)
IF OBJECT_ID('Productos', 'U') IS NOT NULL DROP TABLE Productos;
IF OBJECT_ID('Planes', 'U') IS NOT NULL DROP TABLE Planes;
IF OBJECT_ID('Turnos', 'U') IS NOT NULL DROP TABLE Turnos;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;

GO
PRINT '--- Tablas eliminadas correctamente ---';
GO


USE GymDB;
GO

-- =============================================
-- 1. TABLAS DE CATÁLOGOS (Sin dependencias)
-- =============================================

-- Tabla: ROLES
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL, -- Admin, Portero, Cliente
    Descripcion NVARCHAR(100) NULL
);

-- Tabla: TURNOS
CREATE TABLE Turnos (
    TurnoID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL, -- Ej: Mañana
    HoraInicio TIME(0) NOT NULL,
    HoraFin TIME(0) NOT NULL,
    Descripcion NVARCHAR(100) NULL
);

-- Tabla: PLANES
CREATE TABLE Planes (
    PlanID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL, -- Ej: Mensual, Trimestral
    DuracionDias INT NOT NULL,
    PrecioBase DECIMAL(10,2) NOT NULL,
    PermiteCongelar BIT DEFAULT 0 -- 1=Sí, 0=No
);

-- Tabla: PRODUCTOS
CREATE TABLE Productos (
    ProductoID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PrecioVenta DECIMAL(10,2) NOT NULL,
    StockActual INT DEFAULT 0,
    CodigoBarras NVARCHAR(50) NULL
);

-- =============================================
-- 2. TABLA DE USUARIOS (Núcleo del sistema)
-- =============================================

CREATE TABLE Usuarios (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    NombreCompleto NVARCHAR(100) NOT NULL,
    
    -- Credenciales de Acceso
    NombreUsuario NVARCHAR(50) NOT NULL UNIQUE, -- Ya integrado aquí
    DNI NVARCHAR(15) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NULL, 
    
    -- Datos de Contacto y Estado
    Telefono NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    CodigoQR UNIQUEIDENTIFIER DEFAULT NEWID(), -- Generación automática
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Estado BIT DEFAULT 1, -- 1=Activo, 0=Inactivo
    
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- =============================================
-- 3. TABLAS PRINCIPALES (Membresías)
-- =============================================

CREATE TABLE Membresias (
    MembresiaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    PlanID INT NOT NULL,
    TurnoID INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaVencimiento DATE NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Activa', -- Activa, Vencida, Cancelada
    Observaciones NVARCHAR(200) NULL,
    
    CONSTRAINT FK_Membresias_Usuarios FOREIGN KEY (UserID) REFERENCES Usuarios(UserID),
    CONSTRAINT FK_Membresias_Planes FOREIGN KEY (PlanID) REFERENCES Planes(PlanID),
    CONSTRAINT FK_Membresias_Turnos FOREIGN KEY (TurnoID) REFERENCES Turnos(TurnoID)
);

-- =============================================
-- 4. TABLAS TRANSACCIONALES (Operación Diaria)
-- =============================================

-- Pagos de Membresía
CREATE TABLE PagosMembresia (
    PagoID INT IDENTITY(1,1) PRIMARY KEY,
    MembresiaID INT NOT NULL,
    UsuarioEmpleadoID INT NOT NULL, -- Auditoría: ¿Quién cobró?
    Monto DECIMAL(10,2) NOT NULL,
    MetodoPago NVARCHAR(50) NOT NULL, -- Efectivo, Yape, Tarjeta
    FechaPago DATETIME DEFAULT GETDATE(),
    Comprobante NVARCHAR(50) NULL,
    
    CONSTRAINT FK_Pagos_Membresias FOREIGN KEY (MembresiaID) REFERENCES Membresias(MembresiaID),
    CONSTRAINT FK_Pagos_Empleado FOREIGN KEY (UsuarioEmpleadoID) REFERENCES Usuarios(UserID)
);

-- Congelamientos
CREATE TABLE Congelamientos (
    CongelamientoID INT IDENTITY(1,1) PRIMARY KEY,
    MembresiaID INT NOT NULL,
    UsuarioEmpleadoID INT NOT NULL, -- Auditoría: ¿Quién autorizó?
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Motivo NVARCHAR(200) NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Congelamientos_Membresias FOREIGN KEY (MembresiaID) REFERENCES Membresias(MembresiaID),
    CONSTRAINT FK_Congelamientos_Empleado FOREIGN KEY (UsuarioEmpleadoID) REFERENCES Usuarios(UserID)
);

-- Asistencias (Torniquete)
CREATE TABLE Asistencias (
    AsistenciaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    FechaHora DATETIME DEFAULT GETDATE(),
    AccesoPermitido BIT NOT NULL, -- 1=Entró, 0=Rebotado
    MotivoDenegacion NVARCHAR(100) NULL,
    
    CONSTRAINT FK_Asistencias_Usuarios FOREIGN KEY (UserID) REFERENCES Usuarios(UserID)
);

-- =============================================
-- 5. MÓDULO POS (Ventas de Productos)
-- =============================================

CREATE TABLE VentasCabecera (
    VentaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL, -- Puede ser NULL si es venta a público general
    UsuarioEmpleadoID INT NOT NULL,
    FechaVenta DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    MetodoPago NVARCHAR(50) NOT NULL,
    
    CONSTRAINT FK_Ventas_Cliente FOREIGN KEY (UserID) REFERENCES Usuarios(UserID),
    CONSTRAINT FK_Ventas_Empleado FOREIGN KEY (UsuarioEmpleadoID) REFERENCES Usuarios(UserID)
);

CREATE TABLE VentasDetalle (
    DetalleID INT IDENTITY(1,1) PRIMARY KEY,
    VentaID INT NOT NULL,
    ProductoID INT NOT NULL,
    Cantidad INT NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    
    CONSTRAINT FK_Detalle_Venta FOREIGN KEY (VentaID) REFERENCES VentasCabecera(VentaID),
    CONSTRAINT FK_Detalle_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO



Select * From Usuarios 



INSERT INTO Usuarios (
    RoleID, 
    NombreCompleto, 
    NombreUsuario, 
    DNI, 
    PasswordHash, 
    Telefono, 
    Email
) 
VALUES (
    1, 
    'Admin1', 
    '1234567', 
    '987654321', 
    '$2a$11$DBZ.giFlWeDTLS6AQEqf6Oki7HAMwAp5vzRmtJDd.gGdGLOLXQA7e', 
    '987654321', 
    'admin1@gmail.com'
);

