-- 1. CREACIÓN DE LA BASE DE DATOS

CREATE DATABASE GymDB;
USE GymDB;


select * from Roles
select * from Usuarios

-- 2. TABLAS DE CATÁLOGOS Y CONFIGURACIÓN (Sin dependencias)

-- Tabla: ROLES
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL, -- Admin, Portero, Cliente
    Descripcion NVARCHAR(100) NULL
);

-- Tabla: TURNOS (Para validación estricta)
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

-- Tabla: PRODUCTOS (Para el POS)
CREATE TABLE Productos (
    ProductoID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    PrecioVenta DECIMAL(10,2) NOT NULL,
    StockActual INT DEFAULT 0,
    CodigoBarras NVARCHAR(50) NULL -- Para pistola de código de barras
);

-- 3. TABLAS DE USUARIOS (Dependen de Roles)

CREATE TABLE Usuarios (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    NombreCompleto NVARCHAR(100) NOT NULL,
    DNI NVARCHAR(15) NOT NULL UNIQUE, -- DNI no debe repetirse
    Telefono NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    PasswordHash NVARCHAR(255) NULL, -- Nullable por si registras presencial sin pass inicial
    CodigoQR UNIQUEIDENTIFIER DEFAULT NEWID(), -- Se genera solo automáticamente
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Estado BIT DEFAULT 1, -- 1=Activo, 0=Inactivo
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 4. TABLAS PRINCIPALES (Membresías)

CREATE TABLE Membresias (
    MembresiaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL, -- El cliente
    PlanID INT NOT NULL, -- El plan contratado
    TurnoID INT NOT NULL, -- El horario permitido
    FechaInicio DATE NOT NULL,
    FechaVencimiento DATE NOT NULL,
    Estado NVARCHAR(20) DEFAULT 'Activa', -- Activa, Vencida, Cancelada
    Observaciones NVARCHAR(200) NULL,
    CONSTRAINT FK_Membresias_Usuarios FOREIGN KEY (UserID) REFERENCES Usuarios(UserID),
    CONSTRAINT FK_Membresias_Planes FOREIGN KEY (PlanID) REFERENCES Planes(PlanID),
    CONSTRAINT FK_Membresias_Turnos FOREIGN KEY (TurnoID) REFERENCES Turnos(TurnoID)
);

-- 5. TABLAS TRANSACCIONALES (Pagos, Asistencia, Congelamientos)

-- Auditoría y Pagos de Membresía
CREATE TABLE PagosMembresia (
    PagoID INT IDENTITY(1,1) PRIMARY KEY,
    MembresiaID INT NOT NULL,
    UsuarioEmpleadoID INT NOT NULL, -- ¿Quién cobró? (Auditoría)
    Monto DECIMAL(10,2) NOT NULL,
    MetodoPago NVARCHAR(50) NOT NULL, -- Efectivo, Yape, Tarjeta
    FechaPago DATETIME DEFAULT GETDATE(),
    Comprobante NVARCHAR(50) NULL,
    CONSTRAINT FK_Pagos_Membresias FOREIGN KEY (MembresiaID) REFERENCES Membresias(MembresiaID),
    CONSTRAINT FK_Pagos_Empleado FOREIGN KEY (UsuarioEmpleadoID) REFERENCES Usuarios(UserID) -- Auto-relación
);

-- Congelamientos (Pausas)
CREATE TABLE Congelamientos (
    CongelamientoID INT IDENTITY(1,1) PRIMARY KEY,
    MembresiaID INT NOT NULL,
    UsuarioEmpleadoID INT NOT NULL, -- ¿Quién autorizó?
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Motivo NVARCHAR(200) NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Congelamientos_Membresias FOREIGN KEY (MembresiaID) REFERENCES Membresias(MembresiaID),
    CONSTRAINT FK_Congelamientos_Empleado FOREIGN KEY (UsuarioEmpleadoID) REFERENCES Usuarios(UserID)
);

-- Registro de Asistencias (Log de torniquete/entrada)
CREATE TABLE Asistencias (
    AsistenciaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    FechaHora DATETIME DEFAULT GETDATE(),
    AccesoPermitido BIT NOT NULL, -- 1=Entró, 0=Rebotado
    MotivoDenegacion NVARCHAR(100) NULL, -- 'Membresía Vencida', 'Turno Incorrecto'
    CONSTRAINT FK_Asistencias_Usuarios FOREIGN KEY (UserID) REFERENCES Usuarios(UserID)
);

-- 6. MÓDULO POS (VENTAS DE PRODUCTOS)

CREATE TABLE VentasCabecera (
    VentaID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NULL, -- Cliente que compra (Null si es venta a público general sin registro)
    UsuarioEmpleadoID INT NOT NULL, -- Empleado que vende
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
    PrecioUnitario DECIMAL(10,2) NOT NULL, -- Precio al momento de la venta
    Subtotal DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_Detalle_Venta FOREIGN KEY (VentaID) REFERENCES VentasCabecera(VentaID),
    CONSTRAINT FK_Detalle_Producto FOREIGN KEY (ProductoID) REFERENCES Productos(ProductoID)
);
GO

-- 7. DATOS SEMILLA (SEED DATA) - Opcional pero recomendado

-- Insertar Roles Básicos
INSERT INTO Roles (Nombre, Descripcion) VALUES 
('Administrador', 'Acceso total al sistema'),
('Portero', 'Solo escaneo de QR y ventas básicas'),
('Cliente', 'Acceso a app móvil y consulta de estado');

-- Insertar Turnos Básicos
INSERT INTO Turnos (Nombre, HoraInicio, HoraFin, Descripcion) VALUES
('Mañana', '06:00:00', '13:00:00', 'Acceso solo mañanas'),
('Tarde', '13:00:00', '23:00:00', 'Acceso solo tardes'),
('Full Pass', '06:00:00', '23:00:00', 'Acceso libre todo el día');

-- Insertar Usuario Admin Inicial (Para que puedas loguearte la primera vez)
-- NOTA: En producción, el PasswordHash debe ser un hash real, no texto plano.
INSERT INTO Usuarios (RoleID, NombreCompleto, DNI, Email, PasswordHash, Estado) VALUES
(1, 'Administrador Principal', '00000000', 'admin@stefanosgym.com', 'admin123', 1);

GO