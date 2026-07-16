# Hito 7 — Asistencias

**Estado:** ✅ 2026-06-15
**Etapas cubiertas:** 1 (TBL_FALTAS), 2 (handlers + wrappers SP), 3 (pantallas), 4 (tests).
**Documentos rectores:** `migration_improvements.md` §1.3, §2.1, §3.3, §3.4, §2.4.

## 1. Alcance

Carga de inasistencias por comisión, planilla de reincorporaciones/libres y pase
de un alumno a LIBRE. Tres incrementos commiteados (1/3 modelo+lectura, 2/3 carga,
3/3 planillas+pase libre).

## 2. Hallazgo: TBL_FERIADOS no existe

El roadmap mencionaba `TBL_FERIADOS`, pero **no está en el esquema** ni el legacy
usa feriados. Los días habilitados para cargar faltas salen de los días de dictado
de la comisión (`COMARM.DIA1/2/3`). No se modeló ninguna pieza de feriados.

## 3. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `TBL_FALTAS` (tipos) | entidad `TipoFalta` + config + `TipoFaltasQuery` | Aplica por carrera: `CARRE IS NULL OR CONTAINING`. |
| `XXX_FALTAS_COMISION` | `IFaltasComisionProcedure` (2.B) | Alumnos de la comisión + acumulado. |
| `XXX_FALTAS_FALTAS` | `IFaltasAlumnoProcedure` (2.B) | Faltas cargadas de un alumno. |
| `CargaInasistenciasComisionNuevo.GrabamesaClick` | `InasistenciasRepository.ReemplazarFaltasComisionAsync` + `GuardarInasistenciasComisionHandler` | Delete+insert por (carrera, cutuco, materia, año) en una transacción (Dapper). |
| Calendario por paneles del form | `CargaInasistencias.razor` | Por alumno, lista editable de faltas con fecha restringida a los días de dictado + tipo. |
| `XXX_FALTAS_IMPRESI` | `IPlanillaInasistenciasProcedure` + `PlanillasAsistencia.razor` | Planilla de reincorporaciones/libres; reusa `EsbaListView` (grilla + export Excel/PDF). |
| `XXX_FALTAS_PASLIBRE` | `IPaseLibreProcedure` + `PasarMateriasALibreHandler` | Pase a LIBRE con dos fases (preview rollback + confirmar commit); botón en la cursada del alumno. |
| `lstplanasis.pas` ("Carpeta asistencia") + `lstNotasyPractico.pas` ("Carpeta de trabajos practicos") | `ICarpetaComisionQuery` + `GenerarCarpetaComisionHandler` (enum `TipoCarpetaComision`) + `CarpetaComisionPdfService` + `/asistencias/carpeta` (`?tipo=tp` para TP) | Agregado 2026-07-14 (fuera del alcance original del hito). Planillas en blanco por comisión, una hoja por comisión con cursantes + recursantes al pie: **asistencia** (A4: columna D/H partida, 25 días, INA/ANT/TOT apilados) y **trabajos prácticos** (Oficio: TP 1–5 con línea de fecha + condición, réplica de `trabajos_practicos.wmf`). Ambos formularios legacy compartían la nómina; SQL directo portado a Dapper (sin SPs). Diferencias deliberadas: se filtra `BAJA='N'` también en TP (la impresión legacy lo omitía pero su Excel lo aplicaba — descuido) y no se replicó el Excel legacy (asistencia: solo volcaba la grilla de comisiones, cubierta por el listado del hito 6; TP: el Excel legacy usaba la plantilla OLE `Planilla_de_notas.xls`). **"Planillas de profesores" agregada 2026-07-15** (`?tipo=profesores`): tercer `TipoCarpetaComision.PlanillaProfesores`, réplica de `Planilla_calificaciones.wmf` en Oficio (1er./2do bimestre con 5 notas + Prom., calificación Final/Recup./Def. y columna Notificado; espaciado legacy 1,03 cm ≈ 28 pt por renglón). **Export Excel agregado 2026-07-15** (`CarpetaComisionExcelService`, `/asistencias/carpeta/excel`): sucesor del BtnExcel de `lstNotasyPractico.pas` (OLE sobre `Planilla_de_notas.xls`) — como el legacy, **un archivo por comisión/materia** (`Notas_{CUTUCO}_{materia}.xlsx` / `TP_…`), apaisado con fit-to-width; con varias comisiones se descargan en un .zip (equivalente web del directorio de destino que elegía el usuario). Diferencia deliberada: el legacy exportaba siempre el formato calificaciones sin importar el menú de origen; acá la grilla acompaña al tipo (TP 1–5 + condición para trabajos prácticos, bimestres + calificación + notificado para planilla de profesores) y se agrega la columna Código (el legacy volcaba `COD_ALU` bajo el rótulo "Documento", descuido de la plantilla). La carpeta de asistencia sigue sin export (su Excel legacy solo volcaba la grilla de comisiones, cubierta por el listado del hito 6). |

## 4. Decisiones

- **FALTAS por Dapper, no EF**: su clave única (FALTAS_IDX1) incluye `COD_MAT`
  nullable (no admisible como clave EF) y la escritura es reemplazo masivo, no
  change-tracking. Acceso por Dapper con transacción explícita (§1.3).
- **Reemplazo acotado por (carrera, cutuco, materia, año)**: el legacy borraba por
  rango de meses del cuatrimestre, lo que exigía el chequeo hardcodeado 333/650 de
  modalidad. Como el CUTUCO ya identifica el cuatrimestre y CUA_ANIO el año, el
  scope por año es equivalente para datos válidos y elimina el hardcode.
- **`DateOnly` como parámetro Dapper**: Dapper no lo acepta de entrada (sí de
  salida); el insert convierte a `DateTime`.
- **Planilla sobre `EsbaListView`**: el reporte se pagina en memoria y reusa la
  grilla + exportación del hito 5 en vez de un layout QuestPDF dedicado (eso queda
  para el hito 9, primer reporte formateado).

## 5. Verificación

- `dotnet build Esba.slnx` → 0 warnings.
- `dotnet test` → **181 verdes** (Domain 37, Application 85, Integration 59):
  - Application: handlers de carga y de pase a libre (NSubstitute).
  - Integration (Firebird real): catálogo + wrappers de lectura; roundtrip de
    reemplazo de faltas (insert y borrado); planilla; preview de pase a LIBRE
    (rollback ⇒ CURSANDO sin cambios).
