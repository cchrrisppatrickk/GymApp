# GymApp — Resumen Técnico

## 🏋️ Descripción General

**GymApp** es un sistema de administración integral para gimnasios. Permite gestionar membresías, controlar acceso físico de socios, registrar pagos, administrar ventas de productos y generar reportes financieros. La aplicación está orientada al uso interno del personal del gimnasio (administradores y empleados) y ofrece una vista limitada para clientes.

---

## ⚙️ Funcionalidades Principales

| Módulo | Descripción |
|---|---|
| **Gestión de Membresías** | Alta, edición y listado paginado de membresías. Soporte de renovación inteligente (la nueva membresía inicia el día siguiente al vencimiento anterior). Precio acordado fijo al momento de la venta (`PrecioAcordado`). |
| **Congelamiento de Membresías** | Suspensión temporal de una membresía con extensión automática de la fecha de vencimiento por los días congelados. Requiere que el plan lo permita (`PermiteCongelar`). Historial de congelamientos. |
| **Control de Pagos** | Registro de abonos parciales o totales con soporte para múltiples métodos de pago (Efectivo, Yape). Auditoría del empleado que registra cada cobro. Cálculo automático de deuda pendiente. |
| **Control de Acceso QR** | Cada usuario tiene un código QR único (`Guid`). Un escáner de tablet valida el QR en tiempo real, verifica la membresía activa y registra la asistencia en la tabla `Asistencias`. |
| **Ventas / POS** | Sistema de punto de venta (quiosco) para venta de productos y servicios. Descuento de stock automático para productos físicos. Precio editable en caja. Soporte de ventas a clientes no registrados. |
| **Gestión de Usuarios** | CRUD completo con roles (Admin, Empleado, Cliente). Hashing de contraseñas con BCrypt. Foto de perfil con upload a sistema de archivos. Login por DNI o nombre de usuario. Generación de imagen QR en PNG. |
| **Reportes e Ingresos** | Reporte mensual de ingresos por categoría (Bebidas, Libres, XB) y turno (Mañana/Tarde), desglosado por método de pago. Exportación a Excel con ClosedXML con formato profesional. |
| **Reporte de Membresías** | Listado mensual de membresías con estado, pagos desglosados y exportación a Excel. |
| **Dashboard Analítico** | Estadísticas en tiempo real: nuevos miembros del mes, vencidos sin renovar, por vencer en 7 días, usuarios con deuda, monto total deuda, membresías congeladas. Gráficos de ingresos mensuales y semanales. |
| **Notificaciones / n8n** | Integración con n8n vía Webhooks. Notificaciones instantáneas (Telegram/WhatsApp) para nuevos registros, pagos y membresías. Reportes automatizados programados. |
| **Tareas Programadas** | Orquestación con Hangfire para envío diario de alertas de vencimiento y reportes de desempeño financiero/operativo sin intervención manual. |

---

## 🛠️ Stack Tecnológico

### Backend
| Tecnología | Versión | Rol |
|---|---|---|
| **.NET** | 8.0 | Runtime y SDK de la aplicación |
| **ASP.NET Core MVC** | 8.0 | Framework web (vistas + API JSON mixta) |
| **Entity Framework Core** | 8.0.22 | ORM — acceso a base de datos |
| **EF Core SqlServer** | 8.0.22 | Proveedor de BD para SQL Server |
| **BCrypt.Net-Next** | 4.0.3 | Hashing seguro de contraseñas |
| **QRCoder** | 1.7.0 | Generación de imágenes QR en PNG |
| **ClosedXML** | 0.105.0 | Exportación de reportes a Excel (`.xlsx`) |
| **Hangfire** | 1.8.23 | Orquestación de tareas en segundo plano (Jobs) |
| **WebhookService** | — | Servicio custom para integración con n8n |

### Base de Datos
| Tecnología | Versión | Rol |
|---|---|---|
| **SQL Server** | 2022 (Developer) | Motor de base de datos relacional |
| **EF Core Migrations** | — | Versionado y gestión del esquema |

### Infraestructura / DevOps
| Tecnología | Rol |
|---|---|
| **Docker** | Contenedorización de la app y la BD |
| **Docker Compose** | Orquestación de los servicios (`gymapp_web` + `gym_sql_server`) |
| **Volumes Docker** | Persistencia de datos SQL y fotos de perfil |
| **dotnet watch** | Hot-reload en desarrollo |

### Frontend
| Tecnología | Rol |
|---|---|
| **Razor Views** (`.cshtml`) | Renderizado server-side de HTML |
| **Bootstrap** | Framework CSS para UI responsiva |
| **JavaScript / AJAX** | Llamadas a endpoints JSON sin recargar página |
| **DataTables / Select2** | Tablas interactivas y autocomplete |

---

## 🏗️ Tipo de Arquitectura

### N-Capas (Layered Architecture)

La aplicación sigue una arquitectura en **N capas estrictamente separadas**, con inyección de dependencias gestionada por el contenedor de DI nativo de ASP.NET Core.

```
┌─────────────────────────────────────────┐
│           Capa de Presentación          │  Controllers + Razor Views
│         (ASP.NET Core MVC)              │  AJAX JSON endpoints
├─────────────────────────────────────────┤
│           Capa de Servicios             │  Business Logic
│         (Services / IServices)          │  Validaciones de dominio
├─────────────────────────────────────────┤
│         Capa de Repositorios            │  Data Access
│    (Repositories / IRepositories)       │  Generic Repository Pattern
├─────────────────────────────────────────┤
│           Capa de Datos                 │  EF Core DbContext
│         (Data / GymDbContext)           │  SQL Server
├─────────────────────────────────────────┤
│           Capa de Modelos               │  Entidades EF Core (POCO)
│             (Models/)                   │  Scaffolded desde BD
└─────────────────────────────────────────┘
```

### Patrones Aplicados

| Patrón | Descripción |
|---|---|
| **Repository Pattern** | Abstracción del acceso a datos detrás de interfaces |
| **Generic Repository** | `GenericRepository<T>` con CRUD reutilizable para todas las entidades |
| **Dependency Injection** | Toda dependencia se inyecta vía constructor (IoC nativo de .NET) |
| **DTO Pattern** | ViewModels y DTOs separan las entidades del dominio de la capa de presentación |
| **Unit of Work (implícito)** | `SaveAsync()` actúa como commit del contexto EF Core |
| **Background Jobs** | Hangfire gestiona la ejecución de tareas asíncronas y programadas |

---

## 📁 Estructura de Carpetas

```
GymApp/
│
├── Controllers/                  # Capa de Presentación
│   ├── BaseController.cs         # Base con helpers de autenticación
│   ├── AuthController.cs         # Login / Logout (cookie auth)
│   ├── HomeController.cs         # Dashboard principal
│   ├── ClienteHomeController.cs  # Vista limitada para clientes
│   ├── AccesoController.cs       # Control de acceso QR
│   ├── MembresiasController.cs
│   ├── PagosController.cs
│   ├── PlanesController.cs
│   ├── ProductosController.cs
│   ├── ReportesController.cs     # Reportes + exportación Excel
│   ├── RolesController.cs
│   ├── TurnosController.cs
│   ├── UsuariosController.cs
│   ├── VentasController.cs
│   └── CongelamientosController.cs
│
├── Services/                     # Capa de Negocio
│   ├── I{Nombre}Service.cs       # Contratos de servicio
│   ├── {Nombre}Service.cs        # Implementaciones
│   └── VentasServ_.cs            # IVentaService + VentaService (juntos)
│
├── Repositories/                 # Capa de Acceso a Datos
│   ├── IGenericRepository.cs     # Contrato CRUD genérico
│   ├── GenericRepository.cs      # Implementación base reutilizable
│   ├── I{Nombre}Repository.cs    # Contratos específicos
│   ├── {Nombre}Repository.cs     # Implementaciones específicas
│   └── IReporteService.cs        # ⚠ Ubicada aquí por error histórico
│
├── Models/                       # Entidades EF Core (POCO)
│   ├── Usuario.cs
│   ├── Role.cs
│   ├── Membresia.cs
│   ├── Plane.cs
│   ├── Turno.cs
│   ├── Congelamiento.cs
│   ├── PagosMembresium.cs
│   ├── Producto.cs
│   ├── VentasCabecera.cs
│   ├── VentasDetalle.cs
│   ├── Asistencia.cs
│   ├── ConfiguracionAlerta.cs    # Configuración de webhooks y alertas
│   └── ErrorViewModel.cs
│
├── ViewModels/                   # DTOs de entrada/salida
│   ├── MembresiaCreateDTO.cs
│   ├── MembresiaEditDTO.cs
│   ├── MembresiaListDTO.cs
│   ├── PagoCreateDTO.cs
│   ├── PagoListDTO.cs
│   ├── DeudaInfoDTO.cs
│   ├── ReporteIngresosDTO.cs
│   ├── ReporteMembresiaDTO.cs
│   ├── DashboardUserStatsDTO.cs
│   ├── DashboardFinancialStatsDTO.cs
│   ├── UsuarioViewModel.cs
│   ├── RoleViewModel.cs
│   ├── TurnoDTO.cs
│   ├── VentaCreateDTO.cs
│   ├── CongelarMembresiaDTO.cs
│   └── PagedResult.cs            # Wrapper genérico de paginación
│
├── Data/
│   └── GymDbContext.cs           # DbContext EF Core con todos los DbSets
│
├── Views/                        # Razor Views (.cshtml)
│   ├── Auth/
│   ├── Home/
│   ├── Membresias/
│   ├── Pagos/
│   ├── Planes/
│   ├── Productos/
│   ├── Reportes/
│   ├── Roles/
│   ├── Turnos/
│   ├── Usuarios/
│   ├── Ventas/
│   └── Shared/                   # _Layout.cshtml, _ValidationScripts
│
├── Migrations/                   # EF Core Migrations (historial del esquema)
│
├── wwwroot/                      # Archivos estáticos
│   ├── css/
│   ├── js/
│   └── uploads/fotos/            # Fotos de perfil de usuarios
│
├── docker/
│   └── Dockerfile                # Imagen Docker de la app
│
├── docker-compose.yml            # Orquestación: gymapp_web + gym_sql_server
├── appsettings.json              # Configuración de producción
├── appsettings.Development.json  # Configuración de desarrollo
└── Program.cs                    # Entry point: DI, middleware, rutas
```

---

## 🐳 Configuración Docker

```yaml
# Servicios definidos en docker-compose.yml
gymapp_web:
  - Puerto expuesto: 5000 → 8080 (interno)
  - Imagen: construida desde docker/Dockerfile
  - Volumen: ./gym_data/fotos_usuarios → /app/wwwroot/uploads/fotos

gym_sql_server:
  - Imagen: mcr.microsoft.com/mssql/server:2022-latest
  - Puerto: 1433
  - Volumen: sql_data (persistente)
  - Edition: Developer
```

---

## 🔐 Sistema de Autenticación

- **Tipo:** Cookie Authentication (ASP.NET Core)
- **Login:** por DNI o NombreUsuario
- **Hashing:** BCrypt con salt automático
- **Roles:** `Admin`, `Empleado`, `Cliente`
- **Autorización:** por atributo `[Authorize(Roles="...")]` en controladores
- **BaseController:** provee `CurrentUserId`, `CurrentUserName`, `CurrentUserRole` a todos los controladores heredados

---

## 🔔 Sistema de Notificaciones e Integraciones

GymApp implementa un sistema robusto de notificaciones basado en eventos y tareas programadas, centralizado en `WebhookService`.

### ⚡ Notificaciones Instantáneas (Event-Driven)
Se disparan inmediatamente cuando ocurre una acción relevante en el sistema:
- **Nuevo Usuario:** Notifica el registro de un nuevo socio.
- **Nuevo Pago:** Informa sobre la recepción de dinero (monto, cliente, método).
- **Nueva Membresía:** Alerta sobre ventas de planes.

### 📅 Tareas Programadas (Hangfire)
Orquestadas mediante jobs recurrentes:
1. **AlertaVencimientoJob:** Se ejecuta diariamente para identificar membresías que vencen en los próximos 7 días o que vencieron recientemente, enviando un listado al administrador.
2. **NotificacionProgramadaJob:** Envía reportes de desempeño (Daily/Weekly) con estadísticas clave: ingresos por categoría, nuevos miembros, y lista de deudores.

### 🔗 Integración con n8n
La aplicación actúa como productor de datos para **n8n**. Los webhooks envían payloads JSON que n8n procesa para:
- Enviar mensajes por **Telegram** o **WhatsApp**.
- Registrar logs en hojas de cálculo externas.
- Disparar flujos de retención de clientes.

> [!NOTE]
> `IReporteService.cs` está físicamente ubicada en la carpeta `Repositories/` en lugar de `Services/`, aunque su namespace es `GymApp.Services`. Esto es una inconsistencia menor que no afecta el funcionamiento.
