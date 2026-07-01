# Hito 14 (cierre) — Actas de examen

**Estado:** ✅ 2026-06-30 (completa el hito 14; las notas de finales estaban ✅ 2026-06-26).
**Etapas cubiertas:** 1 (queries Dapper), 2 (handlers de lectura + validadores),
3 (reportes PDF/Excel + páginas + endpoints + navegación), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §1.3, §2.1, §2.3, §3.3, §3.5, §2.4.

## 1. Alcance

Las **cuatro** actas de examen del legacy (decisión 2026-06-30, las tres del hito +
la de exámenes cursando/recursando), en **PDF (hoja Oficio/Legal) + Excel**:

| Acta | Legacy | Fuente | Condición CURSADA | Filtros |
|---|---|---|---|---|
| Volante de mesa | `lstactasMesas.pas` | `XXX_MESAS_ALUMNOS` | (permiso de mesa) | mesa + tipo de examen |
| A/REGULAR | `lstactasARegular.pas` | COMARM + CURSADA | `A/REGULAR` | comisión?, cuatrim., materia? |
| Reincorporación | `lstactasreincorporacion.pas` | COMARM + CURSADA | `REINCORPORA` | comisión?, cuatrim., materia? |
| Exámenes | `lstactasexamenes.pas` | COMARM + CURSADA | `CURSANDO`/`RECURSANDO` | comisión?, cuatrim., materia? |

Las actas son **planillas volantes**: listan los alumnos elegibles con columnas de
calificación en blanco para que el tribunal las complete a mano (no cargan notas).

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `Copy(IntToStr(CUTUCO),1,1)` + `Turnos()`/`Division()` | `CodigoComision` (Domain.Examenes) | Descompone CUTUCO (3 díg.) = **CU**atrimestre + **TU**rno + **CO**misión + texto (ver §4). |
| Las 3 variantes por comisión (condición + título + EXISTS) | `TipoActaComision` (enum) + `ActaComisionDescriptor` | Centraliza condiciones, EXISTS de cabecera, título y "Correspondiente al cuatrimestre". |
| SqlComi/SqlDatos de `lstactasARegular`/`reincorporacion`/`examenes` | `IActasQuery.ObtenerCabecerasComisionAsync` + `ObtenerAlumnosComisionAsync` (Dapper) | SQL parametrizado; agrupación por comisión en el handler. |
| SqlComi + `XXX_MESAS_ALUMNOS` de `lstactasMesas` | `ObtenerCabeceraMesaAsync` + `ObtenerAlumnosMesaAsync` | El SP ya estaba versionado; `PERM_EXA` (VARCHAR) → `CAST ... AS INTEGER`. |
| `ImprimirClick`/`ExportExcel`/`BtnExcelClick` | `GenerarActaComisionHandler` / `GenerarActaMesaHandler` | Lectura pura; arma el modelo y delega en el servicio de PDF/Excel. Sin datos → `Result.Error` ("No hay datos para mostrar"). |
| Dibujo GDI sobre Gnostice (`actas_volantes.wmf`/`actas.wmf`, papel Legal) | `ActaPdfService` (QuestPDF, `PageSizes.Legal`) | Encabezado + grilla con columnas "Calificación"/"En letras" en blanco. Una comisión/mesa por página (PageBreak). |
| Plantilla `Planilla_de_actas_volantes.xls` (OLE) | `ActaExcelService` (ClosedXML) | Una hoja por comisión/mesa; cabecera + grilla. Sin Excel instalado (§3.5). |
| `TGmPreview`/`Imprimir.pas` | endpoints `/actas/comision`, `/actas/comision/excel`, `/actas/mesa`, `/actas/mesa/excel` | PDF inline; Excel con `Content-Disposition: attachment`. |
| Formularios VCL | `ActasComision.razor` (`/examenes/actas/comision`) + `ActasMesa.razor` (`/examenes/actas/mesa`) | Solo filtros + apertura del endpoint (§2.1). Menú "Exámenes" habilitado. |

## 3. Decisiones

- **CUA_ANIO normalizado**: el legacy comparaba `CUA_ANIO` **sin** barra en la cabecera
  (COMARM) pero **con** barra en el detalle (CURSADA), pese a que ambas columnas son
  CHAR(3) "124" — inconsistencia que solo funcionaba si el operador tipeaba sin barra.
  `ActasQuery` **quita la barra para ambas** consultas (corrige el bug latente).
- **Condición con TRIM**: el detalle legacy trimea `CONDICION`; el EXISTS de la cabecera
  no. Se unifica con `TRIM(...) IN @Condiciones` en ambas (más robusto ante padding CHAR).
- **PDF Oficio dibujado, no preimpreso**: a diferencia de las constancias (A4 + membrete
  JPG de fondo), las actas reproducen el papel volante Oficio con la grilla dibujada por
  QuestPDF (las plantillas `.wmf` legacy no son usables directo). Sin asset nuevo.
- **Tipos de examen por mesa**: misma lógica que la carga de notas de finales (TER →
  `FINAL`; resto → `LIBRES/PREVIOS/DICIEMBRE/MARZO/P/EQUIVALEN`), ya que ambas alimentan
  el mismo parámetro de `XXX_MESAS_ALUMNOS`.

## 4. Decodificación del CUTUCO (confirmada por el usuario, 2026-06-30)

`CUTUCO` = **CU**atrimestre + **TU**rno + **CO**misión (3 dígitos):
- **Cuatrimestre**: 1–6.
- **Turno**: 1 = Mañana, 2 = Tarde, 3 = Vespertino, 4 = Noche.
- **Comisión**: 1–6, mostrada como letra (1 = A … 6 = F). El acta legacy rotulaba este
  dígito como "División"; se corrigió a "Comisión".

Las funciones legacy `Turnos()`/`Division()` no estaban en el fuente versionado (solo en el
binario); el mapeo lo confirmó el usuario y vive en `CodigoComision`.

## 5. Verificación

- `dotnet build` → **0 warnings** (Nullable + TreatWarningsAsErrors).
- `dotnet test` → **489 verdes**: Domain 156 (+11 `CodigoComision`), Application 241
  (+8 handlers de acta comisión/mesa), Integration 92 (+2 equivalencia + 4 smoke PDF/Excel).
- Equivalencia (Firebird real): cabecera por comisión vs SELECT legacy; cabecera + alumnos
  de mesa vs `XXX_MESAS_ALUMNOS` directo (igualdad por valor de DTO).
