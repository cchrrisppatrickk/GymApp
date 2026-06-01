# Instrucciones del Proyecto: GymApp

Este archivo contiene las directrices fundamentales, estándares de arquitectura y reglas de negocio para el desarrollo de **GymApp**. Estas instrucciones son de cumplimiento obligatorio para garantizar la consistencia y seguridad del sistema.

## 1. Visión General del Proyecto
GymApp es un sistema integral de gestión para gimnasios que abarca desde el control de acceso físico (QR) hasta la administración financiera y de inventario (Quiosco).

**Stack Tecnológico:**
- **Backend:** ASP.NET Core 8.0 MVC.
- **Base de Datos:** SQL Server / Entity Framework Core.
- **Arquitectura:** Repository Pattern + Services + ViewModels (DTOs).
- **Seguridad:** Autenticación por Cookies + Autorización basada en Políticas (PBAC).
- **UI:** Tailwind CSS (en migración activa), Alpine.js / Vanilla JS para interactividad.
- **Automatización:** Hangfire para tareas programadas y Webhooks para notificaciones (n8n).

## 2. Estándares de Codificación

### Backend (C#)
- **Patrón Repositorio:** Toda interacción con la BD debe pasar por un `GenericRepository<T>` o un repositorio específico que herede de este. No usar el `DbContext` directamente en los controladores.
- **Servicios:** La lógica de negocio (validaciones, cálculos financieros, procesamiento de fotos) reside exclusivamente en la capa de `Services`.
- **ViewModels/DTOs:** No exponer entidades del modelo directamente en las vistas o APIs. Usar proyecciones específicas para cada necesidad (ej. `UsuarioViewModel`, `PagoCreateDTO`).
- **Inyección de Dependencias:** Registrar siempre los nuevos servicios y repositorios en `Program.cs`.

### Frontend (Tailwind CSS)
- **Prioridad:** Estamos eliminando Bootstrap. Todos los nuevos componentes o refactorizaciones deben usar **Tailwind CSS**.
- **Aesthetic:** Seguir el diseño detallado en `diagram/Diseno_UI/design.md`. Preferir un look moderno, oscuro/elegante con acentos de color según el estado (Éxito: verde, Error: rojo, Advertencia: amarillo).

## 3. Reglas de Negocio Críticas
1. **Acceso (QR):** Solo permitir el ingreso si el socio tiene una membresía con estado "Activa" y fecha de vencimiento mayor o igual a la actual.
2. **Finanzas:** 
   - El `Monto` de un pago nunca puede superar la deuda pendiente de la membresía.
   - Pagos con Yape/Plin **requieren obligatoriamente** la captura/subida de la imagen del comprobante.
   - Al anular un pago, el estado de la membresía debe sincronizarse automáticamente.
3. **Membresías:** Las renovaciones deben ser "inteligentes": si la membresía actual no ha vencido, la nueva debe comenzar el día posterior al vencimiento actual.
4. **Seguridad de Personal:** Un empleado no puede modificar sus propios permisos ni los de un Administrador.

## 4. Estructura de Documentación (`diagram/`)
Mantener la organización de carpetas para nuevos documentos:
- `Arquitectura/`: Resúmenes técnicos y diagramas.
- `Seguridad/`: Políticas y auditorías.
- `Gestion_Socios/`: Procesos relacionados con el cliente.
- `Comercial/`: Pagos, ventas e inventario.
- `Admin_Analitica/`: Reportes y Webhooks.
- `Diseno_UI/`: Guías de estilo y fases de migración.

## 5. Estándares de Git y Commits
- **Idioma:** Todos los mensajes de commit deben ser redactados en **español**.
- **Frecuencia:** Se debe crear un commit por cada cambio o sub-tarea completada para mantener un historial trazable.
- **Formato:** Se recomienda seguir el estándar de *Conventional Commits* (ej. `feat:`, `fix:`, `docs:`, `style:`).
- **Descripción:** El mensaje debe describir claramente *qué* se cambió y, si no es obvio, *por qué*.

## 6. Instrucciones para Gemini CLI
- **Surgical Edits:** Usar la herramienta `replace` para cambios precisos. Evitar reescribir archivos completos a menos que sea necesario.
- **Validación:** Tras cada cambio en el backend, intentar compilar y verificar que no se rompan las dependencias en `Program.cs`.
- **Análisis Previo:** Antes de crear un nuevo módulo, consultar `diagram/Arquitectura/modulos_resumen.md` para asegurar que encaje en la estructura actual.

---
*Ultima actualización: Mayo 2026*
