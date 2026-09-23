# Homologar Mensaje de Éxito para "Categorías – FFC"

## Goal Description

Aplicar el mismo estándar de notificaciones de éxito que se usa en los mantenimientos de **Entidades** (toast moderno) a la vista y los controladores de **Categorías** (página `Evaluacion/Index.cshtml` y acciones relacionadas).  De esta forma, todas las operaciones de crear, editar, eliminar o asignar sub‑categorías mostrarán un toast uniforme.

## User Review Required

[!IMPORTANT] **Impacto visual**: Cambiar los mensajes de éxito afecta la UI de la sección *Categorías*. Verifique que el estilo del toast (gradiente verde y fade‑in) coincide con el de Entidades.

[!WARNING] **Compatibilidad**: Los controladores deben seguir devolviendo `JsonResult` con `{ success = true, message = "..." }`.  Si alguna acción actualmente devuelve solo una vista, requerirá ajustes.

## Open Questions

- ¿Desea que el toast incluya también un **título** opcional (como en Entidades) o solo el mensaje?
- ¿Hay alguna acción adicional en *Categorías* (por ejemplo, carga masiva) que necesite el toast y que no hayamos identificado?
- ¿Se debe traducir el mensaje a otros idiomas mediante recursos `.resx` o mantener el texto literal en español?

## Proposed Changes

---
### 1. Re‑usar el helper de toast existente

#### [MODIFY] [Shared/_Layout.cshtml](file:///c:/Users/jcastro/Desktop/Solution/FGA_En_Linea/Views/Shared/_Layout.cshtml)
- Verificar que la función `window.mostrarToastExito` ya está definida (está en `_Layout.cshtml`). No se requieren cambios si ya existe.

---
### 2. Actualizar la vista de Categorías

#### [MODIFY] [Views/Evaluacion/Index.cshtml](file:///c:/Users/jcastro/Desktop/Solution/FGA_En_Linea/Views/Evaluacion/Index.cshtml)
- Añadir la lógica de toast al final del archivo (similar a **Entidad/Index.cshtml**):
```javascript
if (!window.mostrarToastExito) {
    window.mostrarToastExito = function (mensaje, titulo) {
        // Usa la misma implementación que en Entidad
        $.sticky('<br/>' + mensaje, { stickyClass: 'success' }); // fallback
    };
}
```
- En cada llamada AJAX que procesa crear/editar/eliminar categorías, reemplazar los `alert-success` actuales (si existen) por:
```javascript
window.mostrarToastExito(res.message);
```
- Eliminar o comentar código antiguo que muestra `<div class="alert alert-success">` directamente.

---
### 3. Ajustar los controladores de Categorías

#### [MODIFY] [Controllers/EvaluacionController.cs](file:///c:/Users/jcastro/Desktop/Solution/FGA_En_Linea/Controllers/EvaluacionController.cs)
- Cambiar retornos de `return Json(new { success = true, message = "..." });` donde falten.
- Asegurarse de que los mensajes coincidan con los que se mostrarán en el toast.

---
### 4. Refactorizar scripts comunes (opcional)

#### [NEW] [Scripts/site-toasts.js](file:///c:/Users/jcastro/Desktop/Solution/FGA_En_Linea/Scripts/site-toasts.js)
- Centralizar la definición de `mostrarToastExito` para evitar duplicación.
- Incluir este script en `_Layout.cshtml`.

---
### 5. Pruebas

#### Automated Tests
- Ejecutar `dotnet test` para asegurar que no se rompen los tests existentes.
- Añadir pruebas unitarias que llamen a un controlador de Categorías y verifiquen que el `JsonResult` contiene `success = true` y el mensaje esperado.

#### Manual Verification
- Navegar a la página *Categorías* y probar crear, editar y eliminar una categoría.
- Confirmar que aparece el toast verde con el mensaje correcto y desaparece después de unos segundos.
- Verificar que la misma experiencia se mantiene al recargar la página (no aparecen alertas estáticas).

## Verification Plan

### Automated Tests
- `dotnet test` en la solución.
- Test de integración que verifica la respuesta JSON de `EvaluacionController`.

### Manual Verification
- Usuario abre *Categorías – FFC*, realiza operaciones CRUD y confirma el toast.
- Comparar visualmente con el toast de la página *Entidades* para asegurar consistencia.
