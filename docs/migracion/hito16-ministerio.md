# Hito 16 — Comisiones al Ministerio

**Estado:** ✅ 2026-09-29
**Etapas cubiertas:** 1 (queries Dapper sobre CURSADA/ALUMNOS/CARRERA/TUTORES), 2 (handler +
codificación en dominio), 3 (pantalla + endpoints PDF/Excel + menú), 4 (tests unitarios,
de servicios y de equivalencia contra el SQL legacy).
**Documentos rectores:** `migration_improvements.md` §1.3 (Dapper parametrizado), §2.1
(cálculos de dominio puros), §2.2, §2.3, §3.4.

## 1. Alcance

Hueco detectado 2026-09-29: el submenú **Ministerio** del legacy no figuraba en la hoja de
ruta. De sus tres opciones, solo "Comisiones al ministerio" tenía handler en el `.dfm`
(`Comisionesalmnisterio1Click`); "Profesores por comisión" y "Títulos y legalizaciones"
eran botones muertos y **no se migran**.

`ComisionesAlMinisterio.pas` (`TFrmComAlMinisterio`) tenía dos salidas independientes
sobre los mismos filtros (carrera del selector, cuatrimestre obligatorio, comisión
opcional):

- **Imprimir**: nómina de alumnos por comisión (una página por CUTUCO de COMARM) con
  cursantes y el bloque RECURSANTES al pie; opciones "Con Membrete", "Edad y
  Nacionalidad" y margen superior ajustable.
- **Excel**: el padrón que se envía al Ministerio, con dos layouts según
  `CARRERA.TIPO` (terciario / secundario) y la elección "Juntas" (una hoja) o
  "Separadas" (una hoja por comisión).

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| Menú `Ministerio › Comisiones al ministerio` | `NavMenu` grupo **Ministerio** → `/ministerio/comisiones` (`ComisionesMinisterio.razor`) | Carrera por autocomplete (permisos del usuario), cuatrimestre, comisión opcional, opciones de nómina y modo de hojas del Excel. |
| `ImprimirClick` → `SqlComi` (CUTUCO distintos de COMARM) | `IPadronMinisterioQuery.ObtenerComisionesAsync` | Las hojas de la nómina son las comisiones armadas, aunque no tengan alumnos (como el legacy). |
| `ImprimirClick` → `SqlDatos` (CURSADA ⨝ ALUMNOS, CURSANDO/RECURSANDO, BAJA='N') | `IPadronMinisterioQuery.ObtenerAlumnosAsync` (compartida con el Excel) | Una sola consulta cuyas columnas son el superconjunto de los dos SELECT legacy. `CUA_ANIO` normalizado sin barra en ambas (el legacy lo quitaba solo para COMARM). |
| `DOCUM` (SUBSTRING por posiciones de COD_ALU), `EDAD` (`/365`) | `CodificacionMinisterio.DocumentoParaNomina` / `.Edad` | Edad por calendario (diferencia deliberada: el `/365` se corría un día por cada bisiesto vivido). |
| Dibujo GDI sobre Gnostice (encabezado, nómina, línea + "RECURSANTES") | `NominaMinisterioPdfService` (QuestPDF, A4) | Membrete = mismo JPG de las constancias (`Institucion:MembreteConstanciaPath`); margen superior del comando (default 4 cm); letra un punto menor con edad/nacionalidad, como el original. |
| `BitBtn1Click` rama `TIPO='TER'` (IIF/CASE de orientación, modalidad P/D, turno, año, cuatrimestre, división, CR, título) | `CodificacionMinisterio` + `PadronMinisterioMapper.Fila` | Lógica pura en dominio con tests. TIPOCARRE siempre 'G'. |
| `BitBtn1Click` rama secundaria (modalidad C/A, año con caso 650, subselects de TUTORES) | ídem + subselects `FIRST 1` en la query | Los escalares del legacy fallaban en Firebird con más de un tutor cargado; se toma el primero por `FPARENT` (MADRE < PADRE < TUTOR), que es lo que el `ORDER BY` original insinuaba. |
| `Exportar_Excel_DS` ("Juntas") / `Exportar_Excel_DS_Hojas` ("Separadas", hoja = `CARRE-CUTUCO`) | `PadronMinisterioExcelService` (ClosedXML) | Mismos títulos de columna que recibía el Ministerio (con acentos corregidos: NÚMERO, MAÑANA, COMÚN, AÑO). Columnas siempre vacías (PISO, COMUNA/PARTIDO, PROVINCIA, PPI 1–3) se escriben en blanco. |
| `CustomMessageDialog('Juntas'/'Separadas')` | `MudSelect` "Comisiones: Juntas / Separadas" | La pregunta modal pasa a ser una opción previa al export. |
| `Carreras.CarreIndexOf(VCarrera).tipo` (global) | `IPadronMinisterioQuery.ObtenerCarreraAsync` | TIPO, RESOLUCION, DESCARRE y DISTANCIA de CARRERA por consulta, sin estado global. |

## 3. Decisiones

- **TUTORES no se modela como entidad**: solo se lee (subselects Dapper) para el adulto
  responsable del layout secundario. El ABM de tutores (`Tutores.pas`) sigue sin migrar;
  cuando se encare, la entidad nace en ese hito.
- **División A–F**: el CASE legacy cortaba en D y devolvía NULL para la 5ª y 6ª
  comisión, contradiciendo el mapa 1=A … 6=F confirmado en el hito 14 (`CodigoComision`).
  Se usa el mapa confirmado; el test de equivalencia extiende el SELECT de referencia con
  E/F.
- **Edad por calendario** en vez de `/365` (ver trazabilidad).
- **Orientación y título vacíos → null**: el `IIF` de ORIENTACION y la concatenación de
  TITULO devolvían `''`; en Excel la celda queda igual de vacía.
- **Sin zip**: a diferencia de las carpetas por comisión, el legacy generaba un solo libro
  (una hoja o una hoja por comisión), así que el endpoint devuelve un único `.xlsx`.
- **Sin política de autorización propia**: `[Authorize]` a secas como el resto de las
  pantallas; el enforcement por `MNUOPC` es del hito 12.3.

## 4. Verificación (checklists de `migration_prompts.md`)

- **1.B**: SQL parametrizado (sin valores de usuario interpolados) ✅ · `Async` +
  `CancellationToken` ✅ · sin paginación (reporte completo, como el legacy) — n/a.
- **2.A**: trazabilidad ✅ · un validador por comando (`GenerarNominaMinisterioValidator`,
  `ExportarPadronMinisterioValidator`) y `Result<T>` ✅ · sin escritura (solo lectura) ✅ ·
  sin globales (carrera por parámetro, fecha por `TimeProvider`) ✅.
- **3.A/3.B**: cero SQL/Infrastructure en el `.razor` ✅ · `MudGrid` con breakpoints ✅ ·
  ruta propia + `[Authorize]` + entrada de menú ✅ · no usa `EsbaListView`: no es un
  listado en pantalla sino la generación de dos archivos, como las carpetas por comisión
  (justificación equivalente a la del hito 7).
- **4.A/4.B**: `CodificacionMinisterioTests` (dominio, 15 casos parametrizados),
  `GenerarComisionesMinisterioHandlerTests` (validación, carrera inexistente, sin datos,
  agrupación/recursantes, propagación de `CancellationToken`, ambos layouts),
  `PadronMinisterioExcelServiceTests` + `NominaMinisterioPdfServiceTests` (sin base) y
  `PadronMinisterioEquivalenciaTests` (Category=Integration: nómina, padrón terciario y
  padrón secundario contra el SQL legacy sobre la base local) — todos en verde el
  2026-09-29.

## 5. Pendientes / deuda

- El membrete de la nómina reutiliza el JPG A4 de las constancias; si el Ministerio
  requiere otro papel, agregar `Institucion:MembreteNominaPath`.
- `ORIENTACION` solo distingue tecnicaturas ('TC'); el título de columna legacy
  mencionaba 'TS' (técnico superior), valor que el SQL nunca generaba. A confirmar con el
  usuario si corresponde ampliar el mapa.
