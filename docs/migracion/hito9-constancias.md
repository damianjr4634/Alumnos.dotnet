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
