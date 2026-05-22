# Resumen de Seguridad: Doble Validación de Permisos

Este documento describe el estado actual de la doble validación de permisos en los controladores de la aplicación GymApp.
La doble validación consiste en proteger los métodos POST/PUT/DELETE que modifican datos a través de dos capas:
1. Atributo `[Authorize(Policy = "...")]` a nivel del método o clase.
2. Verificación interna `if (!TienePermiso("..."))` para devolver respuestas JSON manejables (ej. AJAX).

## Controladores CON Doble Validación Implementada
Estos controladores manejan la lógica de negocio crítica y ya cuentan con la protección para evitar llamadas directas no autorizadas a sus endpoints de escritura.

- **`UsuariosController`**: Creación y edición de usuarios.
- **`MembresiasController`**: Creación, renovación, edición, eliminación y congelamientos de membresías.
- **`PagosController`**: Registro, edición y anulación de pagos.
- **`VentasController`**: Registro de ventas (POS).
- **`ProductosController`**: Creación, edición y eliminación de productos.
- **`PlanesController`**: Creación, edición y eliminación de planes.

## Controladores que AÚN NO tienen la Doble Validación (Pero es Recomendable)
Estos controladores tienen acciones que modifican el estado en la base de datos. Se recomienda añadir la doble validación en una futura iteración.

- **`PasesDiariosController`**: Registro y edición de pases diarios.
- **`TurnosController`**: Administración de turnos disponibles.
- **`AccesoController`**: Registro de entradas y salidas de los clientes.
- **`CongelamientosController`**: Si maneja lógica propia fuera de MembresiasController para crear/eliminar.

## Controladores que NO REQUIEREN Doble Validación
Estos controladores no la necesitan ya sea porque son de acceso público, acciones sobre el usuario logueado, de solo lectura, o están protegidos a nivel global.

- **`AuthController`**: Acciones de Inicio de sesión (Login) y Cierre de sesión (Logout).
- **`ClienteHomeController`**: Portal exclusivo para el usuario autenticado (lectura de sus propios datos).
- **`HomeController`**: Pantalla inicial o endpoints básicos.
- **`ReportesController`**: Generación y visualización de reportes financieros y operativos (solo lectura GET).
- **`AlertasController`**: Visualización de clientes con deudas o membresías a punto de vencer (solo lectura GET).
- **`RolesController` / `AdministracionController`**: Protegidos típicamente a nivel superior (`[Authorize(Roles = "Admin")]`), lo cual deniega la acción desde el framework antes de llegar a la ejecución del método.
- **`ApiAgentController`**: Si es uso exclusivo por procesos del servidor o llaves de API controladas.

---
**Nota Técnica**: La principal ventaja de esta práctica es garantizar que el Frontend (Angular, React, Vue o Vanilla JS con fetch/jQuery) reciba un objeto JSON con estado de error controlado (`success: false`) en caso de denegación, evitando que el cliente intente procesar una página HTML 302 o un código 403 sin el cuerpo JSON esperado.
