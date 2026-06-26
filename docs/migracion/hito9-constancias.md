# Hito 9.1 — Constancia del Alumno (texto) + infraestructura de reportes QuestPDF

**Estado:** ✅ 2026-06-15 (sub-commit 9.1; analítico tabular, examen final y equivalencias en 9.2+).
**Etapas cubiertas:** 1 (query CARRERA), 2 (wrappers SP + caso de uso), 3 (reporte + endpoint + página), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §1.3, §2.1, §2.3, §3.3, §3.5, §2.4.

## 1. Alcance

Primer **reporte QuestPDF con formato propio** del sistema nuevo y toda la
infraestructura asociada: servicio de reporte dedicado + endpoint que sirve el PDF
**inline** (sucesor de `Imprimir.pas`/`TGmPreview`, §3.3). Cubre la **Constancia del
Alumno de texto** en tres variantes: **CTT** (certificado de estudios en trámite),
**Pase** y **Analítico**, con el **membrete reproducido dentro del PDF**.

Fuentes legacy: `Formulario constancia alumno2/constanciaalumnos2.pas`
(`Impresion_Analitico_Pase`). Los SP `XXX_*` ya estaban versionados en
`db/schema/procedures/`.

### Diferido a 9.2+ (decisión 2026-06-15)
- **Constancia de Examen Final (CE)**: el legacy la dispara seleccionando una
  materia rendida desde la grilla de materias en pantalla; esa grilla la construye
  9.2 (analítico tabular). Marcado para 9.2.
- **Firmas/sello como imágenes** (`firma_secre.jpg`/`firma_recto.jpg`/`sello2.jpg`):
  por ser sensible y depender de assets ausentes, 9.1 imprime las firmas como texto
  (nombre + cargo desde `CARRERA`).
- **Analítico tabular** (grilla de materias) y **Equivalencias** (ABM + impresión
  bachiller/terciaria): 9.2 y 9.3.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `constanciaalumnos2.pas` botones BtnAnalitico/BtnPase/BitBtn3 | `ConstanciaAlumno.razor` (`/alumnos/{Carre}/{Codigo}/constancia`) | Radio CTT/Pase/Analítico + "ante quién" + membrete; solo binding (§2.1). |
| `Impresion_Analitico_Pase` (dibujo GDI) | `GenerarConstanciaAlumnoHandler` + `ConstanciaPdfService` (QuestPDF) | Handler compone el texto; el reporte solo maqueta. |
| `XXX_IMPRIME_CTT` | `ICertificadoEnTramiteProcedure` (2.B) | Valida CTT **y** Analítico. FERRCOD 2→Error, 1→NeedsConfirmation (título intermedio, lleva FCUATRI), 0→Ok. |
| `XXX_IMPRIME_PASE` | `IPaseAlumnoProcedure` (2.B) | Valida Pase. FERRCOD 2→Error, 0→Ok. |
| `XXX_PARRAFO_CONSTANCIA` | `IParrafoConstanciaProcedure` (2.B) | Cuerpo del documento según TIPO ('CTT'/'PASE'/'ANALITICO'). |
| `XXX_CONSTANCIA_TERCIARIA` (IBConstancia) | `IConstanciaMateriasProcedure` (2.B) | Detalle de materias para "Materias que adeuda"; se reusa en 9.2 para la grilla. |
| `FuncionesConfiguracion.Rector/Secretaria` | `ConstanciasQuery.ObtenerDatosCarreraAsync` → `CARRERA` | En el legacy estos globales se cargaban de CARRERA; ahora se leen directo. |
| Cálculo "Materias que adeuda" (loop Cont=5/6 → "TODAS LAS") | `MateriasAdeudadasCalculator` (Domain, con tests) | Lógica de presentación de dominio (§2.1.3). |
| `LetrasCuat`/`MesALetras` | `TextoCastellano` (Domain) | ⚠️ Definiciones legacy no estaban en el repo; reconstruidas (meses coinciden con el SP). |
| `LoadPlantilla('membrete_con_direccion')` (jpg/wmf) | `ConstanciaPdfService` membrete + `InstitucionSettings` (Options) | El `.wmf` no es usable directo; header data-driven (`appsettings`) + logo opcional. |
| `TGmPreview`/`Imprimir.pas` | endpoint `GET /constancias/alumno` → `Results.File(...,"application/pdf")` inline | El navegador es el visor (§3.3). |

## 3. Decisiones

- **Reporte con formato propio aparte del export tabular**: `IConstanciaReportService`
  (nuevo) ≠ `IPdfExportService` (volcado de listados del hito 5). Sienta el patrón
  para todos los reportes maquetados siguientes.
- **PDF inline por endpoint, no descarga**: a diferencia del export de listados (que
  usa JS interop para descargar), la constancia se abre en pestaña nueva y la
  previsualiza el navegador (`Content-Disposition: inline`).
- **Servidor autoritativo**: la página corre `ValidarAsync` para el diálogo de
  confirmación (título intermedio), pero el endpoint **re-valida** en `GenerarPdfAsync`
  antes de renderizar (§2.7); `conf` solo saltea el `NeedsConfirmation`, nunca un Error.
- **`FCUATRI` viaja en el `Result<int>` del wrapper CTT**: incluso en `NeedsConfirmation`
  (no se usa `Result.DesdeErrCod` porque ese factory no lleva valor en ese estado), para
  que el cálculo de adeudadas pueda topear por el título intermedio.
- **Membrete parametrizado** (`InstitucionSettings` en `appsettings`, §2.3): nada
  hardcodeado. Pendiente del usuario: logo (PNG/JPG) + dirección para fidelidad final.

## 4. Verificación

- `dotnet build` → **0 warnings** (Nullable + TreatWarningsAsErrors).
- `dotnet test --filter Category!=Integration` → verdes: Domain (calculator: NINGUNA,
  lista, "TODAS LAS", agrupación, tope, condiciones aprobadas, nota 0; TextoCastellano),
  Application (`GenerarConstanciaAlumnoHandler`: validación, ERRCOD CTT 2→Error /
  1→NeedsConfirmation / confirmado→render, Pase 2→Error / Ok→render, carrera inexistente).
- Integration (Firebird real) → **4 verdes**: equivalencia wrapper vs SP directo para
  `XXX_IMPRIME_CTT`, `XXX_IMPRIME_PASE`, `XXX_PARRAFO_CONSTANCIA`, `XXX_CONSTANCIA_TERCIARIA`.

## 5. Pendiente del usuario (no bloqueante)
- Logo + texto de dirección/encabezado del membrete para fidelidad pixel.

## 6. Próximos pasos (retomar acá)

**9.2 — Analítico tabular + Constancia de Examen Final**
- Reusar `IConstanciaMateriasProcedure` (ya creado) para la **grilla de materias en
  pantalla** (condición/nota/fecha/instituto, agrupada por cuatrimestre, con colores
  por condición — el SP ya devuelve COLOR/HTMLCOLOR) + promedio vía wrapper nuevo de
  `XXX_PROMEDIO_GRAL`.
- Reporte tabular "Constancia de materias aprobadas" (`BitBtn1Click` legacy): tabla por
  cuatrimestre + caja por página. Nuevo método en `IConstanciaReportService` o servicio
  hermano.
- **Constancia de Examen Final (CE)**: `XXX_PARRAFO_CONSTANCIA` con TIPO `'CE-<codmat>'`,
  disparada al seleccionar una materia rendida de la grilla (clic derecho legacy →
  acción de fila). El wrapper de párrafo ya soporta cualquier TIPO.
- **Firmas/sello como imagen**: incorporar `firma_secre.jpg`/`firma_recto.jpg`/`sello2.jpg`
  al `ConstanciaPdfService` (modo "definitivo") cuando se provean los assets; hoy van como
  texto. Ver §1 "Diferido".

**9.3 — Equivalencias**
- ABM de equivalencias (`Equivalencia.pas`) + impresión equivalencia bachiller
  (`lst_impresion_equivalencia_bac.pas`) y terciaria (`lst_impresion_equivalencia_terc.pas`,
  wrapper `XXX_CONSTANCIA_TERCIARIA` ya existe). Plantillas legacy en
  `Esba.Delphi XE2/Plantillas/Equivalencia *`.

**Constancia de Alumno Regular** (`constanciaalumnoregular.pas`): envía la constancia por
mail → depende de MailKit (hito 10, correo). Coordinar con ese hito.

**Cierre del hito 9**: marcar la fila 9 en `CLAUDE.md` con ✅ + fecha recién cuando 9.2/9.3
estén entregados (9.1 es sub-commit; la fila queda ⬜ por ahora).

---

# Hito 9.2a — Grilla del analítico + promedio general

**Estado:** ✅ 2026-06-16 (sub-commit 9.2a; reporte CMA en 9.2b, examen final en 9.2c).
**Etapas cubiertas:** 1+2 (wrapper SP de promedio), 2 (caso de uso de lectura), 3 (grilla en
la página de constancia), 4 (tests unitario + equivalencia).

## 1. Alcance y decisiones (2026-06-16)

Primera pieza de 9.2: la **grilla de materias del alumno** (analítico) con su condición
agrupada por cuatrimestre y coloreada, más el **promedio general** en pantalla. Sucesor de
`Constancia()`/`FormActivate` de `constanciaalumnos2.pas`.

Decisiones acordadas con el usuario para todo 9.2:
- **Ubicación**: se **extiende la página de constancia** existente
  (`/alumnos/{Carre}/{Codigo}/constancia`), espejando el form único del legacy, en vez de
  crear una página aparte.
- **Papel y firmas** (aplica a 9.2b/c): **A4 fija + firmas como texto** en todos los reportes
  (coherente con 9.1; normaliza la inconsistencia A4/Oficio del legacy). Firmas-imagen
  (`firma_secre`/`firma_recto`/`sello2`) diferidas a cuando haya assets (deuda hito 12).
- **Entrega**: sub-commits **9.2a / 9.2b / 9.2c**.

Hallazgo de mapeo importante: en el legacy el **promedio se muestra en pantalla, NO se imprime
en ningún reporte** → se ubica como dato del encabezado de la grilla, no en el PDF. Los colores
del SP (`COLOR`/`HTMLCOLOR`) **no** se replican: por §4.5 se deriva un color semántico del
campo `CONDICION`.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `XXX_PROMEDIO_GRAL` (campo `PromGral`) | `IPromedioGeneralProcedure` / `PromedioGeneralProcedure` (2.B) | Escalar `Result`-libre (`Task<decimal>`); el SP nunca falla y COALESCE a 0. |
| Grilla `DbMaterias`/`DBResto` (memTable `Mt` ← `XXX_CONSTANCIA_TERCIARIA`) | `MudTable` agrupada en `ConstanciaAlumno.razor` + `IConstanciaMateriasProcedure` (reusado de 9.1) | Una sola grilla responsiva (no dos espejo); columnas secundarias ocultas con `d-none d-md/lg-table-cell`. |
| `Constancia()` + cálculo de `PromGral` en `FormActivate` | `ObtenerAnaliticoAlumnoHandler` → `AnaliticoAlumnoModel { Materias, PromedioGeneral }` | Lectura pura, sin transacción; compone materias + promedio. |
| `DbMateriasDrawColumnCell` (color desde `Mt.COLOR`/`FONTCOLOR`) | `ColorCondicion()` (presentación en el `.razor`) | Color semántico del tema (§4.5), no el RGB del VCL; cubre condiciones TER/ADM/BAC-BAD. |
| `LetrasCuat(cuat)` en el encabezado de grupo | `TextoCastellano.CuatrimestreEnLetras` (reusado) | Encabezado de grupo "N Cuatrimestre". |

## 3. Verificación

- `dotnet build` → **0 warnings** (Nullable + TreatWarningsAsErrors).
- `dotnet test --filter Category!=Integration` → verdes (Application 112, +2 del handler; Domain 56).
- Integration (Firebird real) → **5 verdes** en `ConstanciasEquivalenciaTests` (+1: `XXX_PROMEDIO_GRAL`
  wrapper vs SELECT directo).
- Revisión adversarial (3 lentes): 2 hallazgos BAJA, sin bugs ni violaciones 🔴. Hallazgo 1
  (mapa de color incompleto para BAC/BAD) corregido. Hallazgo 2 abajo.

## 4. Deuda registrada

- ⚠️ **Mapa `ColorCondicion` duplicado** entre `ConstanciaAlumno.razor` y `CursadaAlumno.razor`
  (`d-none`… mismo concepto "condición de materia", criterios divergentes; este último sin
  `ToUpperInvariant` y con menos estados). Aún **no** viola la regla de tres (§2.1.4): hay 2
  apariciones. Al aparecer un 3er consumidor (p.ej. el reporte CMA de 9.2b), **consolidar** en
  un helper de presentación compartido y alinear `CursadaAlumno`.

## 5. Próximos pasos

- **9.2b**: reporte tabular "Constancia de Materias Aprobadas" (CMA) — nuevo
  `IConstanciaAnaliticoReportService` (QuestPDF), extender `ConstanciaMateriaDto` + el SELECT
  del wrapper con `ACTINT`/`ACTDEGP`/`EXIMDESC` (ramas equivalencia/eximido), botón en la
  sección del analítico, endpoint PDF inline. Re-verificar equivalencia tras extender el wrapper.
- **9.2c**: Constancia de Examen Final (CE) — método de examen final en
  `IParrafoConstanciaProcedure` (`'CE-'+codMat`), acción de fila sobre materias rendidas
  (validar condición elegible), reporte/endpoint.

---

# Hito 9.2b — Constancia de Materias Aprobadas (reporte tabular)

**Estado:** ✅ 2026-06-18 (sub-commit 9.2b; examen final en 9.2c).
**Etapas cubiertas:** 1 (extensión del wrapper), 2 (caso de uso de lectura + dominio de
presentación), 3 (reporte QuestPDF + endpoint + botón), 4 (tests unitarios + equivalencia +
smoke del PDF).

## 1. Alcance y decisiones

Reporte tabular "Constancia de Materias Aprobadas" (CMA), sucesor de `BitBtn1Click` de
constanciaalumnos2.pas. Vuelca **todo el analítico** (no solo aprobadas, fiel al legacy: el
título dice "Aprobadas" pero lista la grilla completa) en una tabla agrupada por cuatrimestre.
Coherente con la decisión de 9.2: **A4 + firmas como texto** (firmas-imagen diferidas a hito 12);
se **extiende la página de constancia** (botón en la sección del analítico), no una página aparte.

## 2. Trazabilidad legacy → .NET

| Legacy (`BitBtn1Click`) | Artefacto .NET | Notas |
|---|---|---|
| Columnas ACTINT/ACTDEGP/EXIMDESC del grid `Mt` | `ConstanciaMateriaDto` + SELECT de `ConstanciaMateriasProcedure` extendidos | Ramas equivalencia/eximido. Equivalencia re-verificada contra el SP. |
| Lógica por fila (anual / equivalencia-eximido / normal con guiones de "sin dato") | `ConstanciaMateriasAprobadasFormatter` (Domain, con tests) | Presentación de dominio (§2.1.3), como `MateriasAdeudadasCalculator`. Guiones GDI normalizados a "—" y "MATERIA ANUAL". |
| Encabezado "En Buenos Aires a los …" + `PonePuntos(CodigoAlumno)` | `GenerarConstanciaMateriasAprobadasHandler.ComponerIntroduccion` + `TextoCastellano.CodigoConPuntos` | `PonePuntos` reconstruido (no versionado, vivía en FuncionesText). |
| Dibujo GDI de la tabla sobre `TGmPreview` | `ConstanciaAnaliticoPdfService` (QuestPDF) + `IConstanciaAnaliticoReportService` | Tabla por cuatrimestre; grupo y "fila completa" con `ColumnSpan`. |
| Membrete + firmas (compartidos con la constancia de texto) | `ReporteConstanciaLayout` (helper Infra) | Extraído para no duplicar entre los dos reportes (futuros 9.2c/9.3). |
| `BitBtn1Click` (sin chequeo de negocio) | endpoint `GET /constancias/alumno/materias-aprobadas` → `Results.File(...,"application/pdf")` inline | El legacy emite directo; solo exige "ante quién". |

## 3. Verificación

- `dotnet build` → **0 warnings**.
- `dotnet test --filter Category!=Integration` → verde (Domain 71: +9 formatter, +6 `CodigoConPuntos`;
  +3 smoke del PDF sin trait Integration).
- Integration (Firebird real) → verde: nuevo `ConstanciaMaterias_ColumnasDeEquivalenciaCoincidenConSpDirecto`
  (ACTINT/ACTDEGP/EXIMDESC wrapper vs SP directo, fila por fila).

## 4. Deuda saldada

- ✅ **`ColorCondicion` consolidado** en `CondicionMateriaColor` (Web/Components/Shared), usado por
  `ConstanciaAlumno.razor` y `CursadaAlumno.razor`; se alineó la divergencia (`CursadaAlumno` no
  normalizaba a mayúsculas y le faltaban estados). Resuelve la deuda registrada en 9.2a §4.

## 5. Próximo paso

- **9.2c**: Constancia de Examen Final (CE), según el detalle del bloque 9.2a §5.

---

# Hito 9.2c — Constancia de Examen Final (CE)

**Estado:** ✅ 2026-06-18 (sub-commit 9.2c; cierra 9.2).
**Etapas cubiertas:** 2 (regla de dominio + caso de uso de lectura), 3 (acción de fila +
endpoint, reusa el reporte de texto de 9.1), 4 (tests unitarios).

## 1. Alcance y decisiones

Constancia de Examen Final de una materia, sucesor de `Impresion_Constancia_Examen` de
constanciaalumnos2.pas (disparada por el item de popup "Imprimir permiso" sobre una fila de la
grilla). El legacy la invoca con `('CONSTANCIA DE EXAMEN FINAL', '', 'CE', true)`: **Query vacío
⇒ no hay SP de validación**, la única regla es que la condición de la materia sea elegible.

El cuerpo (título + "La Dirección del Instituto:" + párrafo + "Para ser presentada ante…" +
nota legal + firmas) es una variante más simple de la constancia de texto de 9.1, así que **se
reusa `ConstanciaPdfService`** (se hizo opcional el bloque "DATOS CORRESPONDIENTES / materias que
adeuda": no se imprime cuando `MateriasQueAdeuda` viene null). A4 + firmas como texto, coherente
con el resto de 9.2.

## 2. Trazabilidad legacy → .NET

| Legacy (`Impresion_Constancia_Examen`) | Artefacto .NET | Notas |
|---|---|---|
| Guarda de condición (`* ADEUDA *`/`CURSANDO`/`RECURSANDO`/`EQUIVALENCIA`/`PREVIA` ⇒ no imprime) | `ConstanciaExamenFinal.EsCondicionElegible` (Domain, con tests) | Regla de dominio; comparación case-insensitive. |
| `XXX_PARRAFO_CONSTANCIA(codAlu, carre, 'CE-'+codMat)` | `IParrafoConstanciaProcedure.ObtenerAsync(..., "CE-"+codMat)` | El wrapper de párrafo (9.1) ya soporta cualquier TIPO. |
| Item de popup "Imprimir permiso" sobre la fila del grid | Acción de fila (`MudIconButton`) en la grilla del analítico, visible solo si la condición es elegible | La elegibilidad se **re-valida en el servidor** (§2.7). |
| Layout (título/párrafo/cierre/firmas, sin "materias que adeuda") | `GenerarConstanciaExamenFinalHandler` → `ConstanciaAlumnoModel` con `MateriasQueAdeuda=null` → `ConstanciaPdfService` (reuso) | `ConstanciaPdfService` ahora omite el bloque de adeudadas cuando está vacío. |
| `FuncionesConfiguracion.Secretaria/Rector` | `CarreraConstanciaDto` (CARRERA) | Igual que 9.1. |
| `TGmPreview` | endpoint `GET /constancias/alumno/examen-final` inline | §3.3. |

## 3. Verificación

- `dotnet build` → **0 warnings**.
- `dotnet test` → verde: Domain 84 (+13 `ConstanciaExamenFinal`), Application 116 (+4 handler:
  rendida→genera CE, condición no elegible→Error, materia inexistente→Error, sin "ante quién"→Error),
  Integration 75 (sin cambios).

## 4. Cierre de 9.2

9.2a/b/c entregados. **Falta 9.3 (Equivalencias)** y la Constancia de Alumno Regular (depende de
MailKit, hito 10) para cerrar el hito 9 completo: la fila 9 de `CLAUDE.md` §6 sigue ⬜.

---

# Revisión 2026-06-26 — colores del SP + membrete obligatorio (commit `0e4eea4`)

Ajustes pedidos por el usuario sobre el hito 9 ya cerrado. **Revierten dos decisiones
documentadas más arriba**; se dejan registradas acá, no se borran las viejas.

## A. Colores del analítico: ahora del SP (supera 9.2a §1 y la fila `DbMateriasDrawColumnCell`)

- **Antes** (9.2a): el color de condición se derivaba como color **semántico del tema**
  (§4.5), descartando a propósito el RGB del SP ("los colores del SP `COLOR`/`HTMLCOLOR`
  **no** se replican").
- **Ahora**: el chip de la columna Condición se pinta con `HTMLCOLOR` (fondo) y
  `HTMLFONTCOLOR` (fuente) que devuelve `XXX_CONSTANCIA_TERCIARIA`. Fallback a los `TColor`
  enteros `COLOR`/`FONTCOLOR` (`0x00BBGGRR` → `#RRGGBB`) cuando el HTML viene vacío: la rama
  ADM del SP no setea los HTML y `TBL_COLOR.HTMLCODE` está NULL en la base de desarrollo. Si
  el SP no asigna color (fondo blanco "normal") cae al chip semántico anterior, para no romper
  el modo oscuro (§4.5).
- Artefactos: `ConstanciaMateriaDto` (+`HtmlColor`/`HtmlFontColor`/`ColorFondo`/`ColorFuente`),
  `ConstanciaMateriasProcedure` (SELECT ampliado), `ColorCondicionMateria` (Web/Components/Shared).
  `CursadaAlumno.razor` sigue con el chip semántico (`CondicionMateriaColor`): solo se cambió la
  constancia. Separador de cuatrimestre con tinte tenue de marca (`.esba-grupo-cuatrim` en `app.css`).

## B. Membrete obligatorio y unificado al JPG (supera 9.1 §3 y 9.2b membrete de texto)

- **Antes**: membrete opcional (checkbox `IncluirMembrete`, default true) y, en la constancia
  de alumno / materias aprobadas / equivalencia bachiller, era un **membrete de texto**
  data-driven (`ReporteConstanciaLayout.Membrete` + logo). Solo Constancia Regular y Resolución
  usaban el JPG `membrete_con_direccion.jpg` de fondo.
- **Ahora**: el membrete es **obligatorio** (se erradicó el flag `IncluirMembrete` de punta a
  punta: UI, endpoints, commands, models, handlers) y **todas** las constancias de impresión usan
  el mismo JPG de fondo. Nueva config `Institucion:MembreteConstanciaPath`. Se quitó el membrete
  de texto y el logo de `ReporteConstanciaLayout` (sin uso); cargador de fondo compartido `CargarFondo`.
- Pendiente del usuario 9.1 §5 ("logo + dirección del membrete para fidelidad") **queda saldado**:
  ya no aplica, el membrete es la imagen oficial.

## Verificación

- `dotnet build` → 0 warnings. `dotnet test` → verde (439).
- Render del JPG verificado con un PDF real del analítico (la imagen se embebe: PDF con fondo
  >20 KB mayor que sin fondo).
