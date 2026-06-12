# Etapa 1 — Mapeo del área Académica (MATERIAS, COMARM, CURSADA, ciclos lectivos)

> Fuentes: DDL real + `altamodifmaterias.pas`, `cargacomisiones.pas`,
> `InscripcionDeMaterias.pas`, `CargadeTrimestres.pas` y los triggers de CURSADA.
> ⚠️ = decisión dudosa a validar.

## Decisiones clave del área

- **CUTUCO** (3 dígitos) = **CU**atrimestre + **TU**rno + **CO**misión. La inscripción inserta
  `9TC` (900 + turno×10 + comisión) y el trigger `CURSADA_BI0` resuelve el `9` con
  `MATERIAS.CUATRIM`. Se modela como `short` con la semántica documentada (futuro VO si hace falta).
- **CUA_ANIO** `CHAR(3)` = cuatrimestre (1) + año (2): `"124"` = 1º/2024, mostrado "1/24".
- **CONDICION no es enum**: 14 valores reales con variantes históricas (`PREVIA`/`PREVIO`,
  `0LIBRES`, `P/EQUIVALEN`...). Queda `string` hasta sanear el dominio — un enum hoy rompería
  con datos reales. ⚠️ Propuesta futura: tabla de condiciones canónicas + normalización.
- **La lógica pesada de CURSADA vive en el trigger `CURSADA_BI0` y se conserva** (fase 1):
  `INDICE` por generador, denormalización de `APELLIDO`/`MATRIZ` desde ALUMNOS, resolución de
  CUTUCO, sincronización con `RECURSA` según condición, reseteo de finales desaprobados.
  `// TODO-migrar` para la fase 5.
- **LOG_CURSADA NO se mapea en EF**: la escribe un trigger AFTER (patrón LOG_ANALITIC) — desde
  .NET es solo lectura de auditoría y se hará por Dapper cuando exista la pantalla. Mapearla
  invitaría a escribirla por error.
- **RECURSA / CURSADA_HST**: pobladas por triggers, no se mapean en esta etapa.
- SP nuevo detectado para el futuro ABM de comisiones: `XXX_VALIDO_COMISION` (wrapper 2.B pendiente).
- **Sin commands en esta etapa** (desviación del Prompt 1.A, justificada): las escrituras del
  área pasan por validaciones de SP y reglas aún no migradas; los commands se definen junto a
  sus casos de uso en la Etapa 2 para no crear API muerta.

## MATERIAS → `Materia` (PK: CODMATERI+CODCARRE)

| Columna | Propiedad | Tipo | Notas |
|---|---|---|---|
| CODMATERI / CODCARRE | Codigo / CodigoCarrera | string | nav. `Carrera` sin FK física |
| DESCRIPCI / SIGLA | Nombre / Sigla | string? | |
| CUATRIM | Cuatrimestre | short? | cuatrimestre del plan; lo usa el trigger de CURSADA |
| EQUIVALE | CodigoEquivalencia | string? | ⚠️ |
| CORRELATIV / CORREFINAL | CorrelativasCursada / CorrelativasFinal | string? | códigos concatenados |
| LAB / ANUAL / PROMOCION | EsLaboratorio / EsAnual / AdmitePromocion | bool? | 'S'/'N' ⚠️ LAB y PROMOCION inferidos |
| ESTADO / CODANUAL / CODNEW / APRSFINAL / APTSFINAL | (string?) | | ⚠️ semántica a confirmar |
| ORDEN / USUARIO | Orden / Usuario | short? / string? | |

## COMARM → `Comision` (PK: CARRE+CUTUCO+COD_MAT+CUA_ANIO)

| Columna | Propiedad | Notas |
|---|---|---|
| CARRE / CUTUCO / COD_MAT / CUA_ANIO | CodigoCarrera / Cutuco / CodigoMateria / CuatrimestreAnio | nav. `Materia` |
| CODPROFES | CodigoProfesor | FK implícita a DOCENTES — `// TODO-migrar DOCENTES` |
| DIA1..3 / BLOQUE1..3 | Dia1..3 / Bloque1..3 | hasta 3 días/bloques de dictado |
| TIT_SUP | TitularSuplente | ⚠️ valores a confirmar |

## CURSADA → `Cursada` (PK: CARRE+COD_ALU+COD_MAT)

Mapeo 1:1 de las 50 columnas (ver `CursadaConfiguration`). Agrupada en: evaluaciones
(`TP_EVA*`→`Evaluacion1..3`, `RECUP*`→`Recuperatorio1..2`, `REGULAR`→`NotaRegular`,
`PROM`→`Promedio`, fechas y faltas por evaluación), asistencia (`TOT_HORAS`/`INASIST`/`JUSTIF`),
finales (`FINAL1..4`+`FECHA1..4`, `FACTFIN1..3`→`ActaFinal1..3` ⚠️, diciembre/marzo), origen y
equivalencias (`INSTITUT`/`CARAC`/`COLEGIO`/`PLAN`/actas ⚠️ `ACTINT`/`ACTDGE`/`ACTSNE`),
`A_C`/`DEFINE` ⚠️ sin semántica conocida, e `INDICE`/`ULTMOD` por trigger.
Nota: la PK no incluye CUA_ANIO → un alumno tiene UNA fila por materia (la recursada
histórica va a RECURSA vía trigger).

## TBL_CUAT / TBL_TRIM → `CicloCuatrimestral` / `CicloTrimestral` (PK: FANIO)

Fechas desde/hasta de cada cuatrimestre (2) o trimestre (3) por año lectivo.
`TBL_FERIADOS` queda para la etapa de Asistencias (la consume el cálculo de faltas).

## Verificación

- `dotnet build` 0 warnings; 5 smoke tests de integración nuevos materializando las cinco
  entidades contra la base real (incluye joins implícitos Materia→Carrera, Cursada→Materia,
  Comision→Materia) — en verde.
