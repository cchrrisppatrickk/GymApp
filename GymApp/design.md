# Sistema de Diseño GymApp (Tailwind CSS)

## Propósito y Registro
- **Registro:** Producto (Aplicación de gestión y panel de administración).
- **Propósito:** Gestión administrativa de un gimnasio (suscripciones, pagos, accesos).
- **Escena:** Personal de recepción usando el sistema en un monitor a plena luz del día, o administradores revisando finanzas de noche. Debe sentirse rápido, sólido, claro y con un toque enérgico (deportivo) pero sin sacrificar la elegancia y limpieza de un producto premium SaaS.
- **Tono Estético:** *Refinado / Utilitario Deportivo*. Minimalista en su estructura, pero audaz en sus acentos y tipografía. Huimos de la estética de "plantilla genérica de Bootstrap" o los dashboards aburridos.

## Estrategia de Color
**Estrategia:** *Restrained* (Neutros tintados + Acento audaz deportivo).
No usaremos blanco puro `#fff` ni negro puro `#000`. 
- **Fondo Principal (App Background):** Un gris muy claro tintado ligeramente de azul/plata para dar sensación de frescura y limpieza (ej. `bg-slate-50` o `bg-zinc-50`).
- **Superficies (Cards, Sidebar):** Blanco puro `bg-white` para contraste sobre el fondo, con bordes muy sutiles `border-slate-200`.
- **Modo Oscuro (Opcional/Futuro):** Fondos `bg-slate-900`, superficies `bg-slate-800`.
- **Acento Primario (Brand/Action):** Un color enérgico que invite a la acción (ej. un naranja eléctrico o un índigo profundo). Optaremos por un **Índigo Eléctrico** (`indigo-600` / `#4f46e5`) combinado con toques de **Cian/Teal** o **Rosa Neón** para acentos secundarios o indicadores de estado deportivo (vitalidad).
- **Texto:** `text-slate-950` para títulos y valores principales, `text-slate-600` para texto secundario, y `text-slate-500` para etiquetas pequeñas.

## Tipografía
- **Fuentes (No usar Inter/Arial):**
  - **Display / Títulos:** *Outfit* o *Syne* (Tienen un carácter moderno, geométrico y ligeramente deportivo).
  - **Cuerpo / Datos:** *Plus Jakarta Sans* o *Manrope* (Excelente legibilidad para números, tablas y formularios).
- **Jerarquía:** Alto contraste entre tamaños y pesos. Títulos muy pesados (`font-extrabold`, `tracking-tight`), textos secundarios muy limpios (`text-sm`, `font-medium`).

## Layout y Espaciado (Composición)
- **Sidebar:** Diseño "flotante" o con bordes suaves, no un bloque sólido que corte la pantalla. Separación limpia del contenido.
- **Contenedores:** Adiós a las tarjetas repetitivas encajonadas. Usaremos espacios en blanco (negative space) generosos. 
- **Grid:** Asimetría controlada cuando sea posible. Los datos importantes (ej. ingresos) deben tener más peso visual que el resto.
- **Bordes y Sombras:** Bordes sutiles (`border border-slate-200`), sombras muy suaves pero extendidas para elevación (ej. `shadow-sm` o `shadow-lg` con baja opacidad en hovers). Bordes redondeados modernos (`rounded-2xl` o `rounded-3xl` para componentes principales, `rounded-xl` para botones).

## Interacciones y Movimiento (Motion)
- Transiciones CSS-only, rápidas pero perceptibles. 
- Curvas de easing exponenciales (`ease-out`). Nada de rebotes.
- Efectos de *hover* que sorprendan (ej. un ligero desplazamiento `translate-y-[-2px]`, cambios en la opacidad de la sombra, o revelación sutil de iconos).

## Anti-patrones Prohibidos (Leyes Implacables)
- ❌ **Glassmorphism decorativo:** No usaremos blurs innecesarios a menos que sea un modal o un overlay que lo justifique.
- ❌ **Dashboard Cliché:** Bloques cuadrados grises con un número grande sin contexto.
- ❌ **Texto con Gradiente + Fondo con Gradiente.**
- ❌ **Bordes laterales (Side-stripes):** Nada de un borde grueso a la izquierda para denotar estado. Usaremos etiquetas (badges) o colores de fondo tintados completos.

## Elementos UI Clave (Componentes Base Tailwind)
- **Botones:** Sólidos sin bordes gruesos. Efecto de focus ring visible (ej. `focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500`).
- **Tablas:** Limpias, sin líneas verticales. Relleno amplio (`p-4`), tipografía tabular para números. Encabezados sutiles en mayúsculas (`text-xs uppercase tracking-wider`).
- **Inputs:** Fondo gris ultra claro `bg-slate-50` al enfocar pasan a fondo blanco `bg-white` con un anillo `ring-1 ring-indigo-500`.

## Iconografía
- Reemplazar íconos genéricos por íconos de trazo fino (ej. *Lucide Icons* o mantener versiones específicas de *Bootstrap Icons* pero con estilo consistente). 
