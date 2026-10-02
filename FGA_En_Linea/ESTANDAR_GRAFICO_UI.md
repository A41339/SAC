# Estándar Gráfico y de Diseño UI - FGA en Línea

Este documento establece las directrices obligatorias de diseño, estructura visual y componentes gráficos para todas las vistas y módulos del sistema **FGA en Línea**.

---

## 1. Encabezado de Página Institucional (`.content-header`)

Todas las pantallas deben contar con un encabezado unificado renderizado directamente dentro de `<section class="content-header">`. 

### Propiedades Visuales (Definidas en `dashboard_modern.css`)
- **Fondo:** Blanco puro (`#ffffff !important`).
- **Borde inferior:** Azul institucional `2px solid #2F5597 !important`.
- **Sombra:** Suave de elevación `0 2px 8px rgba(47, 85, 151, 0.08) !important`.
- **Espaciado interior:** `padding: 20px 20px 14px 20px;`.
- **Espaciado exterior:** `margin-bottom: 20px;`.

### Estructura HTML Estándar
```html
<section class="content-header" style="padding: 20px 20px 14px 20px;">
    <div style="display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 12px;">
        <div>
            <h1 style="margin: 0; font-size: 21px; font-weight: 700; color: #143750; display: flex; align-items: center; gap: 10px;">
                <i class="fa fa-university" style="color: #0284c7;"></i>
                Título de la Pantalla
            </h1>
            <p style="margin: 6px 0 0 0; font-size: 13px; color: #64748b;">
                Descripción clara y concisa de la función de la pantalla o informe.
            </p>
        </div>
    </div>
    <ol class="breadcrumb" style="top: 18px;">
        <li><a href="@Url.Action("Index", "Home")"><i class="fa fa-dashboard"></i> Inicio</a></li>
        <li><a href="@Url.Action("Index", "Modulo", new { modulo = "..." })" class="breadcrumb-parent-pill"><i class="fa fa-file-text-o" style="margin-right: 4px;"></i> Módulo Padre</a></li>
        <li class="active">Página Actual</li>
    </ol>
</section>
```

---

## 2. Barra de Herramientas y Filtros (`.modern-filter-card`)

Dentro de `<section class="content">`, los controles de selección, filtros y botones de acción se agrupan en una tarjeta de filtro blanca:

```html
<div class="modern-filter-card" style="margin-bottom: 20px; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px 18px; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
    <div class="modern-filter-toolbar" style="display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 12px;">
        <!-- Grupo de Filtros Izquierda -->
        <div style="display: flex; align-items: center; gap: 14px; flex-wrap: wrap;">
            <!-- Selector de Segmentos / Pill Group -->
            <div class="pill-group">
                <a href="..." class="pill-item active">Opción A</a>
                <a href="..." class="pill-item">Opción B</a>
            </div>
        </div>

        <!-- Acciones Derecha -->
        <div style="display: flex; align-items: center; gap: 10px; margin-left: auto;">
            <!-- Badge de Estado o Cuadre -->
            <span class="badge badge-success-soft">
                <i class="fa fa-check-circle"></i> Estado Correcto
            </span>

            <!-- Botón Principal o Menú Desplegable -->
            <button class="btn btn-primary" style="background-color: #0284c7; border: none; font-weight: 600; padding: 7px 16px; border-radius: 6px;">
                <i class="fa fa-download"></i> Acción Principal
            </button>
        </div>
    </div>
</div>
```

---

## 3. Pestañas de Navegación Ejecutiva (`.balance-modern-tabs`)

- **Borde inferior del contenedor:** `1.5px solid #e2e8f0`.
- **Pestaña inactiva:** Texto `#475569`, peso `600`, tamaño `13.5px`, fondo transparente.
- **Pestaña activa:** Texto azul `#0284c7`, borde inferior `3px solid #0284c7`, sin fondo ni recuadros oscuros.

---

## 4. Estándar Gráfico para Visualizaciones (Highcharts)

Para evitar apariencias toscas, líneas negras gruesas o textos con sombras oscuras tipo silueta, se deben aplicar estrictamente las siguientes reglas:

### Reglas Clave:
1. **Sin contornos negros en textos (`textOutline: 'none'`):**
   - **PROHIBIDO:** `textOutline: '1px #334155'` o `textOutline: '1px contrast'`.
   - **OBLIGATORIO:** `textOutline: 'none'`. Los números y etiquetas deben ser limpios y legibles con pesos tipográficos (`fontWeight: '600'` o `'bold'`).
2. **Sin bordes negros en barras o columnas (`borderWidth: 0`):**
   - Todas las series de tipo `column`, `bar` o `pie` deben tener `borderWidth: 0`.
3. **Líneas conectoras sutiles en gráficos Waterfall:**
   - **PROHIBIDO:** Líneas conectoras negras por defecto (`#333333`).
   - **OBLIGATORIO:** `connectorColor: '#cbd5e1'`, `connectorWidth: 1.5`, `borderWidth: 0`.
4. **Líneas de cuadrícula y ejes suaves:**
   - `gridLineColor: '#f1f5f9'`
   - `lineColor: '#e2e8f0'`
   - `tickColor: '#e2e8f0'`

### Configuración Global Recomendada:
```javascript
Highcharts.setOptions({
    chart: {
        style: {
            fontFamily: '"Segoe UI", -apple-system, BlinkMacSystemFont, Roboto, "Helvetica Neue", sans-serif'
        }
    },
    plotOptions: {
        series: {
            borderWidth: 0,
            dataLabels: {
                style: {
                    textOutline: 'none'
                }
            }
        },
        column: {
            borderWidth: 0,
            borderRadius: 3
        },
        pie: {
            borderWidth: 0
        },
        waterfall: {
            borderWidth: 0,
            connectorColor: '#cbd5e1',
            connectorWidth: 1.5
        }
    },
    xAxis: {
        lineColor: '#e2e8f0',
        tickColor: '#e2e8f0',
        gridLineColor: '#f8fafc'
    },
    yAxis: {
        gridLineColor: '#f1f5f9',
        lineColor: '#e2e8f0'
    }
});
```

### Paleta Cromática Institucional:
- **Azul Primario (Activo / Títulos):** `#0284c7`
- **Azul Marino (Cabeceras / FFC):** `#143750` / `#2F5597`
- **Turquesa / Cartera:** `#168ea0`
- **Ámbar / Inversiones:** `#f59e0b` / `#f39c12`
- **Celeste / Liquidez:** `#5dade2`
- **Verde Positivo / Aumento:** `#10b981` / `#22c55e`
- **Naranja / Rojo Disminución:** `#f97316` / `#ef4444`
- **Gris Bordes Suaves:** `#e2e8f0`
- **Gris Fondos:** `#f8fafc` / `#f1f5f9`
