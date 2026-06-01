# Plan de Actualización CRM - Módulo de Usuarios

Este documento sirve como archivo de seguimiento para la actualización masiva del módulo de Usuarios, integrando funcionalidades de CRM y trazabilidad, respetando la retrocompatibilidad del sistema existente.

## Fase 1: Capa de Datos (Modelos y EF Core)
- [x] **Modificar la entidad `Usuario`:**
  - [x] Añadir campos demográficos: `Origen` (string), `ApellidoPaterno` (string), `ApellidoMaterno` (string), `EstadoCivil` (string), `Genero` (string), `Direccion` (string), `WhatsApp` (string).
  - [x] Añadir campo `FechaNacimiento` (DateOnly).
  - [x] Añadir campos de la sección "Otros": `Ocupacion` (string), `Nota` (string largo), `PinAcceso` (string, 4-6 dígitos).
  - [x] Añadir campos de trazabilidad/auditoría: `FechaUltimaModificacion` (DateTime?), `ModificadoPorId` (string/int dependiendo del tipo de ID de usuario).
  - [x] **Retrocompatibilidad:** Asegurar que `NombreCompleto` permanezca intacto sin lógica de concatenación a nivel de base de datos para no romper Claims o reportes.
- [x] **Crear la entidad `RestriccionUsuario`:**
  - [x] Definir propiedades: `Id`, `UserId`, `TipoRestriccion`, `Descripcion`, `FechaAplicacion`, `UsuarioAplicadorId`, `EstadoActiva`.
- [x] **Configurar `GymDbContext`:**
  - [x] Configurar la relación 1:N entre `Usuario` y `RestriccionUsuario`.
  - [x] Configurar restricciones, longitudes máximas y comportamientos (Fluent API o Data Annotations).
- [x] **Migraciones EF Core:**
  - [x] Generar la migración (`AddUsuarioCrmFieldsAndRestricciones`).
  - [x] Actualizar la base de datos (`Update-Database` o `dotnet ef database update`).

## Fase 2: Capa de Repositorios y Servicios
- [x] **Actualización de DTOs:**
  - [x] Actualizar/Crear DTOs de creación y edición (`UsuarioCreateDTO`, `UsuarioEditDTO`) con los nuevos campos.
  - [x] Actualizar `UsuarioDetailsDTO` para incluir la lista de restricciones y los nuevos datos demográficos.
- [x] **Mapeo de Datos:**
  - [x] Configurar el mapeo correcto asegurando que `NombreCompleto` mantenga el funcionamiento original (asignando el nombre(s) a este campo).
- [x] **Lógica de Servicios (`UsuarioService`):**
  - [x] Implementar la generación y validación del `PinAcceso` (4-6 dígitos, automático o manual).
  - [x] Integrar lógica de auditoría: al editar un usuario, inyectar el `ModificadoPorId` y setear `FechaUltimaModificacion`.
- [x] **Nuevo Servicio de Restricciones (`IRestriccionService` / `RestriccionService`):**
  - [x] Crear métodos CRUD y de negocio (ej. `AplicarRestriccionAsync`, `LevantarRestriccionAsync`).
  - [x] Inyectar el nuevo servicio en la capa de DI (`Program.cs`).

## Fase 3: Capa de Controladores
- [x] **Modificar `UsuariosController`:**
  - [x] Ajustar acciones `Create` y `Edit` para aceptar y procesar los nuevos DTOs (mediante el ViewModel actualizado).
  - [x] Ajustar acción `Details` para cargar el usuario y sus restricciones activas/históricas usando el nuevo DTO CRM.
- [x] **Preparación de Datos para la Vista (ViewBags / SelectLists):**
  - [x] Enviar catálogos necesarios a la vista mediante ViewBags (`Genero`, `EstadoCivil`, `Origen`).
- [x] **Auditoría en Controladores:**
  - [x] Extraer el ID del usuario autenticado para inyectarlo en las acciones de guardado y aplicación de restricciones (`ModificadoPorId` y `UsuarioAplicadorId`).
- [x] **Endpoints para Restricciones:**
  - [x] Crear acciones en el controlador para añadir o revocar una restricción directamente desde el perfil del usuario.

## Fase 4: Capa de Presentación (UI)
### Fase 4.1: Rediseño de Formularios (Crear/Editar)
- [ ] **Estructura Base con Alpine.js:** Implementar sistema de pestañas (Tabs) en el modal de usuario.
- [ ] **Sección 1: Datos Personales:** Organizar Nombres, Apellidos, DNI y Foto/Webcam.
- [ ] **Sección 2: Demografía:** Implementar campos de WhatsApp, Dirección, Género, Estado Civil y Origen.
- [ ] **Sección 3: Acceso y Otros:** Gestión de PIN, Usuario/Password, Notas y Estado.
- [ ] **Integración AJAX:** Sincronizar el guardado y carga de datos con los nuevos campos del backend.

### Fase 4.2: Perfil del Socio (Vista de Detalles)
- [ ] **Layout CRM:** Diseño moderno de tarjetas para organizar la información demográfica y de contacto.
- [ ] **Panel de Trazabilidad:** Mostrar última modificación y usuario responsable.
- [ ] **Historial de Restricciones:** Tabla interactiva para visualizar limitaciones pasadas y presentes.
- [ ] **Gestión Activa:** Modales para aplicar nuevas restricciones y funcionalidad para levantarlas.

### Fase 4.3: Ajustes Globales y Validación
- [ ] **Integración en Listados:** Mejoras visuales en el Index para reflejar estados o accesos rápidos.
- [ ] **Validación Final:** Pruebas de flujo completo de usuario (Crear -> Editar -> Detalle -> Restringir).