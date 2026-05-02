# Documentación: Integración de la API de GymApp en n8n

Esta documentación define la estructura correcta para cada endpoint de la API Agent, así como los parámetros y las respuestas esperadas. Es crucial seguir esta guía para evitar errores `404 Not Found` en **n8n**.

**Base URL:** `http://localhost:5000/api/agent`  
**Autenticación:** Requiere el Header `X-API-KEY` en cada llamada, con el valor `DEV-C4B6751C-709F-4A57-95E2-CB2B9A85CE7D`.

---

## 1. Buscar Usuarios
Se utiliza para buscar usuarios por nombre completo, DNI, o coincidencias.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/buscar?q={valor}`
- **Configuración en n8n:** 
  - URL estática: `http://localhost:5000/api/agent/usuarios/buscar`
  - Añadir en **Query Parameters**: Key `q`, Value `[El término de búsqueda, ej: JOSE SEMINARIO]`
- **Respuesta esperada:** Un Array (lista) con los usuarios encontrados.
```json
[
  {
    "id": 158,
    "nombreCompleto": "JOSE SEMINARIO",
    "dni": "DNI_TEMP_158",
    "telefono": "930289206",
    "fechaRegistro": "2026-01-01T01:00:00"
  }
]
```

## 2. Usuarios Nuevos
Obtiene usuarios registrados en los últimos X días.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/nuevos?dias={numero}`
- **Configuración en n8n:** 
  - URL estática: `http://localhost:5000/api/agent/usuarios/nuevos`
  - Añadir en **Query Parameters**: Key `dias`, Value `[número, ej: 7]`
- **Respuesta esperada:** Lista de usuarios registrados recientemente.

## 3. Usuarios por Fecha Exacta
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/fecha?fecha={YYYY-MM-DD}`
- **Configuración en n8n:**
  - URL estática: `http://localhost:5000/api/agent/usuarios/fecha`
  - Añadir en **Query Parameters**: Key `fecha`, Value `[Fecha ISO, ej: 2025-04-24]`

## 4. Membresía Activa de Usuario
Retorna la membresía activa actual para un usuario determinado por su ID.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/usuario/{userId}/activa`
- **Configuración en n8n:**
  - URL dinámica: `http://localhost:5000/api/agent/membresias/usuario/{{ $json.userId }}/activa` (o el nodo/variable que contenga el `userId` en n8n). No usa Query Parameters.
- **Respuesta esperada:** Un solo objeto con los detalles de la membresía, si existe (de lo contrario retorna un error en un campo `error`).
```json
{
  "id": 173,
  "nombrePlan": "1 MES - SOLO",
  "estado": "Activa",
  "fechaInicio": "2026-03-31T00:00:00",
  "fechaFin": "2026-04-30T00:00:00",
  "diasRestantes": 0,
  "deudaPendiente": 0.00
}
```

## 5. Historial de Membresías de Usuario
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/usuario/{userId}/historial`
- **Configuración en n8n:** Igual que el anterior, URL dinámica inyectando el `{userId}` directamente en la ruta de la URL.
- **Respuesta esperada:** Una lista de objetos de membresía representando el historial del cliente.

## 6. Alertas de Membresías (Vencimientos)
Devuelve membresías que vencen en los próximos X días.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/alertas?dias={numero}`
- **Configuración en n8n:** 
  - URL estática: `http://localhost:5000/api/agent/membresias/alertas`
  - Añadir en **Query Parameters**: Key `dias`, Value `[número, ej: 7]`
- **Respuesta esperada:** Array con las membresías a punto de vencer, con datos de contacto.

## 7. Deuda del Cliente
Retorna la deuda financiera de un usuario consolidando sus pagos y planes.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/deuda/{userId}`
- **Configuración en n8n:**
  - URL dinámica inyectando el `{userId}` en la ruta: `http://localhost:5000/api/agent/pagos/deuda/158`
- **Respuesta esperada:** Un resumen financiero.
```json
{
  "deudaTotal": 0,
  "membresiasConDeuda": 0
}
```

## 8. Historial de Pagos de Usuario
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/usuario/{userId}`
- **Configuración en n8n:** URL dinámica inyectando el `{userId}` en la ruta.
- **Respuesta esperada:**
```json
[
  {
    "id": 179,
    "monto": 60.00,
    "fecha": "2026-03-31T11:29:00",
    "metodoPago": "Efectivo",
    "nombreCliente": "JOSE SEMINARIO"
  }
]
```

## 9. Pagos por Rango de Fechas
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/rango?inicio={fecha}&fin={fecha}`
- **Configuración en n8n:**
  - URL estática: `http://localhost:5000/api/agent/pagos/rango`
  - Añadir en **Query Parameters**:
    - Key `inicio`, Value `[fecha inicial, ej: 2025-01-01]`
    - Key `fin`, Value `[fecha final, ej: 2025-12-31]`
- **Respuesta esperada:** Lista de pagos registrados en el intervalo especificado.

---
### Recomendación final para n8n
Al configurar el nodo de **HTTP Request** en n8n:
1. Para las URLs que terminan en `{userId}`: Usa las expresiones de n8n dentro del campo URL (ej: `http://localhost:5000/api/agent/pagos/deuda/{{$json.id}}`). **No** las envíes en Send Query Parameters.
2. Para las consultas que usan parámetros (ej: `?q=`, `?dias=`, `?fecha=`, `?inicio=`, `?fin=`): Activa la opción de **Send Query Parameters** y pon la clave/valor en esos recuadros respectivos. Deja la URL "limpia" (ej: `.../usuarios/buscar`).

---
### Aspectos Clave a tener en cuenta para n8n resaltados en la documentación:

Asegúrate de agregar el Header X-API-KEY en todos los nodos HTTP con el valor de desarrollo.
Cuando la API utilice Query Parameters (?q=, ?dias=, etc.), es recomendable que en n8n habilites la opción de "Send Query Parameters", coloques la Key y el Value en sus campos respectivos, y dejes la "URL" limpia.
Cuando la API requiere inyectar un ID dinámico (ejemplo: /membresias/usuario/{userId}/activa), inyéctalo directamente dentro de la URL del nodo en n8n mediante una expresión como http://localhost:5000/api/agent/membresias/usuario/{{ $json.userId }}/activa.
