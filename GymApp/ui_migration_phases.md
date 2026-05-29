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
*Esta fase implica ir a las carpetas dentro de `Views/` y reemplazar las clases de Bootstrap por las de Tailwind, mejorando la estética según el `design.md`.*

- [x] **Home / Dashboard** (`Views/Home/Index.cshtml`)
- [ ] **Auth / Login** (`Views/Auth/Login.cshtml`)
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
- [ ] **Reportes**
  - [ ] Ingresos Generales
  - [ ] Membresías
- [ ] **Ventas / Productos**
  - [ ] Punto de Venta (POS) / Listado de Ventas
  - [ ] Gestión de Productos
- [ ] **Control de Acceso (Escáner QR)**
  - [ ] Vista del Escáner
  - [ ] Historial de Visitas
- [x] **Administración y Configuración**
  - [x] Planes y Precios (`Planes/Index.cshtml`)
  - [ ] Turnos (Horarios)
  - [ ] Roles
  - [ ] Empleados (Gestión de Personal)
  - [ ] Alertas (n8n)
- [ ] **Portal del Cliente (ClienteHome)**
  - [ ] Perfil del Cliente / Dashboard de Usuario

## Fase 4: Detalles, Micro-Interacciones y Pulido
- [x] Mejorar los "Empty States" (implementados en tablas de Usuarios, Membresías y Pagos).
- [ ] Pulir transiciones en Modales y Dropdowns.
- [x] Mejorar la accesibilidad (focus rings en inputs y botones aplicados).
- [x] Revisión general de contrastes y espaciados (tipografía oscurecida para legibilidad).
