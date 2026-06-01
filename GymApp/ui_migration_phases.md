# Fases de Migración UI (Tailwind CSS)

Este documento sirve como hoja de ruta y checklist para la migración de la interfaz de usuario de Bootstrap a Tailwind CSS, siguiendo las directrices de `design.md`.

## Fase 1: Instalación y Configuración

- [x] Inicializar npm en el proyecto (si no existe).
- [x] Instalar Tailwind CSS vía CLI (`npm install -D tailwindcss`).
- [x] Inicializar la configuración de Tailwind (`npx tailwindcss init`).
- [x] Configurar `tailwind.config.js` para escanear las vistas `.cshtml`.
- [x] Crear el archivo CSS de entrada (ej. `wwwroot/css/app.css` o `Styles/app.css`) con las directivas de Tailwind (`@tailwind base; @tailwind components; @tailwind utilities;`).
- [x] Configurar el script de compilación (watch) para Tailwind.
- [x] Incluir el CSS compilado en el `_Layout.cshtml`.

## Fase 2: Rediseño Estructural Base

- [x] Migrar el contenedor principal (`_Layout.cshtml`).
- [x] Rediseñar el Sidebar (menú lateral) con Tailwind (colores, hover, micro-animaciones).
- [x] Rediseñar el Topbar (barra superior de usuario y título).
- [x] Adaptar la lógica de colapso/expansión del sidebar para que funcione con las nuevas clases de Tailwind.
- [x] Asegurar que las notificaciones (TempData Success/Error) usen estilos de Tailwind.

## Fase 3: Actualización Módulo por Módulo (Vistas)

_Esta fase implica ir a las carpetas dentro de `Views/` y reemplazar las clases de Bootstrap por las de Tailwind, mejorando la estética según el `design.md`._

- [x] **Home / Dashboard** (`Views/Home/Index.cshtml`)
- [x] **Auth / Login** (`Views/Auth/Login.cshtml`)
- [x] **Usuarios / Clientes**
  - [x] Lista de Usuarios (`Index.cshtml`)
  - [x] Crear / Editar Usuario (Modales rediseñados)
  - [x] Detalles del Usuario (`Details.cshtml`)
- [x] **Membresías**
  - [x] Lista de Membresías (`Index.cshtml`)
  - [x] Formulario de Asignación/Edición (Modales y Renovación)
  - [x] Detalles de Membresía (`Details.cshtml`)
- [x] **Pases Diarios**
  - [x] Lista de Pases (`Index.cshtml`)
  - [x] Formulario de Pase (`Registrar.cshtml`)
  - [x] Detalles del Pase (`Details.cshtml`)
- [x] **Pagos / Cajas**
  - [x] Listado de Pagos (`Index.cshtml`)
  - [x] Registrar Pago (Modal de Cobro integrado)
  - [x] Detalles de Pago (`Detalles.cshtml`)
- [x] **Reportes**
  - [x] Ingresos Generales
  - [x] Membresías
- [x] **Ventas / Productos**
  - [x] Punto de Venta (POS) / Listado de Ventas
  - [x] Gestión de Productos
- [x] **Control de Acceso (Escáner QR)**
  - [x] Vista del Escáner
- [x] Administración y Configuración
  - [x] Planes y Precios (`Planes/Index.cshtml`)
  - [x] Turnos (Horarios) (`Turnos/Index.cshtml`)
  - [x] Roles (`Roles/Index.cshtml`)
  - [ ] **Gestión de Personal (Empleados):**
  - [x] Alertas (n8n)

## Fase 4: Detalles, Micro-Interacciones y Pulido

- [x] Mejorar los "Empty States" (implementados en tablas de Usuarios, Membresías y Pagos).
- [ ] Pulir transiciones en Modales y Dropdowns.
- [x] Mejorar la accesibilidad (focus rings en inputs y botones aplicados).

## Fase 5: Modernización de Iconografía (Lucide Icons)

_Sustitución de Bootstrap Icons por Lucide Icons para un aspecto "SaaS Premium", aplicando colores con propósito mediante clases de Tailwind._

- [x] **Infraestructura Base**
  - [x] Inyectar Lucide Icons en `_Layout.cshtml` (CDN y script de inicialización).
  - [ ] Configurar helper o script global para manejar el renderizado de iconos en modales dinámicos.
- [ ] **Migración por Módulos**
  - [x] **Layout & Sidebar:** Menú principal y perfil de usuario.
  - [x] **Auth:** Pantalla de Login (iconos de campos y alertas).
  - [x] **Home / Dashboard:** Iconos de tarjetas estadísticas y gráficos.
  - [x] **Usuarios / Socios:** Acciones de tabla, detalles y modales de creación.
  - [x] **Membresías:** Badges, historial y acciones de gestión.
  - [x] **Pagos / Cajas:** Iconos de métodos de pago, comprobantes y balances.
  - [x] **Pases Diarios:** Registro rápido y listados.
  - [x] **Configuración:** Planes (pricing), Turnos y Roles.
  - [x] **Reportes:** Iconos de exportación y visualización de datos.
  - [x] **Ventas / Productos:** POS e inventario.
  - [x] **Control de Acceso:** Escáner QR y registros de entrada.
  - [x] **Gestión de Personal (Empleados):** Registro de empleados y asigancion de permisos.
- [ ] **Cierre y Limpieza**
  - [ ] Auditoría visual de consistencia (tamaño y peso de iconos).
  - [ ] Eliminar dependencias de Bootstrap Icons en `_Layout.cshtml` y `Login.cshtml`.
