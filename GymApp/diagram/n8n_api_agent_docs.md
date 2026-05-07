# Documentación: Integración de la API de GymApp en n8n

Esta documentación define la estructura correcta para cada endpoint de la API Agent, así como los parámetros y las respuestas esperadas. Es crucial seguir esta guía para evitar errores `404 Not Found` en **n8n**.

**Base URL:** `http://localhost:5000/api/agent`  
**Autenticación:** Requiere el Header `X-API-KEY` en cada llamada, con el valor `DEV-C4B6751C-709F-4A57-95E2-CB2B9A85CE7D`.

---

## 1. Verificar Conexión (Ping)
Se utiliza para comprobar que la API del Agente está activa y responde correctamente.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/ping`
- **Respuesta esperada:**
```json
{
    "status": "success",
    "message": "API del Agente conectada correctamente",
    "timestamp": "2026-05-02T18:00:35.1866372Z"
}
```

## 2. Estadísticas de Usuarios
Obtiene un resumen rápido del estado de los miembros del gimnasio.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/estadisticas/usuarios`
- **Respuesta esperada:**
```json
{
    "nuevosMiembrosMes": 0,
    "vencidosSinRenovar": 0,
    "porVencer7Dias": 0,
    "usuariosConDeuda": 12,
    "montoTotalDeuda": 394.00,
    "membresiasCongeladas": 0
}
```

## 3. Estadísticas Financieras
Obtiene datos de ingresos mensuales, semanales y comparativas de crecimiento.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/estadisticas/financieras`
- **Respuesta esperada:**
```json
{
    "mesesLabels": ["dic", "ene", "feb", "mar", "abr", "may"],
    "ingresosMensuales": [0, 3566.00, 2830.00, 3695.00, 0, 60.00],
    "semanasLabels": ["Sem 1", "Sem 2", "Sem 3", "Sem 4"],
    "ingresosSemanales": [0, 0, 0, 60.00],
    "ingresoMesActual": 60.00,
    "crecimientoMensualPorcentaje": 100,
    "ingresoSemanaActual": 60.00,
    "crecimientoSemanalPorcentaje": 100
}
```

## 4. Buscar Usuarios
Busca usuarios por nombre completo, DNI, o coincidencias.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/buscar?q={valor}`
- **Configuración en n8n:** 
  - Añadir en **Query Parameters**: Key `q`, Value `[ej: chris]`
- **Respuesta esperada:**
```json
[
    {
        "id": 21,
        "nombreCompleto": "CHRISTIAN ROMERO",
        "dni": "DNI_TEMP_021",
        "telefono": "917484257",
        "fechaRegistro": "2026-01-01T01:00:00"
    },
    {
        "id": 1005,
        "nombreCompleto": "Chris Patrick Chilon Segura",
        "dni": null,
        "telefono": null,
        "fechaRegistro": "2026-04-30T11:17:15.86"
    }
]
```

## 5. Usuarios Nuevos
Obtiene usuarios registrados en los últimos X días.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/nuevos?dias={numero}`
- **Configuración en n8n:** 
  - Añadir en **Query Parameters**: Key `dias`, Value `[ej: 7]`
- **Respuesta esperada:**
```json
[
    {
        "id": 1005,
        "nombreCompleto": "Chris Patrick Chilon Segura",
        "dni": null,
        "telefono": null,
        "fechaRegistro": "2026-04-30T11:17:15.86"
    }
]
```

## 6. Usuarios por Fecha Exacta
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/usuarios/fecha?fecha={YYYY-MM-DD}`
- **Configuración en n8n:**
  - Añadir en **Query Parameters**: Key `fecha`, Value `[ej: 2026-04-30]`
- **Respuesta esperada:**
```json
[
    {
        "id": 1005,
        "nombreCompleto": "Chris Patrick Chilon Segura",
        "dni": null,
        "telefono": null,
        "fechaRegistro": "2026-04-30T11:17:15.86"
    }
]
```

## 7. Pagos Recientes
Obtiene la lista de los últimos pagos registrados.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/recientes`
- **Respuesta esperada:**
```json
[
    {
        "id": 3002,
        "cliente": "Chris Patrick Chilon Segura",
        "monto": 60.00,
        "fecha": "2026-05-01T17:10:29",
        "metodoPago": "Efectivo"
    }
]
```

## 8. Pagos de Hoy
Resumen de los pagos realizados en la fecha actual.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/hoy`
- **Respuesta esperada:**
```json
{
    "fecha": "02/05/2026",
    "totalMonto": 40.00,
    "cantidad": 1,
    "pagos": [
        {
            "id": 4002,
            "cliente": "Chris Patrick Chilon Segura",
            "monto": 40.00,
            "fecha": "2026-05-02T13:19:12",
            "metodoPago": "Efectivo"
        }
    ]
}
```

## 9. Deuda del Cliente (Flexible)
Retorna la deuda total de un usuario específico. Soporta ID directo o búsqueda por nombre/DNI.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/deuda?userId={id}&q={valor}`
- **Parámetros (Query):**
  - `userId`: (Opcional) ID numérico del usuario.
  - `q`: (Opcional) Nombre o DNI del usuario.
```json
{
    "deudaTotal": 100.00,
    "membresiasConDeuda": 1
}
```

## 10. Historial de Pagos de Usuario
Retorna la lista de pagos realizados por el usuario. Soporta ID directo o búsqueda por nombre/DNI.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/usuario?userId={id}&q={valor}`
- **Parámetros (Query):**
  - `userId`: (Opcional) ID numérico del usuario.
  - `q`: (Opcional) Nombre o DNI del usuario.
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

## 11. Pagos por Rango de Fechas
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/rango?inicio={fecha}&fin={fecha}`
- **Configuración en n8n:**
  - Query Parameters: `inicio=2026-04-01`, `fin=2026-05-02`
- **Respuesta esperada:**
```json
[
    {
        "id": 3002,
        "monto": 60.00,
        "fecha": "2026-05-01T17:10:29.203",
        "metodoPago": "Efectivo",
        "nombreCliente": "Chris Patrick Chilon Segura"
    }
]
```

## 12. Lista de Deudores
Obtiene una lista detallada de todos los clientes con pagos pendientes.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/deudores`
- **Tip para n8n:** Se puede verificar la longitud del Array recibido para mostrar la cantidad total de deudores antes de listar el detalle.
- **Respuesta esperada:**
```json
[
    {
        "membresiaId": 174,
        "nombreCliente": "MAXIMILIANO OSCAR MIQUEO",
        "dniCliente": "DNI_TEMP_110",
        "nombrePlan": "AÑO",
        "estado": "Pendiente Pago",
        "precioTotal": 660.00,
        "totalPagado": 560.00,
        "deudaPendiente": 100.00
    }
]
```

## 13. Historial de Membresías de Usuario
Retorna el historial completo de membresías. Soporta ID directo o búsqueda por nombre/DNI.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/usuario/historial?userId={id}&q={valor}`
- **Parámetros (Query):**
  - `userId`: (Opcional) ID numérico del usuario.
  - `q`: (Opcional) Nombre o DNI del usuario.
```json
[
    {
        "id": 173,
        "nombrePlan": "1 MES - SOLO",
        "estado": "Vencida",
        "fechaInicio": "2026-03-31T00:00:00",
        "fechaFin": "2026-04-30T00:00:00",
        "diasRestantes": -2,
        "comentarios": null,
        "deudaPendiente": 0.00
    }
]
```

## 14. Alertas de Membresías (Próximos Vencimientos)
Devuelve membresías que vencen en los próximos X días.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/alertas?dias={numero}`
- **Respuesta esperada:**
```json
[
    {
        "id": 132,
        "nombrePlan": "1 MES - SOLO",
        "estado": "Vencida",
        "fechaInicio": "2026-03-18T00:00:00",
        "fechaFin": "2026-04-17T00:00:00",
        "diasRestantes": -15,
        "comentarios": null,
        "deudaPendiente": 0.00
    }
]
```

## 15. Manejo de Ambigüedad (IMPORTANTE)
Cuando se usa el parámetro `q` para buscar por nombre, si el sistema detecta múltiples usuarios que coinciden, retornará un `400 Bad Request` con sugerencias.

- **Respuesta de Ambigüedad:**
```json
{
  "error": "Ambigüedad detectada",
  "mensaje": "Se encontraron 2 usuarios para 'Juan'...",
  "sugerencias": [
    { "id": 10, "nombreCompleto": "Juan Pérez", "dni": "12345678" },
    { "id": 15, "nombreCompleto": "Juan García", "dni": "87654321" }
  ]
}
```
**Tip n8n:** Si recibes este error, el agente puede mostrar los nombres y DNIs al usuario final para que elija el correcto o proporcione el DNI exacto.

---

### Recomendación final para n8n
Al configurar el nodo de **HTTP Request** en n8n:
1. **Búsqueda Flexible:** Para endpoints como `deuda`, `usuario` o `historial`, usa la opción **Send Query Parameters**. Puedes enviar `userId` (si lo tienes) o `q` (nombre o DNI).
2. **URLs Dinámicas:** Las URLs ya no requieren inyectar el ID directamente en el path (ej: `/pagos/deuda/123`). Ahora se recomienda usar parámetros: `.../pagos/deuda?userId=123`.
3. **Manejo de Errores:** Configura el nodo para que no falle inmediatamente ante un error 400, de modo que puedas procesar la respuesta de "Ambigüedad detectada" y guiar al usuario.

---
### Aspectos Clave a tener en cuenta para n8n:

1. Asegúrate de agregar el Header `X-API-KEY` en todos los nodos HTTP con el valor de desarrollo.
2. Utiliza siempre la sección de **Query Parameters** en n8n para enviar `q`, `userId`, `dias`, etc. Esto evita errores de formato en la URL.
3. Si el sistema devuelve "Ambigüedad detectada", el campo `sugerencias` contiene la lista de usuarios que debes mostrar para desambiguar.

