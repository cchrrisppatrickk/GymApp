# Referencia de Eventos Webhook - GymApp a n8n

## Estructura Base (Envelope)
Todos los webhooks emitidos por la API hacia n8n emplean un patrón de envoltura estricto (Envelope Pattern). Esta estandarización permite a n8n realizar un enrutamiento basado en eventos de manera sencilla (utilizando un nodo Switch, por ejemplo) y extraer los datos dinámicos independientemente del origen.

La estructura JSON base siempre contendrá las siguientes tres propiedades:

- **`Evento`** (`string`): Nombre único que identifica qué sucedió (ej: `PING`, `NUEVO_USUARIO`).
- **`ChatId`** (`string`): El identificador de chat de Telegram destino (puede ser cadena vacía si es una alerta global para un canal por defecto).
- **`Datos`** (`object`): Payload en crudo que contiene la información sin formato para que n8n pueda transformarla y darle estilo a la notificación.

**Modelo de Referencia:**
```json
{
    "Evento": "STRING_IDENTIFICADOR",
    "ChatId": "STRING_DESTINO",
    "Datos": { 
        // Objeto dinámico dependiente del evento
    }
}
```

---

## Detalle por Evento

A continuación se detalla cada uno de los eventos del sistema, el archivo de origen que los emite y cómo luce su estructura `Datos` en la práctica.

### 1. PING
Evento para comprobar que la comunicación entre GymApp y n8n está operando de manera correcta.

- **Nombre del Evento**: `PING`
- **Archivo de Origen**: `Services/WebhookService.cs`
- **Ejemplo de JSON Emitido**:

```json
{
  "Evento": "PING",
  "ChatId": "-100987654321",
  "Datos": {
    "mensaje": "Prueba de conexión"
  }
}
```

### 2. NUEVO_USUARIO
Emitido cuando un nuevo cliente se registra o es creado en el sistema.

- **Nombre del Evento**: `NUEVO_USUARIO`
- **Archivo de Origen**: `Services/UsuarioService.cs`
- **Ejemplo de JSON Emitido**:

```json
{
  "Evento": "NUEVO_USUARIO",
  "ChatId": "",
  "Datos": {
    "NombreCompleto": "Juan Perez",
    "FechaRegistro": "2026-05-05T16:50:00Z"
  }
}
```

### 3. NUEVO_PAGO
Emitido instantáneamente en el momento en el que se registra un nuevo pago (total o parcial) de un cliente a su membresía.

- **Nombre del Evento**: `NUEVO_PAGO`
- **Archivo de Origen**: `Services/PagoService.cs`
- **Ejemplo de JSON Emitido**:

```json
{
  "Evento": "NUEVO_PAGO",
  "ChatId": "",
  "Datos": {
    "Monto": 50.00,
    "Cliente": "Maria Gomez",
    "MetodoPago": "Transferencia"
  }
}
```

### 4. RESUMEN_PROGRAMADO
Evento periódico emitido por las tareas en segundo plano del sistema. Agrupa reportes consolidados sobre nuevos registros, pagos del día, clientes en deuda y vencimientos.

- **Nombre del Evento**: `RESUMEN_PROGRAMADO`
- **Archivo de Origen**: `Services/NotificacionProgramadaJob.cs`
- **Ejemplo de JSON Emitido**:

```json
{
  "Evento": "RESUMEN_PROGRAMADO",
  "ChatId": "-100123456789",
  "Datos": {
    "NuevosMiembros": [
      {
        "Id": 152,
        "NombreCompleto": "Carlos Ruiz",
        "DNI": "12345678",
        "Telefono": "+51999888777",
        "FechaRegistro": "2026-05-05T10:00:00Z"
      }
    ],
    "PagosRecientes": [
      {
        "PagoId": 105,
        "NombreCliente": "Maria Gomez",
        "Monto": 50.00,
        "MetodoPago": "Efectivo",
        "FechaPago": "2026-05-05T09:30:00Z"
      }
    ],
    "Deudores": [
      {
        "MembresiaId": 34,
        "NombreCliente": "Lucia Fernandez",
        "DeudaPendiente": 20.00,
        "Estado": "Pendiente Pago"
      }
    ],
    "ProximosVencimientos": {
      "Mensaje": "Próximos vencimientos activado."
    }
  }
}
```
> **Nota**: Dentro de `RESUMEN_PROGRAMADO`, si una configuración (ej. "Enviar Nuevos Miembros") no estaba activada en el panel, su correspondiente nodo vendrá como `null` en los `Datos`.
