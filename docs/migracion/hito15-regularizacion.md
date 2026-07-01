# Hito 15 — Regularización de materias (todas las variantes)

**Estado:** ✅ — terciarias ✅ 2026-06-30; bachillerato, secundario 333/650 y CNA ✅
2026-07-01, con equivalencia de condición y de commit contra los SP. Queda como deuda
transversal el prefill de faltas (`XXX_CONT_FALTAS`) y a confirmar la carrera `197916`.
**Etapas cubiertas:** 1 (queries), 2 (dominio + handler + validador), 3 (páginas + menú/acción),
4 (tests unitarios + equivalencia).
**Documentos rectores:** `migration_improvements.md` §1.2, §1.3, §2.1, §2.3, §2.4.
**Decisiones (2026-06-30):** portar a C# como el hito 14 (sin staging); arrancar por terciarias.

## 1. Alcance

Carga de las **notas del cursado** (2 parciales + recuperatorio + faltas) de un alumno en
una materia **terciaria** y resolución de su **condición** (REGULAR / PROMOCIONA / FINAL /
RECURSA / LIBRE / REINCORPORA / se mantiene CURSANDO). Dos variantes, como el legacy:
- **Por alumno** (`/alumnos/{c}/{cod}/regularizacion`, acción del buscador) — sucesora de
  `RegularizacionDeMaterias_nuevo.pas`.
- **Por comisión, en lote** (`/academica/regularizacion-comision`, menú Alumnos › Regularización)
  — sucesora de `RegularizacionDeMateriasXComision_nuevo.pas`.

Cierra la cadena académica: alta → inscripción → cursado/asistencias → **regularización** →
permiso → final → analítico.

## 2. Trazabilidad legacy → .NET

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `XXX_REGULARIZACION_MAT_TERC` (condición) | `CalculoCondicionRegularizacionTerciaria` (Domain) | Ladder TP_EVA/TP_EVA2/RECUP + ajuste por faltas + PROMOCIONA/FINAL por flags de MATERIAS y umbral `Regula_NotPromocion`. Portado 1:1, con equivalencia. |
| Rama TER de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.ConfirmarTerciariaAsync` | UPDATE CURSADA; si aprueba directo (PROMOCIONA/FINAL) → CURSADA_HST + DELETE + ANALITIC, en una transacción. Nota al analítico: promedio (PROMOCIONA) o recuperatorio/promedio (FINAL). |
| Staging `"$$$CURSADA"` (por usuario) | Estado del componente `RegularizacionEditor` | Se erradica el estado global (§2.3), como `"$$$PERMEXA"` en el hito 14. |
| INSERT ... SELECT a `"$$$CURSADA"` | `RegularizacionQuery.ObtenerPorAlumnoAsync` / `ObtenerPorComisionAsync` | SQL parametrizado; por comisión excluye CONDICION='REGULAR' y bajas. |
| `ValidoGrabaciondeMateria` + commit | `ConfirmarRegularizacionHandler` | Valida, resuelve condición (dominio, autoridad del servidor) y vuelca. `Result<T>`. |
| Grilla editable + tabs por carrera | `RegularizacionEditor.razor` (compartido) + páginas por alumno / por comisión | Solo markup/binding; preview de condición en vivo con el dominio. |
| Menú `Alumnos › Regularización` (deshabilitado) | Habilitado → por comisión; acción "Regularización" en el buscador → por alumno | |

## 3. Decisiones y particularidades

- **CUA_ANIO normalizado** (CHAR(3) "124"): se quita la barra que el operador tipee, en query
  y commit (misma corrección que en actas).
- **Centinela `99`** = parcial ausente/no rendido; nota válida = vacía, 1..10 o 99.
- **TOT_HORAS = 0** ⇒ el SP fuerza la condición al fallback (CURSANDO): el port lo replica.
- **INDICE de CURSADA_HST** lo pone su trigger (no se inserta); `LOG_*` los escriben triggers AFTER.
- **Autoridad del servidor**: la condición la calcula el handler, no el cliente (§2.7).

## 4. Bachillerato (incremento 2, 2026-07-01)

Alcance: la carrera **`BAC`** (única para la que el legacy corre el ladder de notas —
`_POSTVAL` tiene guard `CARRE='BAC'`). CNA (`TIPO=BAC`), `197916` (`TIPO=BAD`) y el
secundario `333/650` quedan diferidos: muestran el aviso "se migra en un incremento posterior".

Bachillerato es un flujo de **dos fases** que el dominio fusiona (sin staging):
1. **Faltas** (`XXX_REGULARIZACION_MAT_BAC`): `%inasist = round(INASIST×100/TOT_HORAS)`
   → ≤25% sigue, 26-40% **CONSEJO**, >40% **LIBRES**; rescate a RECURSANDO por tabla `RECURSA`.
2. **Notas** (`XXX_REGULARIZACION_MAT_POSTVAL`): ladder sobre 2 bimestres + promedio (TP_EVA3)
   + recuperatorio + nota "a regular" → REGULAR / A/REGULAR / PREVIO / LIBRES / CONSEJO + nota
   definitiva (FINAL1). **Interactivo**: ante CONSEJO devuelve las opciones Consejo/Regular/Libre
   (FBUTTONS) y recalcula con `PASO`.

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `_BAC` + `_POSTVAL` (condición) | `CalculoCondicionRegularizacionBachiller` (Domain) | Fusión faltas + ladder + PASO. Devuelve `RequiereDecision` ante CONSEJO. Portado 1:1, con equivalencia. |
| Rama BAC de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.ConfirmarBachilleratoAsync` | UPDATE CURSADA (con REGULAR/FECHA1/FINAL1); si queda REGULAR → CURSADA_HST + DELETE + ANALITIC (nota FINAL1, fecha FECHA1, CONDICION='REGULAR'). |
| `PASO`/`FBUTTONS` (diálogo por fila) | Selector inline Consejo/Regular/Libre en `RegularizacionEditorBachiller` | Confirmar se bloquea hasta resolver las filas en CONSEJO (mejor para el lote, §3.4). |
| `BtnLibre` (override manual) | Acción "Libre" por fila (solo CURSANDO/RECURSANDO) → `ForzarLibre` | Fuerza CONDICION=LIBRE con notas/horas en 99, sin ladder. |
| Router `XXX_REGULARIZACION_MAT` por `CARRERA.TIPO` | Branch en las páginas (`CARRE='BAC'` → editor bachillerato) | |

**Particularidades:** el ladder de faltas usa **solo INASIST** (no JUSTIF+INASIST como
terciarias); la rama BAC del commit **no persiste TP_EVA3/PROM** en CURSADA (se replica el
legacy); el analítico lleva la condición real (`REGULAR`) y la fecha del operador (FECHA1),
no la de TBL_CUAT.

## 4.bis Secundario 333/650 (incremento 3, 2026-07-01)

Alcance: los planes **333 y 650** (`CARRERA.TIPO ∈ {BAC,BAD}` con `CARRE IN ('333','650')`;
el router los manda a `_333`). Régimen **trimestral** (3 trimestres) con exámenes de
diciembre y marzo; **sin faltas ni CONSEJO**.

La lógica **activa** de `XXX_REGULARIZACION_MAT_333` (el bloque PROM/DICIEMBRE/MARZO estaba
comentado) decide por el **2° trimestre** (`TP_EVA2`): ≥6 → REGULAR (nota = 2° trim); si no
alcanza (o ambos trimestres ausentes) evalúa **diciembre** y **marzo** → REGULAR / PREVIA /
ENPROCESO; si no, mantiene la condición de origen. La nota al analítico (`NOTAFIN`) es la del
2° trimestre, diciembre o marzo, con su fecha (`NOTAFIN_FECHA`).

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `XXX_REGULARIZACION_MAT_333` (condición) | `CalculoCondicionRegularizacion333` (Domain) | Port 1:1 de la lógica activa + flag `FaltaFecha` (= FERRCOD=2: dic/mar aprueban sin su fecha). |
| Rama 333/650 de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.Confirmar333Async` | UPDATE CURSADA (3 trim + dic/mar + fechas); si REGULAR → CURSADA_HST (con CONDANT, dic/mar, NOTAFIN) + DELETE + ANALITIC (nota NOTAFIN, fecha NOTAFIN_FECHA). |
| Botón "A previa" (333/650) | Acción "A previa" por fila → `ForzarPrevia` | Fuerza CONDICION=PREVIA y marca marzo pendiente (NOTAMAR=99), sin ladder. |

**Particularidades:** un diciembre en 99 (99 ≥ 6) queda REGULAR con nota 99 — quirk del SP,
replicado. La columna PROM y otras de pass-through no se re-escriben en el UPDATE (el
operador no las edita en esta pantalla); la equivalencia lo confirma poblando el staging
con los valores actuales de CURSADA.

## 4.ter CNA (incremento 5, 2026-07-01)

Alcance: la carrera **`CNA`**. Es la variante más simple: **no usa ningún SP** de
condición — el formulario legacy (`GrabaMateriaCNAClick`) la decide en el cliente por la
**nota final** que carga el operador. El volcado usa la **rama BAC** del commit (CNA es
`CARRERA.TIPO='BAC'`).

| Legacy | Artefacto .NET | Notas |
|---|---|---|
| `GrabaMateriaCNAClick` (condición client-side) | `CalculoCondicionRegularizacionCna` (Domain) | nota ≥ 7 → REGULAR; ≥ 1 → RECURSA; si no → CURSANDO. Fecha obligatoria (validación). |
| Rama BAC de `XXX_REGULARIZACION` (commit) | `RegularizacionRepository.ConfirmarCnaAsync` | UPDATE CURSADA (solo FINAL1/FECHA1/CONDICION, como el .pas); si REGULAR → CURSADA_HST + DELETE + ANALITIC (nota FINAL1, fecha FECHA1). Reusa el histórico/analítico de BAC. |

**Nota:** `197916` (`TIPO=BAD`) queda **fuera de alcance**: su solapa/flujo no es deducible
del código (el router lo manda a `_BAC` de faltas, pero no corre `_POSTVAL`, con lo que nunca
regularizaría por notas). Requiere confirmación funcional antes de migrarlo.

## 5. Verificación

- `dotnet build` → **0 warnings**.
- `dotnet test` → **567 verdes**: Domain 205 (terciario +16, bachillerato +16, secundario +8,
  CNA +9), Application 262 (terciario +5, bachillerato +7, secundario +5, CNA +4), Integration
  100 (condición terciario/bachillerato/secundario + equivalencia del commit terciario/BAC/333/CNA).
- Equivalencia de **condición** (Firebird real):
  - `CalculoCondicionRegularizacionTerciaria` vs `XXX_REGULARIZACION_MAT_TERC` (6 escenarios).
  - `CalculoCondicionRegularizacionBachiller` vs `XXX_REGULARIZACION_MAT_BAC` + `_POSTVAL`
    (9 escenarios de notas/faltas + CONSEJO + CONSEJO/Regular), poblando `"$$$CURSADA"`
    con los derivados TP_EVA3/FINAL1 y comparando condición y nota final.
  - `CalculoCondicionRegularizacion333` vs `XXX_REGULARIZACION_MAT_333` (7 escenarios de
    2° trimestre / diciembre / marzo), comparando condición, NOTAFIN y su fecha.
- Equivalencia del **commit** (`RegularizacionCommitEquivalenciaTests`, 2026-07-01): corre cada
  volcado por dos caminos (SP `XXX_REGULARIZACION` sobre `"$$$CURSADA"` vs seam C#
  `ConfirmarFilas*Async` directo), cada uno en su transacción revertida, y compara el efecto en
  CURSADA/CURSADA_HST/ANALITIC. Cubre: terciaria PROMOCIONA (materia `561/16`), bachillerato
  REGULAR, bachillerato no-aprobado (update-only), secundario 333/650 REGULAR (materia `650`)
  y CNA REGULAR (rama BAC).
  - **Hallazgo corregido:** la rama TER de `XXX_REGULARIZACION` deja `CURSADA_HST.CONDANT` en
    NULL (a diferencia de la rama BAC, que sí guarda la condición previa) — el port del
    incremento 1 la escribía. Se alineó `RegularizacionRepository` al SP para lograr paridad.
  - **Bug latente corregido:** `SqlFechaPromocion` truncaba el parámetro `@CuaAnio` (Firebird lo
    inferí­a más corto que el código de 3 chars dentro de `SUBSTRING`); se tipó con `CAST(... AS
    VARCHAR(10))`. Afectaba al volcado terciario a analítico (nunca antes ejercido contra la base).

## 6. Pendiente (deuda transversal)

- **Autocompletado de faltas** desde `XXX_CONT_FALTAS` (asistencias) cuando CURSADA está en 0:
  hoy las faltas se leen de CURSADA y el usuario las edita, en las ramas que las usan. // TODO-migrar prefill.
- **Carrera `197916`** (`TIPO=BAD`): flujo no deducible del código; a confirmar con el usuario.
