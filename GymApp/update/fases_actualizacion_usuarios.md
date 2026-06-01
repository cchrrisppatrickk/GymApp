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
- [ ] **Actualización de DTOs:**
  - [ ] Actualizar/Crear DTOs de creación y edición (`UsuarioCreateDTO`, `UsuarioEditDTO`) con los nuevos campos.
  - [ ] Actualizar `UsuarioDetailsDTO` para incluir la lista de restricciones y los nuevos datos demográficos.
- [ ] **Mapeo de Datos:**
  - [ ] Configurar el mapeo correcto asegurando que `NombreCompleto` mantenga el funcionamiento original (asignando el nombre(s) a este campo).
- [ ] **Lógica de Servicios (`UsuarioService`):**
  - [ ] Implementar la generación y validación del `PinAcceso` (4-6 dígitos, automático o manual).
  - [ ] Integrar lógica de auditoría: al editar un usuario, inyectar el `ModificadoPorId` y setear `FechaUltimaModificacion`.
- [ ] **Nuevo Servicio de Restricciones (`IRestriccionService` / `RestriccionService`):**
  - [ ] Crear métodos CRUD y de negocio (ej. `AplicarRestriccionAsync`, `LevantarRestriccionAsync`).
  - [ ] Inyectar el nuevo servicio en la capa de DI (`Program.cs`).

## Fase 3: Capa de Controladores
- [ ] **Modificar `UsuariosController`:**
  - [ ] Ajustar acciones `Create` y `Edit` para aceptar y procesar los nuevos DTOs.
  - [ ] Ajustar acción `Details` para cargar el usuario y sus restricciones activas/históricas.
- [ ] **Preparación de Datos para la Vista (ViewBags / SelectLists):**
  - [ ] Enviar catálogos necesarios a la vista mediante ViewBags o dentro del DTO (`Genero`, `EstadoCivil`, `Origen`).
- [ ] **Auditoría en Controladores:**
  - [ ] Extraer el ID del usuario autenticado (del `HttpContext.User`) para inyectarlo en las llamadas a los servicios (`ModificadoPorId` y `UsuarioAplicadorId`).
- [ ] **Endpoints para Restricciones:**
  - [ ] Crear acciones en el controlador para añadir o revocar una restricción directamente desde el perfil del usuario.

## Fase 4: Capa de Presentación (UI)
- [ ] **Rediseño de Vista `Create.cshtml` & `Edit.cshtml`:**
  - [ ] Reestructurar el formulario usando Tabs/Pestañas o Secciones colapsables (usando Tailwind CSS y Alpine.js).
  - [ ] Sección 1: **Datos Personales** (NombreCompleto, ApellidoPaterno, ApellidoMaterno, Documento, etc.).
  - [ ] Sección 2: **Demografía** (FechaNacimiento, Género, Estado Civil, Origen, Dirección, WhatsApp en lugar de Teléfono).
  - [ ] Sección 3: **Otros** (Ocupación, Nota, PIN de Acceso).
  - [ ] Para `Edit.cshtml`: Mostrar indicador de solo lectura con `FechaUltimaModificacion` y quién fue el último usuario en modificarlo.
- [ ] **Rediseño de Vista `Details.cshtml`:**
  - [ ] Adaptar el perfil para mostrar toda la nueva información demográfica de forma elegante y organizada.
  - [ ] Crear un panel/historial de **Restricciones**, mostrando estado (activa/inactiva), motivo y quién la aplicó.
  - [ ] Incluir un botón/modal para "Añadir Restricción".
- [ ] **Ajustes en Listados (Opcional):**
  - [ ] Actualizar `Index.cshtml` si se desea mostrar el ícono de WhatsApp, o un indicador visual si el usuario tiene una restricción activa.