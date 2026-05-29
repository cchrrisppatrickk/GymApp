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
- [ ] Migrar el contenedor principal (`_Layout.cshtml`).
- [ ] Rediseñar el Sidebar (menú lateral) con Tailwind (colores, hover, micro-animaciones).
- [ ] Rediseñar el Topbar (barra superior de usuario y título).
- [ ] Adaptar la lógica de colapso/expansión del sidebar para que funcione con las nuevas clases de Tailwind.
- [ ] Asegurar que las notificaciones (TempData Success/Error) usen estilos de Tailwind.

## Fase 3: Actualización Módulo por Módulo (Vistas)
*Esta fase implica ir a las carpetas dentro de `Views/` y reemplazar las clases de Bootstrap por las de Tailwind, mejorando la estética según el `design.md`.*

- [ ] **Home / Dashboard** (`Views/Home/Index.cshtml`)
- [ ] **Auth / Login** (`Views/Auth/Login.cshtml`)
- [ ] **Usuarios / Clientes**
  - [ ] Lista de Usuarios (`Index.cshtml`)
  - [ ] Crear / Editar Usuario
  - [ ] Detalles del Usuario
- [ ] **Membresías**
  - [ ] Lista de Membresías (`Index.cshtml`)
  - [ ] Formulario de Asignación/Edición
- [ ] **Pases Diarios**
  - [ ] Lista de Pases (`Index.cshtml`)
  - [ ] Formulario de Pase
- [ ] **Pagos / Cajas**
  - [ ] Listado de Pagos
  - [ ] Registrar Pago
- [ ] **Reportes**
  - [ ] Ingresos Generales
  - [ ] Membresías
- [ ] **Ventas / Productos**
  - [ ] Punto de Venta (POS) / Listado de Ventas
  - [ ] Gestión de Productos
- [ ] **Control de Acceso (Escáner QR)**
  - [ ] Vista del Escáner
  - [ ] Historial de Visitas
- [ ] **Administración y Configuración**
  - [ ] Planes y Precios
  - [ ] Turnos (Horarios)
  - [ ] Roles
  - [ ] Empleados (Gestión de Personal)
  - [ ] Alertas (n8n)
- [ ] **Portal del Cliente (ClienteHome)**
  - [ ] Perfil del Cliente / Dashboard de Usuario

## Fase 4: Detalles, Micro-Interacciones y Pulido
- [ ] Mejorar los "Empty States" (pantallas cuando no hay registros en una tabla).
- [ ] Pulir transiciones en Modales y Dropdowns.
- [ ] Mejorar la accesibilidad (focus rings en inputs y botones).
- [ ] Revisión general de contrastes y espaciados (respiración del diseño).
