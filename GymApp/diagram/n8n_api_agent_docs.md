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

## 9. Deuda del Cliente (por ID)
Retorna la deuda total de un usuario específico.
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/deuda/{userId}`
- **Respuesta esperada:**
```json
{
    "deudaTotal": 100.00,
    "membresiasConDeuda": 1
}
```

## 10. Historial de Pagos de Usuario
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/pagos/usuario/{userId}`
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
- **Método:** `GET`
- **URL Completa:** `{{baseUrl}}/membresias/usuario/{userId}/historial`
- **Respuesta esperada:**
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

---

### Recomendación final para n8n
Al configurar el nodo de **HTTP Request** en n8n:
1. Para las URLs que terminan en `{userId}`: Usa las expresiones de n8n dentro del campo URL (ej: `http://localhost:5000/api/agent/pagos/deuda/{{$json.id}}`). **No** las envíes en Send Query Parameters.
2. Para las consultas que usan parámetros (ej: `?q=`, `?dias=`, `?fecha=`, `?inicio=`, `?fin=`): Activa la opción de **Send Query Parameters** y pon la clave/valor en esos recuadros respectivos. Deja la URL "limpia" (ej: `.../usuarios/buscar`).

---
### Aspectos Clave a tener en cuenta para n8n:

1. Asegúrate de agregar el Header `X-API-KEY` en todos los nodos HTTP con el valor de desarrollo.
2. Cuando la API utilice Query Parameters (`?q=`, `?dias=`, etc.), es recomendable que en n8n habilites la opción de "Send Query Parameters", coloques la Key y el Value en sus campos respectivos, y dejes la "URL" limpia.
3. Cuando la API requiere inyectar un ID dinámico (ejemplo: `/membresias/usuario/{userId}/activa`), inyéctalo directamente dentro de la URL del nodo en n8n mediante una expresión como `http://localhost:5000/api/agent/membresias/usuario/{{ $json.userId }}/activa`.

