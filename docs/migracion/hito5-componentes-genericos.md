# Hito 5 — Componentes genéricos de listado (EsbaListView + EsbaFilterPanel)

**Estado:** ✅ 2026-06-15
**Etapas cubiertas:** 1 (query server-side), 3 (componentes Blazor), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §3.3 (componentes genéricos),
§3.2 (paginación server-side), §1.2 (capas), §3.5 (mapa de dependencias).

## 1. Objetivo

Construir los sucesores de `Modulo Variable/modulovariable.pas` y `parametros.pas`:
un componente genérico de listado y un panel de filtros declarativo, con
exportación a Excel/PDF, validados contra un listado real de Académica
(**Listado de Materias**). Desbloquean en cadena todas las pantallas "Listado de…".

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `modulovariable.pas` (listado genérico + Excel + imprimir) | `Components/Shared/EsbaListView.razor` | `MudDataGrid` server-side + toolbar Excel/PDF. Columnas vía `EsbaColumn<T>` (única fuente de verdad para grilla y export). |
| `parametros.pas` (array `Param`, filtros en runtime) | `Components/Shared/EsbaFilterPanel.razor` + `EsbaFilterField` | Tipos de `Param` → `EsbaFilterKind`: C→Texto, N→Numero, D→Fecha, L/K→Seleccion, X→MultiSeleccion; flags 'S'/'N' → Booleano. Combos cargan por servicio (`OpcionesAsync`), nunca por SQL embebido. |
| `Param.Obligatorio='S'` + `MessageDlg('Parametro Obligatorio')` | `EsbaFilterField.Obligatorio` + marca de error en el campo | Se valida antes de buscar; sin `MessageDlg` final. |
| `&CUA_ACT` / `&CAR_ACT` (valores por defecto) | `EsbaFilterField.ValorInicial` | El default lo provee la página (no hay "carrera activa" global; §3.2). |
| `FuncionesExcel.pas` (automación OLE) | `Infrastructure/Excel/ClosedXmlExportService.cs` | ClosedXML; no requiere Excel instalado. |
| `Imprimir.pas` / Gnostice (preview + PDF) | `Infrastructure/Reports/QuestPdfExportService.cs` | QuestPDF tabla apaisada; el navegador es el visor (descarga inline). |
| Listado de materias de `altamodifmaterias.pas` | `Queries/MateriasQuery.BuscarAsync` + `Pages/Academica/ListadoMaterias.razor` | Server-side: paginación `OFFSET/FETCH`, orden por whitelist, filtros parametrizados. |

## 3. Contrato de los componentes

### EsbaListView&lt;T&gt;
- `Columnas: IReadOnlyList<EsbaColumn<T>>` — título, `Valor` (export + celda por
  defecto), `Celda` (render custom opcional), `ClaveOrden` (orden server-side),
  `OcultarEnChico` (responsive), `Exportable`.
- `DataProvider: Func<EsbaListRequest, CancellationToken, Task<PagedResult<T>>>` —
  la página la conecta a su query Dapper.
- Slots: `Filtros`, `AccionPrimaria`, `AccionesFila`, `OnFilaClick`.
- Export: descarga vía JS (`wwwroot/js/esba-descargas.js`), cap de
  `MaxFilasExport = 50.000` filas con aviso por Snackbar si se trunca.

### EsbaFilterPanel
- `Campos: IReadOnlyList<EsbaFilterField>` + `OnBuscar: EventCallback<EsbaFilterValores>`.
- `EsbaFilterValores` expone getters tipados por clave (`Texto`, `Numero`,
  `Fecha`, `Booleano`, `MultiSeleccion`).

## 4. Decisiones / desviaciones

- **Orden server-side**: el `SortDefinition` de MudDataGrid se mapea a
  `EsbaColumn.ClaveOrden` por título de columna; si no matchea, cae al orden por
  defecto de la query sin romper. La query valida el campo contra una whitelist
  (anti-inyección en el `ORDER BY`).
- **Dos interfaces de export** (`IExcelExportService`, `IPdfExportService`) en vez
  del `IReportePdfService` de §1.2: este es volcado tabular de un listado; los
  reportes con formato propio (hito 9) serán un servicio aparte.
- **CA1716** desactivado solo para `Components/Shared/**.cs` (el nombre `Shared`
  es normativo, §1.2; no hay consumidores VB). Documentado en `.editorconfig`.
- **Foto del alumno**: la deuda etiquetada a hito 5 se re-asignó a hito 12
  (binarios por endpoint autenticado, §3.4 ⚪); no era parte de los componentes.

## 5. Verificación

- `dotnet build Esba.slnx` → 0 warnings, 0 errors (TreatWarningsAsErrors).
- `dotnet test` → 97 verdes (14 Domain + 40 Application + 43 Integration).
  - 3 de `ExportServicesTests` (Excel: encabezados/tipos/Sí-No/fecha; PDF: firma `%PDF`).
  - 8 de `MateriasQueryTests` (Etapa 4.B) contra Firebird real
    (`/pool/firebird/esba.gdb`): equivalencia con el listado legacy por carrera,
    paginación estable/disjunta, orden por whitelist (incl. campo inválido que no
    rompe — anti-inyección) y filtros (anuales, texto, cuatrimestre).

## 6. Checklist de aceptación (3.B)

- [x] La página usa `EsbaListView`/`EsbaFilterPanel`.
- [x] Paginación, orden y filtros se resuelven en el servidor (Firebird).
- [x] Los combos de filtros cargan por servicio inyectado (`ICarrerasQuery`).
